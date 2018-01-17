using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace LaserPewer.Grbl
{
    public class GrblConnection
    {
        private const int TxRxBufferSize = 128;

        public delegate void ClosedEventHandler(GrblConnection sender);
        public event ClosedEventHandler Closed;

        public delegate void LineReceivedEventHandler(GrblConnection sender, string line);
        public event LineReceivedEventHandler LineReceived;

        public delegate void UnsupportedVersionEventHandler(GrblConnection sender, string welcomeMessage);
        public event UnsupportedVersionEventHandler UnsupportedVersion;

        public bool IsActive { get { return serialPort != null && serialPort.IsOpen; } }

        private readonly SynchronizationContext ownerContext;

        private SerialPort serialPort;
        private StreamReader reader;
        private StreamWriter writer;

        private readonly GrblRequestQueue pendingRequests;
        private GrblRequest pendingStatusQueryRequest;
        private bool pendingHomingRequest;

        private BackgroundWorker receivingWorker;

        public GrblConnection()
        {
            ownerContext = SynchronizationContext.Current;
            pendingRequests = new GrblRequestQueue();
        }

        public bool TryConnect(string portName)
        {
            if (serialPort != null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.None);
                serialPort.ReadTimeout = 1000;
                serialPort.Open();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

            reader = new StreamReader(serialPort.BaseStream, Encoding.ASCII);
            writer = new StreamWriter(serialPort.BaseStream, Encoding.ASCII);

            receivingWorker = new BackgroundWorker();
            receivingWorker.DoWork += receivingWorker_DoWork;
            receivingWorker.RunWorkerAsync();

            return true;
        }

        public void Disconnect()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public bool Send(GrblRequest request)
        {
            if (request.ResponseStatus != GrblResponseStatus.Pending)
            {
                throw new InvalidOperationException();
            }

            if (request.FireAndForget)
            {
                try
                {
                    writer.Write(request.Message);
                    request.Complete(GrblResponseStatus.Silent);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    request.Complete(GrblResponseStatus.Failure);
                }

                return true;
            }

            lock (pendingRequests)
            {
                if ((request.Message.Length > TxRxBufferSize - pendingRequests.CharacterCount) ||
                    (pendingStatusQueryRequest != null && request.Type == GrblRequestType.StatusQuery) ||
                    pendingHomingRequest)
                {
                    return false;
                }

                try
                {
                    writer.Write(request.Message);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    serialPort.Close();
                    request.Complete(GrblResponseStatus.Failure);
                    return true;
                }

                if (request.Type != GrblRequestType.StatusQuery)
                {
                    pendingRequests.Enqueue(request);

                    if (request.Type == GrblRequestType.Homing)
                    {
                        pendingHomingRequest = true;
                    }
                }
                else
                {
                    pendingStatusQueryRequest = request;
                }
            }

            return true;
        }

        private void completeQueuedRequest(GrblResponseStatus status, int? errorCode = null)
        {
            lock (pendingRequests)
            {
                if (!pendingRequests.IsEmpty)
                {
                    GrblRequest request = pendingRequests.Dequeue();

                    if (request.Type == GrblRequestType.Homing)
                    {
                        pendingHomingRequest = false;
                    }

                    if (!errorCode.HasValue)
                    {
                        request.Complete(status);
                    }
                    else
                    {
                        request.Complete(status, errorCode.Value);
                    }
                }
            }
        }

        private void completeStatusQueryRequest(GrblResponseStatus status)
        {
            lock (pendingRequests)
            {
                if (pendingStatusQueryRequest != null)
                {
                    pendingStatusQueryRequest.Complete(status);
                    pendingStatusQueryRequest = null;
                }
            }
        }

        private void completeAll(GrblResponseStatus status)
        {
            lock (pendingRequests)
            {
                while (!pendingRequests.IsEmpty)
                {
                    completeQueuedRequest(status);
                }

                completeStatusQueryRequest(status);
            }
        }

        private void receiveLoop()
        {
            while (serialPort.IsOpen)
            {
                string line;

                try
                {
                    line = reader.ReadLine();
                }
                catch (TimeoutException)
                {
                    continue;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    serialPort.Close();
                    break;
                }

                if (line == "ok")
                {
                    completeQueuedRequest(GrblResponseStatus.Ok);
                }
                else if (line.StartsWith("<"))
                {
                    completeStatusQueryRequest(GrblResponseStatus.Ok);
                }
                else if (line.StartsWith("error:"))
                {
                    int errorCode;
                    int.TryParse(line.Substring(6), out errorCode);
                    completeQueuedRequest(GrblResponseStatus.Error, errorCode);
                }
                else if (line.StartsWith("Grbl "))
                {
                    completeAll(GrblResponseStatus.Failure);

                    if (!line.StartsWith("Grbl 1.1"))
                    {
                        ownerContext.Send(state => UnsupportedVersion?.Invoke(this, line), null);
                    }
                }

                ownerContext.Send(state => LineReceived?.Invoke(this, line), null);
            }

            completeAll(GrblResponseStatus.Failure);

            ownerContext.Send(state => Closed?.Invoke(this), null);
        }

        private void receivingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            receiveLoop();
        }
    }
}
