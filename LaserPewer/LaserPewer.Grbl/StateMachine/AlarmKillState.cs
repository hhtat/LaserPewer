using LaserPewer.Shared;

namespace LaserPewer.Grbl.StateMachine
{
    public class AlarmKillState : State
    {
        private readonly StopWatch retryTimeout;
        private readonly StopWatch abortTimeout;

        public AlarmKillState(Controller controller) : base(controller)
        {
            retryTimeout = new StopWatch();
            abortTimeout = new StopWatch();
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));

            addTransition(new MachineStateTransition(controller.ReadyState, GrblStatus.MachineState.Alarm, true));
        }

        protected override void onEnter(Trigger trigger)
        {
            retryTimeout.Zero();
            abortTimeout.Reset();

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            if (retrySend(retryTimeout, GrblRequest.CreateKillAlarmRequest()))
            {
            }
            else if (timeoutAbort(abortTimeout, controller.AlarmedState))
            {
            }
        }
    }
}
