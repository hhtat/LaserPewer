namespace LaserPewer.Grbl.StateMachine
{
    public class ConnectingState : StateBase
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
            GrblConnection connection = new GrblConnection();
            if (connection.TryConnect(portName))
            {
                controller.Connection = connection;
                controller.TransitionTo(controller.ReadyState);
            }
            else
            {
                controller.TransitionTo(controller.DisconnectedState);
            }
        }
    }
}
