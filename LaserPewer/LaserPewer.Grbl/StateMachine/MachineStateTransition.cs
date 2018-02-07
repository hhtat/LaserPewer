namespace LaserPewer.Grbl.StateMachine
{
    public class MachineStateTransition : Transition
    {
        private readonly GrblStatus.MachineState state;
        private readonly bool negate;

        public MachineStateTransition(State target, GrblStatus.MachineState state, bool negate = false) : base(target)
        {
            this.state = state;
            this.negate = negate;
        }

        public override bool TryStep(Controller controller)
        {
            bool stateCheck = controller.StatusReported.State == state;
            if (negate) stateCheck = !stateCheck;
            if (stateCheck)
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }
    }
}
