using LaserPewer.Grbl.StateMachine;
using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl
{
    public class GrblMachine2 : LaserMachine
    {
        private readonly Controller controller;

        public GrblMachine2()
        {
            controller = new Controller();
            controller.PropertiesModified += Controller_PropertiesModified;
            controller.Start();
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

        private void Controller_PropertiesModified(object sender, EventArgs e)
        {
            bool connected = controller.ActiveConnection != null;
            string message = controller.CurrentState.ToString();
            double x = controller.LatestStatus.WPosX;
            double y = controller.LatestStatus.WPosY;

            invokeStatusUpdated(new MachineStatus(connected, message, x, y));
        }
    }
}
