namespace LaserPewer.Grbl.StateMachine
{
    public class HomingState : State
    {
        private GrblRequest request;

        public HomingState(Controller controller) : base(controller)
        {
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
        }

        protected override void onEnter(Trigger trigger)
        {
            request = GrblRequest.CreateHomingRequest();
        }

        protected override void onStep()
        {
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
