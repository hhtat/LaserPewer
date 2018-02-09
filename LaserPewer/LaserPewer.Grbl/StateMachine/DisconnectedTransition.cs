namespace LaserPewer.Grbl.StateMachine
{
    public class DisconnectedTransition : Transition
    {
        public DisconnectedTransition(State target) : base(target)
        {
        }

        public override bool TryStep(Controller controller)
        {
            if (controller.ActiveConnection == null)
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }
    }
}
