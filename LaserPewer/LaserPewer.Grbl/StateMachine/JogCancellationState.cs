using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JogCancellationState : State
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

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        public override void Step()
        {
            if (handleCommonStates()) return;

            if (retrySend(retryTimeout, GrblRequest.CreateJogCancelRequest()))
            {
            }
            else if (handleMachineStateNegTimeout(ref stateTimeout, GrblStatus.MachineState.Jog, controller.ReadyState))
            {
            }
        }
    }
}
