namespace LaserPewer.Grbl.StateMachine
{
    public class ReadyState : StateBase
    {
        public ReadyState(Controller controller) : base(controller)
        {
        }

        public override StateBase Step()
        {
            if (controller.DisconnectTriggered()) return controller.DisconnectedState;
            if (controller.ResetTriggered()) return controller.ResettingState;
            if (controller.HomeTriggered()) return controller.HomingState;

            return this;
        }
    }
}
