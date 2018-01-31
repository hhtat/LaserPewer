namespace LaserPewer.Grbl.StateMachine
{
    public class HomingState : StateBase
    {
        private GrblRequest request;

        public HomingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Trigger trigger)
        {
            request = GrblRequest.CreateHomingRequest();
        }

        public override void Step()
        {
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;

            if (request.ResponseStatus == GrblResponseStatus.Unsent)
            {
                controller.Connection.Send(request);
            }
            else if (request.ResponseStatus != GrblResponseStatus.Pending)
            {
                controller.TransitionTo(controller.ReadyState);
            }
        }
    }
}
