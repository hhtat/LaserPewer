namespace LaserPewer.Grbl.StateMachine
{
    public class DisconnectedState : StateBase
    {
        public DisconnectedState(Controller controller) : base(controller)
        {
        }

        public override StateBase Step()
        {
            Trigger trigger = controller.PopTrigger(TriggerType.Connect);
            if (trigger != null)
            {
                GrblConnection connection = new GrblConnection();
                if (connection.TryConnect(trigger.Parameter))
                {
                    controller.Connection = connection;
                    return controller.ReadyState;
                }
            }

            if (controller.Connection != null)
            {
                controller.Connection = null;
                return this;
            }

            return this;
        }
    }
}
