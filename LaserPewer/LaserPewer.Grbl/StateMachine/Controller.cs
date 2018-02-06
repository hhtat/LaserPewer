using LaserPewer.Shared;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LaserPewer.Grbl.StateMachine
{
    public class Controller
    {
        private static readonly TimeSpan DefaultStatusQueryInterval = TimeSpan.FromSeconds(0.5);

        public readonly State DisconnectedState;
        public readonly State ConnectingState;
        public readonly State ReadyState;
        public readonly State ResettingState;
        public readonly State AlarmedState;
        public readonly State AlarmKillState;
        public readonly State HomingState;
        public readonly State JoggingState;
        public readonly State JogCancellationState;
        public readonly State RunningState;

        private GrblConnection _connection;
        public GrblConnection Connection
        {
            get { return _connection; }
            private set
            {
                if (_connection != null)
                {
                    _connection.Disconnect();
                    _connection.LineReceived -= connection_LineReceived;
                }

                _connection = value;
                resetConnectionState();

                if (_connection != null)
                {
                    _connection.LineReceived += connection_LineReceived;
                    _connection.StartReceiving();
                }
            }
        }

        public GrblProgram Program { get; private set; }

        public bool ResetDetected { get; private set; }
        public GrblStatus StatusReported { get; private set; }

        private readonly object queuedTriggerLock;
        private State.Trigger queuedTrigger;

        private readonly Queue<string> receivedLines;

        private readonly StopWatch statusQueryTimeout;
        private TimeSpan statusQueryInterval;
        private GrblRequest pendingStatusQueryRequest;

        private State currentState;
        private readonly Thread thread;

        public Controller()
        {
            DisconnectedState = new DisconnectedState(this);
            ConnectingState = new ConnectingState(this);
            ReadyState = new ReadyState(this);
            ResettingState = new ResettingState(this);
            AlarmedState = new AlarmedState(this);
            AlarmKillState = new AlarmKillState(this);
            HomingState = new HomingState(this);
            JoggingState = new JoggingState(this);
            JogCancellationState = new JogCancellationState(this);
            RunningState = new RunningState(this);

            queuedTriggerLock = new object();
            receivedLines = new Queue<string>();
            statusQueryTimeout = new StopWatch();
            statusQueryInterval = DefaultStatusQueryInterval;

            thread = new Thread(threadStart);
            thread.Start();
        }

        public void TriggerConnect(string portName)
        {
            pushTrigger(State.TriggerType.Connect, portName);
        }

        public void TriggerDisconnect()
        {
            pushTrigger(State.TriggerType.Disconnect);
        }

        public void TriggerReset()
        {
            pushTrigger(State.TriggerType.Reset);
        }

        public void TriggerCancel()
        {
            pushTrigger(State.TriggerType.Cancel);
        }

        public void TriggerUnlock()
        {
            pushTrigger(State.TriggerType.Unlock);
        }

        public void TriggerHome()
        {
            pushTrigger(State.TriggerType.Home);
        }

        public void TriggerJog(string line)
        {
            pushTrigger(State.TriggerType.Jog, line);
        }

        public void TriggerRun(string code)
        {
            pushTrigger(State.TriggerType.Run, code);
        }

        private void clearTrigger()
        {
            lock (queuedTriggerLock) queuedTrigger = null;
        }

        private void pushTrigger(State.TriggerType type)
        {
            lock (queuedTriggerLock) queuedTrigger = new State.Trigger(type);
        }

        private void pushTrigger(State.TriggerType type, String parameter)
        {
            lock (queuedTriggerLock) queuedTrigger = new State.Trigger(type, parameter);
        }

        public State.Trigger PopTrigger(State.TriggerType type)
        {
            State.Trigger trigger = null;
            lock (queuedTriggerLock)
            {
                if (queuedTrigger != null && queuedTrigger.Type == type)
                {
                    trigger = queuedTrigger;
                    queuedTrigger = null;
                }
            }
            return trigger;
        }

        public void TransitionTo(State state, State.Trigger trigger = null)
        {
            Console.WriteLine("STATE: " + state.GetType().Name);

            statusQueryInterval = DefaultStatusQueryInterval;

            currentState = state;
            currentState.Enter(trigger);
        }

        public bool TryConnect(string portName)
        {
            GrblConnection connection = new GrblConnection();
            if (connection.TryConnect(portName))
            {
                Connection = connection;
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            Connection = null;
        }

        public void LoadProgram(string code)
        {
            Program = new GrblProgram(code);
        }

        public void ClearResetDetected()
        {
            ResetDetected = false;
        }

        public void RequestStatusQueryInterval(double secs)
        {
            statusQueryInterval = TimeSpan.FromSeconds(secs);
        }

        private void connection_LineReceived(GrblConnection sender, string line)
        {
            queueReceivedLine(line);
        }

        private void clearReceivedLines()
        {
            lock (receivedLines) receivedLines.Clear();
        }

        private void queueReceivedLine(string line)
        {
            lock (receivedLines) receivedLines.Enqueue(line);
        }

        private string dequeueReceivedLine()
        {
            lock (receivedLines) return receivedLines.Count > 0 ? receivedLines.Dequeue() : null;
        }

        private void processReceivedLines()
        {
            for (string line = dequeueReceivedLine(); line != null; line = dequeueReceivedLine())
            {
                if (line.StartsWith("<"))
                {
                    GrblStatus status = GrblStatus.Parse(line);
                    if (status != null)
                    {
                        StatusReported = status;
                        pendingStatusQueryRequest = null;
                    }
                }
                else if (line.StartsWith("Grbl "))
                {
                    resetConnectionState();
                    ResetDetected = true;
                }
            }
        }

        private void resetConnectionState()
        {
            ClearResetDetected();
            StatusReported = GrblStatus.Unknown;
            clearTrigger();
            clearReceivedLines();
            statusQueryTimeout.Zero();
            pendingStatusQueryRequest = null;
        }

        private void pollStatusReport()
        {
            if (statusQueryTimeout.Expired(statusQueryInterval) && pendingStatusQueryRequest == null)
            {
                GrblRequest request = GrblRequest.CreateStatusQueryRequest();
                if (Connection.Send(request))
                {
                    statusQueryTimeout.Reset();
                    pendingStatusQueryRequest = request;
                }
            }
        }

        private void threadStart()
        {
            TransitionTo(DisconnectedState);

            while (true) // till when?
            {
                if (Connection != null)
                {
                    if (Connection.IsActive)
                    {
                        pollStatusReport();
                        processReceivedLines();
                    }
                    else
                    {
                        Disconnect();
                    }
                }

                currentState.Step();

                Thread.Sleep(10);
            }
        }
    }
}
