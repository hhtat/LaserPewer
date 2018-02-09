using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JogCancellationState : State
    {
        private readonly StopWatch retryTimeout;
        private TimeoutTransition stateTimeoutTransition;

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

            stateTimeoutTransition = new TimeoutTransition(controller.ReadyState, TimeSpan.FromSeconds(StateTimeoutSecs));
            addTransition(stateTimeoutTransition);
        }

        protected override void onEnter(Trigger trigger)
        {
            retryTimeout.Zero();
            stateTimeoutTransition.Reset();

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            retrySend(retryTimeout, GrblRequest.CreateJogCancelRequest());

            if (controller.LatestStatus.State == GrblStatus.MachineState.Jog)
            {
                stateTimeoutTransition.Reset();
            }
        }
    }
}
