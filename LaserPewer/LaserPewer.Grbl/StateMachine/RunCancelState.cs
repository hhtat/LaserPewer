using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class RunCancelState : StateBase
    {
        public RunCancelState(Controller controller) : base(controller)
        {
        }

        public override void Step()
        {
            throw new NotImplementedException();
        }
    }
}
