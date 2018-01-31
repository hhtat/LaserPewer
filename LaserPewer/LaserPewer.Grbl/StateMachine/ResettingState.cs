using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class ResettingState : StateBase
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
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;

            if (timeout.Expired(TimeSpan.FromSeconds(0.5)))
            {
                GrblRequest request = GrblRequest.CreateSoftResetRequest();
                if (controller.Connection.Send(request))
                {
                    timeout.Reset();
                }
            }
            else if (controller.ResetDetected)
            {
                controller.TransitionTo(controller.ReadyState);
            }
        }
    }
}
