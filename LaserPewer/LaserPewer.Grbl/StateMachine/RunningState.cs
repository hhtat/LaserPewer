using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunningState : State
    {
        private TimeoutTransition stateTimeoutTransition;

        public RunningState(Controller controller) : base(controller)
        {
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Cancel));

            stateTimeoutTransition = new TimeoutTransition(controller.ReadyState, TimeSpan.FromSeconds(StateTimeoutSecs));
        }

        protected override void onEnter(Trigger trigger)
        {
            stateTimeoutTransition.Reset();

            if (trigger.Parameter != null)
            {
                controller.LoadProgram(trigger.Parameter);
            }
        }

        protected override void onStep()
        {
            controller.Program.Poll(controller.Connection);

            if (controller.Program.ErrorsDetected)
            {
                controller.TransitionTo(controller.ResettingState);
            }
            else if (!controller.Program.EndOfProgram)
            {
                stateTimeoutTransition.Reset();
            }
            else if (controller.LatestStatus.State == GrblStatus.MachineState.Run)
            {
                stateTimeoutTransition.Reset();
            }
        }
    }
}
