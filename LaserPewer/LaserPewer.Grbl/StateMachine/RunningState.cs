using LaserPewer.Shared;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunningState : State
    {
        private StopWatch timeout;

        public RunningState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Trigger trigger)
        {
            timeout = null;

            if (trigger.Parameter != null)
            {
                controller.LoadProgram(trigger.Parameter);
            }
        }

        public override void Step()
        {
            if (handleCommonStates()) return;
            if (handleTrigger(TriggerType.Cancel, controller.ResettingState)) return;

            controller.Program.Poll(controller.Connection);

            if (controller.Program.ErrorsDetected)
            {
                controller.TransitionTo(controller.ResettingState);
            }
            else if (!controller.Program.EndOfProgram)
            {
            }
            else if (handleMachineStateNegTimeout(ref timeout, GrblStatus.MachineState.Run, controller.ReadyState))
            {
            }
        }
    }
}
