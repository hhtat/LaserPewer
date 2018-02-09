using System;
using System.Threading.Tasks;

namespace LaserPewer.Shared
{
    public abstract class LaserMachine
    {
        public event EventHandler StateUpdated;

        private MachineState _state;
        public MachineState State
        {
            get { return _state; }
            protected set
            {
                if (value == null) throw new ArgumentNullException();
                _state = value;
                StateUpdated?.Invoke(this, null);
            }
        }

        public LaserMachine()
        {
            State = new MachineState(false, string.Empty, double.NaN, double.NaN);
        }

        public void ConnectAsync(string portName)
        {
            Task.Run(() => doConnect(portName));
        }

        protected abstract void doConnect(string portName);

        public Task DisconnectAsync()
        {
            return Task.Run(() => doDisconnect());
        }

        protected abstract void doDisconnect();

        public void ResetAsync()
        {
            Task.Run(() => doReset());
        }

        protected abstract void doReset();

        public void HomeAsync()
        {
            Task.Run(() => doHome());
        }

        protected abstract void doHome();

        public void UnlockAsync()
        {
            Task.Run(() => doUnlock());
        }

        protected abstract void doUnlock();

        public class MachineState
        {
            public readonly bool Connected;
            public readonly string Status;
            public readonly double X;
            public readonly double Y;

            public MachineState(bool connected, string message, double x, double y)
            {
                Connected = connected;
                Status = message;
                X = x;
                Y = y;
            }
        }
    }
}
