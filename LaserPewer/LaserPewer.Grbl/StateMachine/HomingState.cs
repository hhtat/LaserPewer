namespace LaserPewer.Grbl.StateMachine
{
    public class HomingState : StateBase
    {
        private GrblRequest request;

        public HomingState(Controller controller) : base(controller)
        {
        }

        public override void Enter()
        {
            request = GrblRequest.CreateHomingRequest();
        }

        public override StateBase Step()
        {
            if (controller.DisconnectTriggered()) return controller.DisconnectedState;
            if (controller.ResetTriggered()) return controller.ResettingState;

            if (request.ResponseStatus == GrblResponseStatus.Unsent)
            {
                controller.Connection.Send(request);
                return this;
            }

            if (request.ResponseStatus != GrblResponseStatus.Pending)
            {
                return controller.ReadyState;
            }

            return this;
        }
    }
}
