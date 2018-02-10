using System;
using System.Threading.Tasks;

namespace LaserPewer.Shared
{
    public abstract class LaserMachine : IDisposable
    {
        public delegate void StateUpdatedEventHandler(LaserMachine sender, MachineState state, bool invalidateCanDo);
        public event StateUpdatedEventHandler StateUpdated;

        public MachineState State { get; private set; }

        private bool disposed = false;

        public LaserMachine()
        {
            State = new MachineState(false, string.Empty, double.NaN, double.NaN, 0, 0);
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

        protected void updateState(MachineState state, bool invalidateCanDos)
        {
            State = state ?? throw new ArgumentNullException();
            StateUpdated?.Invoke(this, state, invalidateCanDos);
        }

        public abstract bool CanConnect();

        public void ConnectAsync(string portName)
        {
            Task.Run(() => doConnect(portName));
        }

        protected abstract void doConnect(string portName);

        public abstract bool CanDisconnect();

        public Task DisconnectAsync()
        {
            return Task.Run(() => doDisconnect());
        }

        protected abstract void doDisconnect();

        public abstract bool CanReset();

        public void ResetAsync()
        {
            Task.Run(() => doReset());
        }

        protected abstract void doReset();

        public abstract bool CanCancel();

        public void CancelAsync()
        {
            Task.Run(() => doCancel());
        }

        protected abstract void doCancel();

        public abstract bool CanHome();

        public void HomeAsync()
        {
            Task.Run(() => doHome());
        }

        protected abstract void doHome();

        public abstract bool CanUnlock();

        public void UnlockAsync()
        {
            Task.Run(() => doUnlock());
        }

        protected abstract void doUnlock();

        public abstract bool CanJog();

        public void JogAsync(double x, double y, double rate)
        {
            Task.Run(() => doJog(x, y, rate));
        }

        protected abstract void doJog(double x, double y, double rate);

        public abstract bool CanRun();

        public void RunAsync(string code)
        {
            Task.Run(() => doRun(code));
        }

        protected abstract void doRun(string code);

        public abstract bool CanPause();

        public void PauseAsync()
        {
            Task.Run(() => doPause());
        }

        protected abstract void doPause();

        public abstract bool CanResume();

        public void ResumeAsync()
        {
            Task.Run(() => doResume());
        }

        protected abstract void doResume();

        public class MachineState
        {
            public readonly bool Connected;
            public readonly string Status;
            public readonly double X;
            public readonly double Y;
            public readonly int LineCount;
            public readonly int LineAt;

            public MachineState(bool connected, string status, double x, double y, int lineCount, int lineAt)
            {
                Connected = connected;
                Status = status;
                X = x;
                Y = y;
                LineCount = lineCount;
                LineAt = lineAt;
            }

            public object TriggerType { get; set; }
        }
    }
}
