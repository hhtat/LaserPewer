using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JogCancelState : State
    {
        private readonly StopWatch retryTimeout;
        private TimeoutTransition stateTimeoutTransition;

        public JogCancelState(Controller controller) : base(controller, "Cancelling")
        {
            retryTimeout = new StopWatch();
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            stateTimeoutTransition = addTransition(new TimeoutTransition(controller.ReadyState, TimeSpan.FromSeconds(StateTimeoutSecs)));
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
