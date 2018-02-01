namespace LaserPewer.Grbl.StateMachine
{
    public abstract class StateBase
    {
        protected const double RetryTimeoutSecs = 0.5;
        protected const double StateTimeoutSecs = 0.2;

        protected readonly Controller controller;

        protected StateBase(Controller controller)
        {
            this.controller = controller;
        }

        public virtual void Enter(Trigger trigger) { }

        public abstract void Step();

        protected bool handleTrigger(TriggerType type, StateBase target)
        {
            Trigger trigger = controller.PopTrigger(type);
            if (trigger != null)
            {
                controller.TransitionTo(target, trigger);
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
            Home,
            Jog,
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
