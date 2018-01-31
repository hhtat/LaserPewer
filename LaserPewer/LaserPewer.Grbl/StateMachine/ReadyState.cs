namespace LaserPewer.Grbl.StateMachine
{
    public class ReadyState : StateBase
    {
        public ReadyState(Controller controller) : base(controller)
        {
        }

        public override void Step()
        {
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;
            if (handleTrigger(TriggerType.Home, controller.HomingState)) return;
            if (handleTrigger(TriggerType.Jog, controller.JoggingState)) return;
        }
    }
}
