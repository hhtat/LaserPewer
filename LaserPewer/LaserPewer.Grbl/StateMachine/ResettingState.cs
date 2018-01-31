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

        public override void Enter()
        {
            timeout.Zero();
            controller.ClearResetDetected();
        }

        public override StateBase Step()
        {
            if (controller.DisconnectTriggered()) return controller.DisconnectedState;

            if (timeout.Expired(TimeSpan.FromSeconds(0.5)))
            {
                GrblRequest request = GrblRequest.CreateSoftResetRequest();
                if (controller.Connection.Send(request))
                {
                    timeout.Reset();
                }

                return this;
            }

            if (controller.ResetDetected)
            {
                return controller.ReadyState;
            }

            return this;
        }
    }
}
