namespace LaserPewer.Grbl.StateMachine
{
    public abstract class Transition
    {
        protected readonly State target;

        protected Transition(State target)
        {
            this.target = target;
        }

        public abstract bool TryStep(Controller controller);
    }
}
