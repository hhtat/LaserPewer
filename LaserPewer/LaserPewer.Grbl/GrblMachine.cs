using LaserPewer.Grbl.StateMachine;
using LaserPewer.Shared;
using System;
using System.Globalization;

namespace LaserPewer.Grbl
{
    public class GrblMachine : LaserMachine
    {
        private readonly Controller controller;

        public GrblMachine()
        {
            controller = new Controller();
            controller.PropertiesModified += Controller_PropertiesModified;
            controller.Start();
        }

        protected override void doDispose(bool disposing)
        {
            controller.Stop();
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

        protected override void doCancel()
        {
            controller.TriggerCancel();
        }

        protected override void doHome()
        {
            controller.TriggerHome();
        }

        protected override void doUnlock()
        {
            controller.TriggerUnlock();
        }

        protected override void doJog(double x, double y, double rate)
        {
            string line = string.Format(CultureInfo.InvariantCulture, "G21 G90 X{0:F2} Y{1:F2} F{2:F2}", x, y, rate);
            controller.TriggerJog(line);
        }

        protected override void doRun(string code)
        {
            controller.TriggerRun(code);
        }

        private void Controller_PropertiesModified(object sender, EventArgs e)
        {
            bool connected = controller.ActiveConnection != null;
            string message = controller.CurrentState.FriendlyName;
            double x = controller.LatestStatus.WPosX;
            double y = controller.LatestStatus.WPosY;

            State = new MachineState(connected, message, x, y);
        }
    }
}
