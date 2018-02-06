using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JoggingState : State
    {
        private GrblRequest request;
        private StopWatch timeout;

        public JoggingState(Controller controller) : base(controller)
        {
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            addTransition(new TriggerTransition(controller.JogCancellationState, TriggerType.Cancel));
        }

        protected override void onEnter(Trigger trigger)
        {
            request = GrblRequest.CreateJoggingRequest(trigger.Parameter);
            timeout = null;

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
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
