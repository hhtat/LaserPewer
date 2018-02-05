namespace LaserPewer.Grbl.StateMachine
{
    public class DisconnectedState : StateBase
    {
        public DisconnectedState(Controller controller) : base(controller)
        {
        }

        public override void Step()
        {
            if (handleTrigger(TriggerType.Connect, controller.ConnectingState)) return;

            if (controller.Connection != null)
            {
                controller.Disconnect();
            }
        }
    }
}
