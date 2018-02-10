using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunCancelState : State
    {
        private readonly StopWatch retryTimeout;
        private TimeoutTransition abortTimeoutTransition;

        public RunCancelState(Controller controller) : base(controller, "Unlocking")
        {
            retryTimeout = new StopWatch();
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            addTransition(new MachineStateTransition(controller.ResettingState, GrblStatus.MachineState.Hold));
            addTransition(new MachineStateTransition(controller.ResettingState, GrblStatus.MachineState.Hold0));
            abortTimeoutTransition = addTransition(new TimeoutTransition(controller.ResettingState, TimeSpan.FromSeconds(AbortTimeoutSecs)));
        }

        protected override void onEnter(Trigger trigger)
        {
            retryTimeout.Zero();
            abortTimeoutTransition.Reset();

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            if (retrySend(retryTimeout, GrblRequest.CreateFeedHoldRequest()))
            {
            }
        }
    }
}
