using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public class TimeoutTransition : Transition
    {
        private readonly TimeSpan timeout;
        private readonly StopWatch stopWatch;

        public TimeoutTransition(State target, TimeSpan timeout) : base(target)
        {
            this.timeout = timeout;
            this.stopWatch = new StopWatch();
        }

        public void Reset()
        {
            stopWatch.Reset();
        }

        public override bool TryStep(Controller controller)
        {
            if (stopWatch.Expired(timeout))
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }
    }
}
