using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class AlarmKillState : State
    {
        private readonly StopWatch retryTimeout;
        private TimeoutTransition abortTimeoutTransition;

        public AlarmKillState(Controller controller) : base(controller)
        {
            retryTimeout = new StopWatch();
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));

            addTransition(new MachineStateTransition(controller.ReadyState, GrblStatus.MachineState.Alarm, true));

            abortTimeoutTransition = new TimeoutTransition(controller.AlarmedState, TimeSpan.FromSeconds(AbortTimeoutSecs));
            addTransition(abortTimeoutTransition);
        }

        protected override void onEnter(Trigger trigger)
        {
            retryTimeout.Zero();
            abortTimeoutTransition.Reset();

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            if (retrySend(retryTimeout, GrblRequest.CreateKillAlarmRequest()))
            {
            }
        }
    }
}
