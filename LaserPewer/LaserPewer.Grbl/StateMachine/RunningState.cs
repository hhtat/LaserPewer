using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunningState : State
    {
        private TimeoutTransition stateTimeoutTransition;

        public RunningState(Controller controller) : base(controller, "Running")
        {
        }

        protected override void addTransitions()
        {
            addTransition(new DisconnectedTransition(controller.DisconnectedState));
            addTransition(new TriggerTransition(controller.DisconnectedState, TriggerType.Disconnect));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Reset));
            addTransition(new MachineStateTransition(controller.AlarmedState, GrblStatus.MachineState.Alarm));

            addTransition(new TriggerTransition(controller.RunHoldState, TriggerType.Pause));
            addTransition(new TriggerTransition(controller.ResettingState, TriggerType.Cancel));

            stateTimeoutTransition = addTransition(new TimeoutTransition(controller.ReadyState, TimeSpan.FromSeconds(StateTimeoutSecs)));
        }

        protected override void onEnter(Trigger trigger)
        {
            stateTimeoutTransition.Reset();

            if (trigger != null)
            {
                controller.LoadProgram(trigger.Parameter);
            }

            controller.RequestStatusQueryInterval(FastStatusQueryIntervalSecs);
        }

        protected override void onStep()
        {
            controller.LoadedProgram.Poll(controller.ActiveConnection);

            if (controller.LoadedProgram.ErrorsDetected)
            {
                controller.TransitionTo(controller.ResettingState);
            }
            else if (!controller.LoadedProgram.EndOfProgram)
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
