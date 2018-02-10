using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunResumeState : State
    {
        private readonly StopWatch retryTimeout;
        private TimeoutTransition stateTimeoutTransition;


        public RunResumeState(Controller controller) : base(controller, "Resuming")
        {
            retryTimeout = new StopWatch();
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            stateTimeoutTransition = addTransition(new TimeoutTransition(controller.RunningState, TimeSpan.FromSeconds(StateTimeoutSecs)));
        }

        protected override void onEnter(Trigger trigger)
        {
            retryTimeout.Zero();
            stateTimeoutTransition.Reset();

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            if (controller.LatestStatus.State == GrblStatus.MachineState.Hold ||
                controller.LatestStatus.State == GrblStatus.MachineState.Hold0 ||
                controller.LatestStatus.State == GrblStatus.MachineState.Hold1)
            {
                retrySend(retryTimeout, GrblRequest.CreateCycleResumeRequest());
                stateTimeoutTransition.Reset();
            }
        }
    }
}
