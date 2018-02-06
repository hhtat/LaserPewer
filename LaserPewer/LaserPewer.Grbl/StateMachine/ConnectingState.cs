namespace LaserPewer.Grbl.StateMachine
{
    public class ConnectingState : State
    {
        private string portName;

        public ConnectingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Trigger trigger)
        {
            portName = trigger.Parameter;
        }

        public override void Step()
        {
            if (controller.TryConnect(portName))
            {
                controller.TransitionTo(controller.ResettingState);
            }
            else
            {
                controller.TransitionTo(controller.DisconnectedState);
            }
        }
    }
}
