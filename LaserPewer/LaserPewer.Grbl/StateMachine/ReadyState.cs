namespace LaserPewer.Grbl.StateMachine
{
    public class ReadyState : StateBase
    {
        public ReadyState(Controller controller) : base(controller)
        {
        }

        public override void Step()
        {
            if (handleCommonStates()) return;
            if (handleTrigger(TriggerType.Home, controller.HomingState)) return;
            if (handleTrigger(TriggerType.Jog, controller.JoggingState)) return;
            if (handleTrigger(TriggerType.Run, controller.RunningState)) return;
        }
    }
}
