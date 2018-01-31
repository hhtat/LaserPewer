namespace LaserPewer.Grbl.StateMachine
{
    public class DisconnectedState : StateBase
    {
        public DisconnectedState(Controller controller) : base(controller)
        {
        }

        public override void Step()
        {
            Trigger trigger = controller.PopTrigger(TriggerType.Connect);
            if (trigger != null)
            {
                controller.TransitionTo(controller.ConnectingState, trigger);
            }
            else if (controller.Connection != null)
            {
                controller.Connection = null;
            }
        }
    }
}
