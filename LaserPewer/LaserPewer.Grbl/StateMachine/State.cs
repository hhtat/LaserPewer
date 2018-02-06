using LaserPewer.Shared;
using System;
using System.Collections.Generic;

namespace LaserPewer.Grbl.StateMachine
{
    public abstract class State
    {
        protected const double AbortTimeoutSecs = 2.0;
        protected const double RetryTimeoutSecs = 0.5;
        protected const double StateTimeoutSecs = 0.2;
        protected const double RapidStatusQueryIntervalSecs = 0.01;

        protected readonly List<Transition> _transitions;
        public IReadOnlyList<Transition> Transitions { get { return _transitions; } }

        protected readonly Controller controller;

        protected State(Controller controller)
        {
            this.controller = controller;
            _transitions = new List<Transition>();
        }

        public void Enter(Trigger trigger)
        {
            if (Transitions.Count == 0)
            {
                addTransitions();
            }

            onEnter(trigger);
        }

        protected virtual void onEnter(Trigger trigger) { }

        public void Step()
        {
            foreach (Transition transition in Transitions)
            {
                if (transition.TryStep(controller)) return;
            }

            onStep();
        }

        protected abstract void onStep();

        protected virtual void addTransitions() { }

        protected void addTransition(Transition transition)
        {
            _transitions.Add(transition);
        }

        protected bool handleMachineStateNegTimeout(ref StopWatch timeout, GrblStatus.MachineState state, State target)
        {
            if (timeout == null)
            {
                timeout = new StopWatch();
                return true;
            }

            if (timeout.Expired(TimeSpan.FromSeconds(StateTimeoutSecs)))
            {
                controller.TransitionTo(target);
                return true;
            }

            if (controller.StatusReported.State == state)
            {
                timeout.Reset();
                return true;
            }

            return false;
        }

        protected bool retrySend(StopWatch timeout, GrblRequest request)
        {
            if (timeout.Expired(TimeSpan.FromSeconds(RetryTimeoutSecs)))
            {
                if (controller.Connection.Send(request))
                {
                    timeout.Reset();
                }

                return true;
            }

            return false;
        }

        protected bool timeoutAbort(StopWatch timeout, State target)
        {
            if (timeout.Expired(TimeSpan.FromSeconds(AbortTimeoutSecs)))
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }

        public enum TriggerType
        {
            Connect,
            Disconnect,
            Reset,
            Cancel,
            Unlock,
            Home,
            Jog,
            Run,
        }

        public class Trigger
        {
            public readonly TriggerType Type;
            public readonly string Parameter;

            public Trigger(TriggerType type, string parameter = null)
            {
                Type = type;
                Parameter = parameter;
            }
        }
    }
}
