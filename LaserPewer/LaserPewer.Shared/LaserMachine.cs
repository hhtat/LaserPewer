using System;
using System.Threading.Tasks;

namespace LaserPewer.Shared
{
    public abstract class LaserMachine : IDisposable
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

        private bool disposed = false;

        public LaserMachine()
        {
            State = new MachineState(false, string.Empty, double.NaN, double.NaN);
        }

        ~LaserMachine()
        {
            dispose(false);
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void dispose(bool disposing)
        {
            if (!disposed)
            {
                doDispose(disposing);
                disposed = true;
            }
        }

        protected abstract void doDispose(bool disposing);

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

        public void CancelAsync()
        {
            Task.Run(() => doCancel());
        }

        protected abstract void doCancel();

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

        public void JogAsync(double x, double y, double rate)
        {
            Task.Run(() => doJog(x, y, rate));
        }

        protected abstract void doJog(double x, double y, double rate);

        public void RunAsync(string code)
        {
            Task.Run(() => doRun(code));
        }

        protected abstract void doRun(string code);

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
