using System.Diagnostics;
using System.Globalization;

namespace LaserPewer
{
    public class GrblMachine
    {
        private readonly GrblStreamer streamer;

        public delegate void StatusUpdatedEventHandler(object sender, MachineStatus status);
        public event StatusUpdatedEventHandler StatusUpdated;

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
        }

        public class MachineStatus
        {
            public string Status;
            public double X;
            public double Y;
        }
    }
}
