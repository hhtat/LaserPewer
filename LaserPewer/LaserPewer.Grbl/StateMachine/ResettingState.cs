using LaserPewer.Shared;

namespace LaserPewer.Grbl.StateMachine
{
    public class ResettingState : State
    {
        private readonly StopWatch timeout;

        public ResettingState(Controller controller) : base(controller)
        {
            timeout = new StopWatch();
        }

        public override void Enter(Trigger trigger)
        {
            timeout.Zero();
            controller.ClearResetDetected();
        }

        public override void Step()
        {
            if (handleDisconnect(controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;

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
