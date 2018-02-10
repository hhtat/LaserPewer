using LaserPewer.Shared;
using System;
using System.Collections.Generic;

namespace LaserPewer.Grbl.StateMachine
{
    public abstract class State
    {
        protected const double AbortTimeoutSecs = 2.0;
        protected const double RetryTimeoutSecs = 0.5;
        protected const double StateTimeoutSecs = 1.0;
        protected const double RapidStatusQueryIntervalSecs = 0.01;
        protected const double FastStatusQueryIntervalSecs = 0.1;

        public readonly string FriendlyName;

        protected readonly List<Transition> _transitions;
        public IReadOnlyList<Transition> Transitions { get { return _transitions; } }

        protected readonly Controller controller;
        private readonly ISet<TriggerType> triggerTypes;

        protected State(Controller controller, string friendlyName)
        {
            this.controller = controller;
            FriendlyName = friendlyName;
            _transitions = new List<Transition>();
            triggerTypes = new HashSet<TriggerType>();
        }

        public bool AcceptsTrigger(TriggerType type)
        {
            return triggerTypes.Contains(type);
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

        protected T addTransition<T>(T transition) where T : Transition
        {
            _transitions.Add(transition);

            TriggerTransition triggerTransition = transition as TriggerTransition;
            if (triggerTransition != null)
            {
                triggerTypes.Add(triggerTransition.Type);
            }

            return transition;
        }

        protected bool retrySend(StopWatch timeout, GrblRequest request)
        {
            if (timeout.Expired(TimeSpan.FromSeconds(RetryTimeoutSecs)))
            {
                if (controller.ActiveConnection.Send(request))
                {
                    timeout.Reset();
                }

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
            Pause,
            Resume,
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
