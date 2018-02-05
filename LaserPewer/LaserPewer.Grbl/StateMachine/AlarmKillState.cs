using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class AlarmKillState : StateBase
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
            if (handleDisconnect(controller.DisconnectedState)) return;
            if (handleMachineStateNeg(GrblStatus.MachineState.Alarm, controller.ReadyState)) return;
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;

            if (retrySend(retryTimeout, GrblRequest.CreateKillAlarmRequest()))
            {
            }
            else if (timeoutAbort(abortTimeout, controller.AlarmedState))
            {
            }
        }
    }
}
