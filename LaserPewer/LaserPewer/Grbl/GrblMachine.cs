using System.Threading.Tasks;

namespace LaserPewer.Grbl
{
    public class GrblMachine
    {
        public delegate void StateChangedEventHandler(GrblMachine sender, MachineState state, MachineState oldState);
        public event StateChangedEventHandler StateChanged;

        private MachineState _state;
        public MachineState State
        {
            get { return _state; }
            set
            {
                MachineState oldValue = _state;
                _state = value;
                StateChanged?.Invoke(this, _state, oldValue);
            }
        }

        private GrblConnection connection;

        public GrblMachine()
        {
            State = MachineState.Disconnected;
        }

        public void Connect(string portName)
        {
            if (State != MachineState.Disconnected)
            {
                return;
            }

            GrblConnection newConnection = new GrblConnection();
            if (!newConnection.TryConnect(portName))
            {
                return;
            }

            connection = newConnection;
            connection.Closed += connection_Closed;
            connection.LineReceived += connection_LineReceived;

            State = MachineState.Connected;
        }

        public void Disconnect()
        {
            if (State == MachineState.Disconnected)
            {
                return;
            }

            connection.Disconnect();
            connection.Closed -= connection_Closed;
            connection.LineReceived -= connection_LineReceived;
            connection = null;

            State = MachineState.Disconnected;
        }

        private void connection_Closed(GrblConnection sender)
        {
            throw new System.NotImplementedException();
        }

        private void connection_LineReceived(GrblConnection sender, string line)
        {
            throw new System.NotImplementedException();
        }

        public enum MachineState
        {
            Disconnected,
            Connected,
            Resetting,
            Idle,
            Homing,
            Jogging,
            Running,
        }
    }
}
