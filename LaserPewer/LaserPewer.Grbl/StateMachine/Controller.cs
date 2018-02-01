using LaserPewer.Shared;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LaserPewer.Grbl.StateMachine
{
    public class Controller
    {
        private static readonly TimeSpan DefaultStatusQueryInterval = TimeSpan.FromSeconds(0.5);

        public readonly StateBase DisconnectedState;
        public readonly StateBase ConnectingState;
        public readonly StateBase ReadyState;
        public readonly StateBase ResettingState;
        public readonly StateBase HomingState;
        public readonly StateBase JoggingState;
        public readonly StateBase JogCancellationState;

        private GrblConnection _connection;
        public GrblConnection Connection
        {
            get { return _connection; }
            set
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

        public bool ResetDetected { get; private set; }
        public GrblStatus StatusReported { get; private set; }

        private readonly object queuedTriggerLock;
        private StateBase.Trigger queuedTrigger;

        private readonly Queue<string> receivedLines;

        private readonly StopWatch statusQueryTimeout;
        private TimeSpan statusQueryInterval;
        private GrblRequest pendingStatusQueryRequest;

        private StateBase currentState;
        private readonly Thread thread;

        public Controller()
        {
            DisconnectedState = new DisconnectedState(this);
            ConnectingState = new ConnectingState(this);
            ReadyState = new ReadyState(this);
            ResettingState = new ResettingState(this);
            HomingState = new HomingState(this);
            JoggingState = new JoggingState(this);
            JogCancellationState = new JogCancellationState(this);

            queuedTriggerLock = new object();
            receivedLines = new Queue<string>();
            statusQueryTimeout = new StopWatch();
            statusQueryInterval = DefaultStatusQueryInterval;

            thread = new Thread(threadStart);
            thread.Start();
        }

        public void TriggerConnect(string portName)
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Connect, portName));
        }

        public void TriggerDisconnect()
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Disconnect));
        }

        public void TriggerReset()
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Reset));
        }

        public void TriggerCancel()
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Cancel));
        }

        public void TriggerHome()
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Home));
        }

        public void TriggerJog(string line)
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Jog, line));
        }

        private void pushTrigger(StateBase.Trigger trigger)
        {
            lock (queuedTriggerLock) queuedTrigger = trigger;
        }

        public StateBase.Trigger PopTrigger(StateBase.TriggerType type)
        {
            StateBase.Trigger trigger = null;
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

        public void TransitionTo(StateBase state, StateBase.Trigger trigger = null)
        {
            Console.WriteLine("STATE: " + state.GetType().Name);

            statusQueryInterval = DefaultStatusQueryInterval;

            currentState = state;
            currentState.Enter(trigger);
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
                    ResetDetected = true;
                }
            }
        }

        private void resetConnectionState()
        {
            ClearResetDetected();
            StatusReported = GrblStatus.Unknown;
            pushTrigger(null);
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
                    pollStatusReport();
                    processReceivedLines();
                }

                currentState.Step();

                Thread.Sleep(10);
            }
        }
    }
}
