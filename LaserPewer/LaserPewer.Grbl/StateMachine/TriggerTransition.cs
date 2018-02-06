namespace LaserPewer.Grbl.StateMachine
{
    public class TriggerTransition : Transition
    {
        private readonly State.TriggerType type;

        public TriggerTransition(State target, State.TriggerType type) : base(target)
        {
            this.type = type;
        }

        public override bool TryStep(Controller controller)
        {
            State.Trigger trigger = controller.PopTrigger(type);
            if (trigger != null)
            {
                controller.TransitionTo(target, trigger);
                return true;
            }

            return false;
        }
    }
}
