namespace LaserPewer.Grbl.StateMachine
{
    public class ConnectingState : State
    {
        private string portName;

        public ConnectingState(Controller controller) : base(controller)
        {
        }

        protected override void onEnter(Trigger trigger)
        {
            portName = trigger.Parameter;
        }

        protected override void onStep()
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
