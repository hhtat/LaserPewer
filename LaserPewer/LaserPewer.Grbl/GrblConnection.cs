using LaserPewer.Shared;
using System;
using System.Diagnostics;
using System.IO.Ports;
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

        private SerialPort serialPort;

        private readonly GrblRequestQueue pendingRequests;
        private GrblRequest pendingStatusQueryRequest;
        private bool pendingHomingRequest;

        private Thread receivingThread;

        public GrblConnection()
        {
            pendingRequests = new GrblRequestQueue();
        }

        public bool TryConnect(string portName)
        {
            if (serialPort != null) throw new InvalidOperationException();

            SerialPort newPort;

            try
            {
                newPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                newPort.ReadTimeout = 1000;
                newPort.Open();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

            serialPort = newPort;

            return true;
        }

        public void StartReceiving()
        {
            if (receivingThread != null) return;
            if (serialPort == null) throw new InvalidOperationException();

            receivingThread = new Thread(receiveLoop);
            receivingThread.Priority = ThreadPriority.AboveNormal;
            receivingThread.Start();
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
            if (request.ResponseStatus != GrblResponseStatus.Unsent)
            {
                throw new InvalidOperationException();
            }

            if (request.FireAndForget)
            {
                try
                {
                    request.Sent();
                    Console.WriteLine("SENT: " + request.Message);
                    serialWrite(request.Message);
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
                    request.Sent();
                    Console.WriteLine("SENT: " + request.Message);
                    serialWrite(request.Message);
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

        private void serialWrite(string data)
        {
            byte[] buffer = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                buffer[i] = (byte)data[i];
            }
            serialPort.Write(buffer, 0, buffer.Length);
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
                    line = serialPort.ReadLine();
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

                Console.WriteLine("RECV: " + line);
                line = line.Trim();

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
                        UnsupportedVersion?.Invoke(this, line);
                    }
                }

                LineReceived?.Invoke(this, line);
            }

            completeAll(GrblResponseStatus.Failure);

            Closed?.Invoke(this);
        }
    }
}
