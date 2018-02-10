namespace LaserPewer.Grbl.StateMachine
{
    public class TriggerTransition : Transition
    {
        public readonly State.TriggerType Type;

        public TriggerTransition(State target, State.TriggerType type) : base(target)
        {
            this.Type = type;
        }

        public override bool TryStep(Controller controller)
        {
            State.Trigger trigger = controller.PopTrigger(Type);
            if (trigger != null)
            {
                controller.TransitionTo(target, trigger);
                return true;
            }

            return false;
        }
    }
}
