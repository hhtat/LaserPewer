namespace LaserPewer.Grbl.StateMachine
{
    public abstract class StateBase
    {
        protected readonly Controller controller;

        protected StateBase(Controller controller)
        {
            this.controller = controller;
        }

        public virtual void Enter(Trigger trigger) { }

        public abstract void Step();

        protected bool handleTrigger(TriggerType type, StateBase target)
        {
            if (controller.PopTrigger(type) != null)
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
            Home,
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
