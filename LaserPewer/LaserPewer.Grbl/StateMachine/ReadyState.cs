namespace LaserPewer.Grbl.StateMachine
{
    public class ReadyState : State
    {
        public ReadyState(Controller controller) : base(controller)
        {
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            addTransition(new TriggerTransition(controller.HomingState, TriggerType.Home));
            addTransition(new TriggerTransition(controller.JoggingState, TriggerType.Jog));
            addTransition(new TriggerTransition(controller.RunningState, TriggerType.Run));
        }

        protected override void onStep()
        {
        }
    }
}
