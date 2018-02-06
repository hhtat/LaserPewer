namespace LaserPewer.Grbl.StateMachine
{
    public class HomingState : State
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
            if (handleCommonStates()) return;

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
