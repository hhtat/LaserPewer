using LaserPewer.Shared;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LaserPewer.Grbl.StateMachine
{
    public class Controller
    {
        private static readonly TimeSpan DefaultStatusQueryInterval = TimeSpan.FromSeconds(0.5);

        public delegate void PropertiesModifiedEventHandler(Controller sender, bool invalidateAcceptsTrigger);
        public event PropertiesModifiedEventHandler PropertiesModified;

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
        public readonly State RunHoldState;
        public readonly State RunResumeState;

        public string StateName
        {
            get { return currentState?.FriendlyName ?? string.Empty; }
        }

        private GrblConnection _activeConnection;
        public GrblConnection ActiveConnection
        {
            get { return _activeConnection; }
            private set
            {
                if (_activeConnection != null)
                {
                    _activeConnection.Disconnect();
                    _activeConnection.LineReceived -= connection_LineReceived;
                }

                _activeConnection = value;
                resetConnectionState();
                propertiesModifiedFlag = true;

                if (_activeConnection != null)
                {
                    _activeConnection.LineReceived += connection_LineReceived;
                    _activeConnection.StartReceiving();
                }
            }
        }

        private bool _resetDetected;
        public bool ResetDetected
        {
            get { return _resetDetected; }
            private set
            {
                _resetDetected = value;
                propertiesModifiedFlag = true;
            }
        }

        private GrblStatus _latestStatus;
        public GrblStatus LatestStatus
        {
            get { return _latestStatus; }
            private set
            {
                _latestStatus = value;
                propertiesModifiedFlag = true;
            }
        }

        private GrblProgram _loadedProgram;
        public GrblProgram LoadedProgram
        {
            get { return _loadedProgram; }
            private set
            {
                _loadedProgram = value;
                propertiesModifiedFlag = true;
            }
        }

        private bool propertiesModifiedFlag;
        private bool stateChangedFlag;

        private readonly object queuedTriggerLock;
        private State.Trigger queuedTrigger;

        private readonly Queue<string> receivedLines;

        private readonly StopWatch statusQueryTimeout;
        private TimeSpan statusQueryInterval;
        private GrblRequest pendingStatusQueryRequest;

        private State currentState;

        private bool stop;
        private Thread thread;

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
            RunHoldState = new RunHoldState(this);
            RunResumeState = new RunResumeState(this);

            queuedTriggerLock = new object();
            receivedLines = new Queue<string>();
            statusQueryTimeout = new StopWatch();
            statusQueryInterval = DefaultStatusQueryInterval;
        }

        public void Start()
        {
            if (thread != null) return;

            thread = new Thread(threadStart);
            thread.Start();
        }

        public void Stop()
        {
            stop = true;
        }

        public bool AcceptsTrigger(State.TriggerType type)
        {
            return currentState != null && currentState.AcceptsTrigger(type);
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

        public void TriggerPause()
        {
            pushTrigger(State.TriggerType.Pause);
        }

        public void TriggerResume()
        {
            pushTrigger(State.TriggerType.Resume);
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
            if (parameter == null) throw new ArgumentNullException();
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
            stateChangedFlag = true;
        }

        public bool TryConnect(string portName)
        {
            GrblConnection connection = new GrblConnection();
            if (connection.TryConnect(portName))
            {
                ActiveConnection = connection;
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            ActiveConnection = null;
        }

        public void ClearResetDetected()
        {
            ResetDetected = false;
        }

        public void RequestStatusQueryInterval(double secs)
        {
            statusQueryInterval = TimeSpan.FromSeconds(secs);
        }

        public void LoadProgram(string code)
        {
            LoadedProgram = new GrblProgram(code);
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
                        if (!status.Equals(LatestStatus))
                        {
                            LatestStatus = status;
                        }
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
            LatestStatus = GrblStatus.Unknown;
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
                if (ActiveConnection.Send(request))
                {
                    statusQueryTimeout.Reset();
                    pendingStatusQueryRequest = request;
                }
            }
        }

        private void threadStart()
        {
            resetConnectionState();
            TransitionTo(DisconnectedState);

            while (!stop) // till when?
            {
                if (ActiveConnection != null)
                {
                    if (ActiveConnection.IsActive)
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

                if (propertiesModifiedFlag || stateChangedFlag || (LoadedProgram != null && LoadedProgram.ProgressUpdated))
                {
                    PropertiesModified?.Invoke(this, stateChangedFlag);

                    propertiesModifiedFlag = false;
                    stateChangedFlag = false;
                    LoadedProgram?.ClearProgressUpdated();
                }

                Thread.Sleep(10);
            }

            Disconnect();
        }
    }
}
