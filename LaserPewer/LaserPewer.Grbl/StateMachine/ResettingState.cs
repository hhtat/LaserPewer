using LaserPewer.Shared;

namespace LaserPewer.Grbl.StateMachine
{
    public class ResettingState : State
    {
        private readonly StopWatch timeout;

        public ResettingState(Controller controller) : base(controller, "Resetting")
        {
            timeout = new StopWatch();
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
        }

        protected override void onEnter(Trigger trigger)
        {
            timeout.Zero();
            controller.ClearResetDetected();
        }

        protected override void onStep()
        {
            if (retrySend(timeout, GrblRequest.CreateSoftResetRequest()))
            {
            }
            else if (controller.ResetDetected)
            {
                controller.TransitionTo(controller.ReadyState);
            }
        }
    }
}
