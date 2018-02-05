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

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        public override void Step()
        {
            if (handleDisconnect(controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;

            if (retrySend(retryTimeout, GrblRequest.CreateJogCancelRequest()))
            {
            }
            else if (handleMachineStateNegTimeout(ref stateTimeout, GrblStatus.MachineState.Jog, controller.ReadyState))
            {
            }
        }
    }
}
