namespace LaserPewer.Grbl.StateMachine
{
    public class DisconnectedState : State
    {
        public DisconnectedState(Controller controller) : base(controller, "Disconnected")
        {
        }

        protected override void addTransitions()
        {
            addTransition(new TriggerTransition(controller.ConnectingState, TriggerType.Connect));
        }

        protected override void onStep()
        {
            if (controller.ActiveConnection != null)
            {
                controller.Disconnect();
            }
        }
    }
}
