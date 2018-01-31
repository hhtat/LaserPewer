using System;
using System.Collections.Generic;
using System.Threading;

namespace LaserPewer.Grbl.StateMachine
{
    public class Controller
    {
        public readonly StateBase DisconnectedState;
        public readonly StateBase ReadyState;
        public readonly StateBase ResettingState;
        public readonly StateBase HomingState;

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

        private readonly object queuedTriggerLock;
        private StateBase.Trigger queuedTrigger;

        private readonly Queue<string> receivedLines;

        private StateBase currentState;
        private readonly Thread thread;

        public Controller()
        {
            DisconnectedState = new DisconnectedState(this);
            ReadyState = new ReadyState(this);
            ResettingState = new ResettingState(this);
            HomingState = new HomingState(this);

            queuedTriggerLock = new object();
            receivedLines = new Queue<string>();

            currentState = DisconnectedState;
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

        public bool DisconnectTriggered()
        {
            return PopTrigger(StateBase.TriggerType.Disconnect) != null;
        }

        public void TriggerReset()
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Reset));
        }

        public bool ResetTriggered()
        {
            return PopTrigger(StateBase.TriggerType.Reset) != null;
        }

        public void TriggerHome()
        {
            pushTrigger(new StateBase.Trigger(StateBase.TriggerType.Home));
        }

        public bool HomeTriggered()
        {
            return PopTrigger(StateBase.TriggerType.Home) != null;
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

        public void ClearResetDetected()
        {
            ResetDetected = false;
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
                if (line.StartsWith("Grbl "))
                {
                    ResetDetected = true;
                }
            }
        }

        private void resetConnectionState()
        {
            ClearResetDetected();
            pushTrigger(null);
            clearReceivedLines();
        }

        private void threadStart()
        {
            while (true) // till when?
            {
                processReceivedLines();

                StateBase newState = currentState.Step();
                if (newState != currentState)
                {
                    Console.WriteLine("NEW STATE: " + newState.GetType().Name);
                    currentState = newState;
                    currentState.Enter();
                }

                Thread.Sleep(1);
            }
        }
    }
}
