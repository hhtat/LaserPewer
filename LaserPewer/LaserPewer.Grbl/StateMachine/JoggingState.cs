using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JoggingState : StateBase
    {
        private GrblRequest request;
        private StopWatch stopWatch;

        public JoggingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Trigger trigger)
        {
            request = GrblRequest.CreateJoggingRequest(trigger.Parameter);
            stopWatch = null;
        }

        public override void Step()
        {
            if (handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return;
            if (handleTrigger(TriggerType.Reset, controller.ResettingState)) return;

            if (request.ResponseStatus == GrblResponseStatus.Unsent)
            {
                controller.Connection.Send(request);
            }
            else if (request.ResponseStatus == GrblResponseStatus.Pending)
            {
            }
            else if (stopWatch == null)
            {
                stopWatch = new StopWatch();
            }
            else if (stopWatch.Expired(TimeSpan.FromSeconds(0.25)))
            {
                controller.TransitionTo(controller.ReadyState);
            }
            else if (controller.StatusReported != null)
            {
                if (controller.StatusReported.State == GrblStatus.MachineState.Jog) stopWatch.Reset();
                controller.ClearStatusReported();
            }
        }
    }
}
