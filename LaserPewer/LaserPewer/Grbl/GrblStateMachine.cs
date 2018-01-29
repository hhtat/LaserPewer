using LaserPewer.Utilities;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace LaserPewer.Grbl
{
    public class GrblStateMachine
    {
        public MachineState State { get; private set; }

        private readonly object inputLock;
        private InputCommand _input;
        public InputCommand Input
        {
            get { return _input; }
            set { lock (inputLock) { _input = value; } }
        }

        private readonly Thread machineThread;
        private readonly ConcurrentQueue<string> receivedLines;

        private readonly StopWatch stopWatch;
        private bool resetDetected;

        private GrblConnection connection;

        public GrblStateMachine()
        {
            State = MachineState.Disconnected;

            inputLock = new object();

            machineThread = new Thread(machineThreadStart);
            receivedLines = new ConcurrentQueue<string>();

            stopWatch = new StopWatch();
            resetDetected = false;

            machineThread.Start();
        }

        private void machineThreadStart()
        {
            while (true)
            {
                processReceivedLines();

                switch (State)
                {
                    case MachineState.Disconnected:
                        if (handleConnectCommand()) break;
                        break;
                    case MachineState.Connected:
                        if (handleDisconnectCommand()) break;
                        if (handleResetCommand()) break;
                        break;
                    case MachineState.Resetting:
                        if (handleDisconnectCommand()) break;
                        resettingStateLogic();
                        break;
                    default:
                        throw new NotSupportedException();
                }

                Thread.Sleep(1);
            }
        }

        private InputCommand popInput(InputCommandType type)
        {
            InputCommand popped = null;

            lock (inputLock)
            {
                if (Input != null && Input.Type == InputCommandType.Connect)
                {
                    popped = Input;
                    Input = null;
                }
            }

            return popped;
        }

        private bool handleConnectCommand()
        {
            InputCommand command = popInput(InputCommandType.Connect);
            if (command == null) return false;

            GrblConnection newConnection = new GrblConnection();
            if (newConnection.TryConnect(command.Parameter))
            {
                clearReceivedLines();
                connection = newConnection;
                connection.LineReceived += connection_LineReceived;
                connection.StartReceiving();
                State = MachineState.Connected;
            }

            return true;
        }

        private bool handleDisconnectCommand()
        {
            InputCommand command = popInput(InputCommandType.Disconnect);
            if (command == null) return false;

            connection.LineReceived -= connection_LineReceived;
            connection.Disconnect();
            State = MachineState.Disconnected;

            return true;
        }

        private bool handleResetCommand()
        {
            InputCommand command = popInput(InputCommandType.Reset);
            if (command == null) return false;

            State = MachineState.Resetting;

            stopWatch.Zero();
            sendSoftResetRequest();

            return true;
        }

        private void resettingStateLogic()
        {
            if (resetDetected)
            {
                State = MachineState.Connected;
            }
            else if (stopWatch.Expired(TimeSpan.FromSeconds(0.5)))
            {
                sendSoftResetRequest();
            }
        }

        private void sendSoftResetRequest()
        {
            GrblRequest request = GrblRequest.CreateSoftResetRequest();
            if (connection.Send(request) && request.ResponseStatus == GrblResponseStatus.Silent)
            {
                resetDetected = false;
                stopWatch.Reset();
            }
        }

        private void clearReceivedLines()
        {
            string dummy;
            while (!receivedLines.IsEmpty)
            {
                receivedLines.TryDequeue(out dummy);
            }
        }

        private void processReceivedLines()
        {
            string line;

            while (!receivedLines.IsEmpty)
            {
                if (!receivedLines.TryDequeue(out line)) continue;

                if (line.StartsWith("Grbl "))
                {
                    resetDetected = true;
                }
            }
        }

        private void connection_LineReceived(GrblConnection sender, string line)
        {
            receivedLines.Enqueue(line);
        }

        public enum MachineState
        {
            Disconnected,
            Connected,
            Resetting,
        }

        public enum InputCommandType
        {
            None,
            Connect,
            Disconnect,
            Reset,
        }

        public class InputCommand
        {
            public readonly InputCommandType Type;
            public readonly string Parameter;

            public InputCommand(InputCommandType type, string parameter = null)
            {
                Type = type;
                Parameter = parameter;
            }
        }
    }
}
