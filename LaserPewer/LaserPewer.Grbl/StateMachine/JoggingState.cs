namespace LaserPewer.Grbl.StateMachine
{
    public class JoggingState : StateBase
    {
        private GrblRequest request;

        public JoggingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Trigger trigger)
        {
            request = GrblRequest.CreateJoggingRequest(trigger.Parameter);
        }

        public override void Step()
        {
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;

            handleRequest(request, controller.ReadyState);
        }
    }
}
