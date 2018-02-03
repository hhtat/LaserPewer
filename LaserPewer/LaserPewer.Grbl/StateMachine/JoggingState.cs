using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JoggingState : StateBase
    {
        private GrblRequest request;
        private StopWatch timeout;

        public JoggingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Trigger trigger)
        {
            request = GrblRequest.CreateJoggingRequest(trigger.Parameter);
            timeout = null;

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        public override void Step()
        {
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;
            if (handleTrigger(TriggerType.Cancel, controller.JogCancellationState)) return;

            if (request.ResponseStatus == GrblResponseStatus.Unsent)
            {
                controller.Connection.Send(request);
            }
            else if (request.ResponseStatus == GrblResponseStatus.Pending)
            {
            }
            else if (handleMachineStateNegTimeout(ref timeout, GrblStatus.MachineState.Jog, controller.ReadyState))
            {
            }
        }
    }
}
