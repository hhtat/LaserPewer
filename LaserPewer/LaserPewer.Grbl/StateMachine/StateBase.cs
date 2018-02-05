using LaserPewer.Shared;
using System;

namespace LaserPewer.Grbl.StateMachine
{
    public abstract class StateBase
    {
        protected const double AbortTimeoutSecs = 2.0;
        protected const double RetryTimeoutSecs = 0.5;
        protected const double StateTimeoutSecs = 0.2;
        protected const double RapidStatusQueryIntervalSecs = 0.01;

        protected readonly Controller controller;

        protected StateBase(Controller controller)
        {
            this.controller = controller;
        }

        public virtual void Enter(Trigger trigger) { }

        public abstract void Step();

        protected bool handleDisconnect(StateBase target)
        {
            if (controller.Connection == null)
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }

        protected bool handleCommonStates()
        {
            if (this != controller.DisconnectedState && handleDisconnect(controller.DisconnectedState)) return true;
            if (this != controller.AlarmedState && this != controller.AlarmKillState && this != controller.HomingState && handleMachineState(GrblStatus.MachineState.Alarm, controller.AlarmedState)) return true;
            if (this != controller.DisconnectedState && handleTrigger(TriggerType.Disconnect, controller.DisconnectedState)) return true;
            if (this != controller.ResettingState && handleTrigger(TriggerType.Reset, controller.ResettingState)) return true;
            return false;
        }

        protected bool handleTrigger(TriggerType type, StateBase target)
        {
            Trigger trigger = controller.PopTrigger(type);
            if (trigger != null)
            {
                controller.TransitionTo(target, trigger);
                return true;
            }

            return false;
        }

        protected bool handleMachineState(GrblStatus.MachineState state, StateBase target)
        {
            if (controller.StatusReported.State == state)
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }

        protected bool handleMachineStateNeg(GrblStatus.MachineState state, StateBase target)
        {
            if (controller.StatusReported.State != state)
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }

        protected bool handleMachineStateNegTimeout(ref StopWatch timeout, GrblStatus.MachineState state, StateBase target)
        {
            if (timeout == null)
            {
                timeout = new StopWatch();
                return true;
            }

            if (timeout.Expired(TimeSpan.FromSeconds(StateTimeoutSecs)))
            {
                controller.TransitionTo(target);
                return true;
            }

            if (controller.StatusReported.State == state)
            {
                timeout.Reset();
                return true;
            }

            return false;
        }

        protected bool retrySend(StopWatch timeout, GrblRequest request)
        {
            if (timeout.Expired(TimeSpan.FromSeconds(RetryTimeoutSecs)))
            {
                if (controller.Connection.Send(request))
                {
                    timeout.Reset();
                }

                return true;
            }

            return false;
        }

        protected bool timeoutAbort(StopWatch timeout, StateBase target)
        {
            if (timeout.Expired(TimeSpan.FromSeconds(AbortTimeoutSecs)))
            {
                controller.TransitionTo(target);
                return true;
            }

            return false;
        }

        public enum TriggerType
        {
            Connect,
            Disconnect,
            Reset,
            Cancel,
            Unlock,
            Home,
            Jog,
            Run,
        }

        public class Trigger
        {
            public readonly TriggerType Type;
            public readonly string Parameter;

            public Trigger(TriggerType type, string parameter = null)
            {
                Type = type;
                Parameter = parameter;
            }
        }
    }
}
