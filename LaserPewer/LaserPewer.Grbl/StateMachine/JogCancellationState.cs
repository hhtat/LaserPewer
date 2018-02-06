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

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));
        }

        protected override void onEnter(Trigger trigger)
        {
            retryTimeout.Zero();
            stateTimeout = null;

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            if (retrySend(retryTimeout, GrblRequest.CreateJogCancelRequest()))
            {
            }
            else if (handleMachineStateNegTimeout(ref stateTimeout, GrblStatus.MachineState.Jog, controller.ReadyState))
            {
            }
        }
    }
}
