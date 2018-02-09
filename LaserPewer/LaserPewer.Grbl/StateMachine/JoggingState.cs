using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class JoggingState : State
    {
        private TimeoutTransition stateTimeoutTransition;
        private GrblRequest request;

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

            stateTimeoutTransition = new TimeoutTransition(controller.ReadyState, TimeSpan.FromSeconds(StateTimeoutSecs));
        }

        protected override void onEnter(Trigger trigger)
        {
            stateTimeoutTransition.Reset();
            request = GrblRequest.CreateJoggingRequest(trigger.Parameter);

            controller.RequestStatusQueryInterval(RapidStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            if (request.ResponseStatus == GrblResponseStatus.Unsent)
            {
                controller.Connection.Send(request);
                stateTimeoutTransition.Reset();
            }
            else if (request.ResponseStatus == GrblResponseStatus.Pending)
            {
                stateTimeoutTransition.Reset();
            }
            else if (controller.LatestStatus.State == GrblStatus.MachineState.Jog)
            {
                stateTimeoutTransition.Reset();
            }
        }
    }
}
