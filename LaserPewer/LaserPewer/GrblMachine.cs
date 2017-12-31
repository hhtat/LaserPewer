using System;
using System.Diagnostics;
using System.Globalization;

namespace LaserPewer
{
    public class GrblMachine
    {
        private readonly GrblStreamer streamer;

        public event EventHandler MachineReset;

        public delegate void StatusUpdatedEventHandler(object sender, MachineStatus status);
        public event StatusUpdatedEventHandler StatusUpdated;

        public delegate void AlarmRaisedEventHandler(object sender, int alarm);
        public event AlarmRaisedEventHandler AlarmRaised;

        public delegate void MessageFeedbackEventHandler(object sender, string message);
        public event MessageFeedbackEventHandler MessageFeedback;

        public GrblMachine()
        {
            streamer = new GrblStreamer();
            streamer.MessageReceived += Streamer_MessageReceived;
        }

        public void Connect(string portName)
        {
            streamer.Connect(portName);
        }

        public void Reset()
        {
            streamer.SendReset();
        }

        public void Home()
        {
            streamer.TrySendHomeRequest();
        }

        public void Unlock()
        {
            streamer.TrySendUnlockRequest();
        }

        public void Hold()
        {
            streamer.SendHoldRequest();
        }

        public void Jog(double x, double y)
        {
            string line = "$J=G21 G90 X" + x.ToString("F", CultureInfo.InvariantCulture) + " Y" + y.ToString("F", CultureInfo.InvariantCulture) + " F10000";
            streamer.TrySendCommand(line);
        }

        public void PollStatus()
        {
            streamer.TrySendStatusRequest();
        }

        private void Streamer_MessageReceived(object sender, string message)
        {
            if (message.StartsWith("<") && message.EndsWith(">"))
            {
                string[] tokens = message.Substring(1, message.Length - 2).Split('|');
                MachineStatus status = new MachineStatus();

                status.Status = "???";
                status.X = double.NaN;
                status.Y = double.NaN;

                if (tokens.Length > 0)
                {
                    status.Status = tokens[0];
                }

                foreach (string token in tokens)
                {
                    if (token.StartsWith("WPos:"))
                    {
                        string[] wPosTokens = token.Substring(5).Split(',');
                        if (wPosTokens.Length != 3) continue;
                        double parsed;
                        if (double.TryParse(wPosTokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out parsed)) status.X = parsed; else Debugger.Break();
                        if (double.TryParse(wPosTokens[1], NumberStyles.Any, CultureInfo.InvariantCulture, out parsed)) status.Y = parsed; else Debugger.Break();
                    }
                }

                StatusUpdated?.Invoke(this, status);
            }
            else if (message.StartsWith("ALARM:"))
            {
                int alarm;
                if (int.TryParse(message.Substring(6), NumberStyles.Any, CultureInfo.InvariantCulture, out alarm))
                {
                    AlarmRaised?.Invoke(this, alarm);
                }
            }
            else if (message.StartsWith("[MSG:") && message.EndsWith("]"))
            {
                MessageFeedback?.Invoke(this, message.Substring(5, message.Length - 6));
            }
            else if (message.StartsWith("Grbl "))
            {
                MachineReset?.Invoke(this, null);
            }
        }

        public class MachineStatus
        {
            public string Status;
            public double X;
            public double Y;
        }
    }
}
