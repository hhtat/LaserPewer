namespace LaserPewer.Grbl.StateMachine
{
    public class AlarmedState : StateBase
    {
        public AlarmedState(Controller controller) : base(controller)
        {
        }

        public override void Step()
        {
            if (handleDisconnect(controller.DisconnectedState)) return;
            if (handleMachineStateNeg(GrblStatus.MachineState.Alarm, controller.ReadyState)) return;
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;
            if (handleTrigger(TriggerType.Home, controller.HomingState)) return;
            if (handleTrigger(TriggerType.Unlock, controller.AlarmKillState)) return;
        }
    }
}
