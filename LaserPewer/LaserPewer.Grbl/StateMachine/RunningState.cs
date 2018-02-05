using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunningState : StateBase
    {
        public RunningState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Trigger trigger)
        {
            if (trigger.Parameter != null)
            {
                controller.LoadProgram(trigger.Parameter);
            }
        }

        public override void Step()
        {
            if (handleCommonStates()) return;
            if (handleTrigger(TriggerType.Cancel, controller.RunCancelState)) return;

            controller.Program.Poll(controller.Connection);
            if (controller.Program.EndOfProgram)
            {
                controller.TransitionTo(controller.ReadyState);
            }
            else if (controller.Program.ErrorsDetected)
            {
                controller.TransitionTo(controller.RunCancelState);
            }
        }
    }
}
