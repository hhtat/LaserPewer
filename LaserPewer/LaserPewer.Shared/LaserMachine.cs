using System.Threading.Tasks;

namespace LaserPewer.Shared
{
    public abstract class LaserMachine
    {
        public delegate void StatusUpdatedEventHandler(LaserMachine sender, MachineStatus status);
        public event StatusUpdatedEventHandler StatusUpdated;

        protected void invokeStatusUpdated(MachineStatus status)
        {
            StatusUpdated?.Invoke(this, status);
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

        public class MachineStatus
        {
            public readonly bool Connected;
            public readonly string Message;
            public readonly double X;
            public readonly double Y;

            public MachineStatus(bool connected, string message, double x, double y)
            {
                Connected = connected;
                Message = message;
                X = x;
                Y = y;
            }
        }
    }
}
