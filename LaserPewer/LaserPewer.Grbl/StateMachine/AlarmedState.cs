namespace LaserPewer.Grbl.StateMachine
{
    public class AlarmedState : State
    {
        public AlarmedState(Controller controller) : base(controller)
        {
        }

        public override void Step()
        {
            if (handleCommonStates()) return;
            if (handleMachineStateNeg(GrblStatus.MachineState.Alarm, controller.ReadyState)) return;
            if (handleTrigger(TriggerType.Home, controller.HomingState)) return;
            if (handleTrigger(TriggerType.Unlock, controller.AlarmKillState)) return;
        }
    }
}
