namespace LaserPewer.Grbl.StateMachine
{
    public abstract class StateBase
    {
        protected readonly Controller controller;

        protected StateBase(Controller controller)
        {
            this.controller = controller;
        }

        public virtual void Enter() { }

        public abstract StateBase Step();

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
