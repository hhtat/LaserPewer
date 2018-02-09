using LaserPewer.Grbl.StateMachine;
using LaserPewer.Shared;

namespace LaserPewer.Grbl
{
    public class GrblMachine2 : LaserMachine
    {
        private readonly Controller controller;

        public GrblMachine2()
        {
            controller = new Controller();
            controller.StatusUpdated += Controller_StatusUpdated;
        }

        private void Controller_StatusUpdated(Controller sender, GrblStatus status)
        {
            invokeStatusUpdated(new MachineStatus(status.State.ToString(), status.WPosX, status.WPosY));
        }

        protected override void doConnect(string portName)
        {
            controller.TriggerConnect(portName);
        }

        protected override void doDisconnect()
        {
            controller.TriggerDisconnect();
        }

        protected override void doReset()
        {
            controller.TriggerReset();
        }

        protected override void doHome()
        {
            controller.TriggerHome();
        }

        protected override void doUnlock()
        {
            controller.TriggerUnlock();
        }
    }
}
