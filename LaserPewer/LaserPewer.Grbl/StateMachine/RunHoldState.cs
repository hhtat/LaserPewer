using LaserPewer.Shared;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunHoldState : State
    {
        private readonly StopWatch timeout;

        public RunHoldState(Controller controller) : base(controller, "Paused")
        {
            timeout = new StopWatch();
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            addTransition(new TriggerTransition(controller.RunResumeState, TriggerType.Resume));
            addTransition(new TriggerTransition(controller.RunCancelState, TriggerType.Cancel));
        }

        protected override void onEnter(Trigger trigger)
        {
            timeout.Reset();

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            if (controller.LatestStatus.State != GrblStatus.MachineState.Hold &&
                controller.LatestStatus.State != GrblStatus.MachineState.Hold0 &&
                controller.LatestStatus.State != GrblStatus.MachineState.Hold1)
            {
                retrySend(timeout, GrblRequest.CreateFeedHoldRequest());
            }
        }
    }
}
