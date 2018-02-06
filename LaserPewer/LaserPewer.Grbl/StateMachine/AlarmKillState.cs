﻿using LaserPewer.Shared;

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

        public override void Enter(Trigger trigger)
        {
            retryTimeout.Zero();
            abortTimeout.Reset();

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        public override void Step()
        {
            if (handleCommonStates()) return;
            if (handleMachineStateNeg(GrblStatus.MachineState.Alarm, controller.ReadyState)) return;

            if (retrySend(retryTimeout, GrblRequest.CreateKillAlarmRequest()))
            {
            }
            else if (timeoutAbort(abortTimeout, controller.AlarmedState))
            {
            }
        }
    }
}
