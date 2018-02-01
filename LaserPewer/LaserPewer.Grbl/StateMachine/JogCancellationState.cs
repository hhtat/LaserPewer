using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JogCancellationState : StateBase
    {
        private readonly StopWatch retryTimeout;
        private StopWatch stateTimeout;

        public JogCancellationState(Controller controller) : base(controller)
        {
            retryTimeout = new StopWatch();
        }

        public override void Enter(Trigger trigger)
        {
            retryTimeout.Zero();
            stateTimeout = null;
        }

        public override void Step()
        {
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;

            if (retryTimeout.Expired(TimeSpan.FromSeconds(RetryTimeoutSecs)))
            {
                GrblRequest request = GrblRequest.CreateJogCancelRequest();
                if (controller.Connection.Send(request))
                {
                    retryTimeout.Reset();
                }
            }
            else if (stateTimeout == null)
            {
                stateTimeout = new StopWatch();
            }
            else if (stateTimeout.Expired(TimeSpan.FromSeconds(StateTimeoutSecs)))
            {
                controller.TransitionTo(controller.ReadyState);
            }
            else if (controller.StatusReported != null)
            {
                if (controller.StatusReported.State == GrblStatus.MachineState.Jog) stateTimeout.Reset();
                controller.ClearStatusReported();
            }
        }
    }
}
