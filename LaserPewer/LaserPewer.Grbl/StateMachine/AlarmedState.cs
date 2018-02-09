namespace LaserPewer.Grbl.StateMachine
{
    public class AlarmedState : State
    {
        public AlarmedState(Controller controller) : base(controller, "Alarm")
        {
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));

            addTransition(new MachineStateTransition(controller.ReadyState, GrblStatus.MachineState.Alarm, true));

            addTransition(new TriggerTransition(controller.HomingState, TriggerType.Home));
            addTransition(new TriggerTransition(controller.AlarmKillState, TriggerType.Unlock));
        }

        protected override void onStep()
        {
        }
    }
}
