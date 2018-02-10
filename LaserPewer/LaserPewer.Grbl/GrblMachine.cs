using LaserPewer.Grbl.StateMachine;
using LaserPewer.Shared;
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

        public override bool CanConnect()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Connect);
        }

        protected override void doConnect(string portName)
        {
            controller.TriggerConnect(portName);
        }

        public override bool CanDisconnect()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Disconnect);
        }

        protected override void doDisconnect()
        {
            controller.TriggerDisconnect();
        }

        public override bool CanReset()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Reset);
        }

        protected override void doReset()
        {
            controller.TriggerReset();
        }

        public override bool CanCancel()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Cancel);
        }

        protected override void doCancel()
        {
            controller.TriggerCancel();
        }

        public override bool CanHome()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Home);
        }

        protected override void doHome()
        {
            controller.TriggerHome();
        }

        public override bool CanUnlock()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Unlock);
        }

        protected override void doUnlock()
        {
            controller.TriggerUnlock();
        }

        public override bool CanJog()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Jog);
        }

        protected override void doJog(double x, double y, double rate)
        {
            string line = string.Format(CultureInfo.InvariantCulture, "G21 G90 X{0:F2} Y{1:F2} F{2:F2}", x, y, rate);
            controller.TriggerJog(line);
        }

        public override bool CanRun()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Run);
        }

        protected override void doRun(string code)
        {
            controller.TriggerRun(code);
        }

        public override bool CanPause()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Pause);
        }

        protected override void doPause()
        {
            controller.TriggerPause();
        }

        public override bool CanResume()
        {
            return controller.AcceptsTrigger(StateMachine.State.TriggerType.Resume);
        }

        protected override void doResume()
        {
            controller.TriggerResume();
        }

        private void Controller_PropertiesModified(Controller sender, bool invalidateAcceptsTrigger)
        {
            updateState(
                new MachineState(
                    controller.ActiveConnection != null,
                    controller.StateName,
                    controller.LatestStatus.WPosX,
                    controller.LatestStatus.WPosY,
                    controller.LoadedProgram?.Lines.Count ?? 0,
                    controller.LoadedProgram?.CurrentLine ?? 0),
                invalidateAcceptsTrigger);
        }
    }
}
