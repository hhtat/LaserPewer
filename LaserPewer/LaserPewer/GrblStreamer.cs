using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace LaserPewer
{
    public class GrblStreamer
    {
        private static readonly int SEND_BUFFER = 128;

        public delegate void MessageReceivedEventHandler(object sender, string message);
        public event MessageReceivedEventHandler MessageReceived;

        private SerialPort serialPort;
        private StreamReader reader;
        private StreamWriter writer;

        private int pendingStatusRequests;
        private bool pendingHomingRequest;
        private Queue<int> pendingCommands;
        private int pendingCommandsBytes;

        public GrblStreamer()
        {
            pendingCommands = new Queue<int>();
        }

        public void Connect(string portName)
        {
            serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            serialPort.Open();
            reader = new StreamReader(serialPort.BaseStream, Encoding.ASCII);
            writer = new StreamWriter(serialPort.BaseStream, Encoding.ASCII);

            receiveLoop();
        }

        public void SendReset()
        {
            Debug.WriteLine("SENT: CAN");
            writer.Write((char)0x18);
            writer.Flush();
        }

        public bool TrySendStatusRequest()
        {
            if (!readyToSend(1)) return false;

            Debug.WriteLine("SENT: ?");
            writer.Write('?');
            writer.Flush();
            pendingStatusRequests++;

            return true;
        }

        public bool TrySendHomeRequest()
        {
            bool sent = trySendCommand("$H");
            if (sent) pendingHomingRequest = true;
            return sent;
        }

        public bool TrySendUnlockRequest()
        {
            return trySendCommand("$X");
        }

        public void SendHoldRequest()
        {
            Debug.WriteLine("SENT: !");
            writer.Write('!');
            writer.Flush();
        }

        public bool TrySendCommand(string line)
        {
            return trySendCommand(line);
        }

        private bool trySendCommand(string line)
        {
            if (!readyToSend(line.Length + 1)) return false;

            Debug.WriteLine("SENT: " + line);
            writer.Write(line);
            writer.Write('\r');
            writer.Flush();
            pendingCommands.Enqueue(line.Length + 1);
            pendingCommandsBytes += line.Length + 1;

            return true;
        }

        private bool readyToSend(int bytes)
        {
            return !pendingHomingRequest && bytesLeft() >= bytes;
        }

        private int bytesLeft()
        {
            return SEND_BUFFER - pendingStatusRequests - pendingCommandsBytes;
        }

        private async void receiveLoop()
        {
            while (true)
            {
                string line = await reader.ReadLineAsync();

                Debug.WriteLine("RECV: " + line);

                if (line.Length == 0) continue;

                if (line == "ok" || line.StartsWith("error:"))
                {
                    if (pendingCommands.Count > 0) pendingCommandsBytes -= pendingCommands.Dequeue();
                    if (pendingCommands.Count == 0) pendingHomingRequest = false;
                }
                else if (line.StartsWith("<"))
                {
                    if (pendingStatusRequests > 0) pendingStatusRequests--;
                }
                else if (line.StartsWith("Grbl "))
                {
                    pendingStatusRequests = 0;
                    pendingCommands.Clear();
                    pendingCommandsBytes = 0;
                }

                MessageReceived?.Invoke(this, line);
            }
        }
    }
}
