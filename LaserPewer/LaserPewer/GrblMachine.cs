using System;
using System.Diagnostics;
using System.Globalization;

namespace LaserPewer
{
    public class GrblMachine
    {
        private GrblStreamer streamer;

        public bool Connected { get { return streamer != null && streamer.Connected; } }

        public event EventHandler MachineDisconnected;
        public event EventHandler MachineReset;

        public delegate void StatusUpdatedEventHandler(object sender, MachineStatus status);
        public event StatusUpdatedEventHandler StatusUpdated;

        public delegate void AlarmRaisedEventHandler(object sender, int alarm);
        public event AlarmRaisedEventHandler AlarmRaised;

        public delegate void MessageFeedbackEventHandler(object sender, string message);
        public event MessageFeedbackEventHandler MessageFeedback;

        public GrblMachine()
        {
        }

        public void Connect(string portName)
        {
            if (streamer != null) streamer.Disconnect();

            streamer = new GrblStreamer();

            try
            {
                streamer.MessageReceived += Streamer_MessageReceived;
                streamer.Connect(portName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                MachineDisconnected?.Invoke(this, null);
            }
        }

        public void Reset()
        {
            wrapStreamerCall(() => streamer.SendReset());
        }

        public void Home()
        {
            wrapStreamerCall(() => streamer.TrySendHomeRequest());
        }

        public void Unlock()
        {
            wrapStreamerCall(() => streamer.TrySendUnlockRequest());
        }

        public void Hold()
        {
            wrapStreamerCall(() => streamer.SendHoldRequest());
        }

        public void Jog(double x, double y, double rate)
        {
            string line = "$J=G21 G90 X" + ToGcode(x) + " Y" + ToGcode(y) + " F" + ToGcode(rate);
            wrapStreamerCall(() => streamer.TrySendCommand(line));
        }

        public void PollStatus()
        {
            wrapStreamerCall(() => streamer.TrySendStatusRequest());
        }

        public static string ToGcode(double number)
        {
            return number.ToString("F", CultureInfo.InvariantCulture);
        }

        private bool checkConnection()
        {
            if (!Connected)
            {
                MachineDisconnected?.Invoke(this, null);
                return false;
            }

            return true;
        }

        private void wrapStreamerCall(Action call)
        {
            if (!checkConnection()) return;

            try
            {
                call();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                checkConnection();
            }
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
                        status.X = parseNumber(wPosTokens[0]);
                        status.Y = parseNumber(wPosTokens[1]);
                    }
                }

                StatusUpdated?.Invoke(this, status);
            }
            else if (message.StartsWith("ALARM:"))
            {
                AlarmRaised?.Invoke(this, parseInt(message.Substring(6)));
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

        private static double parseNumber(string s)
        {
            double number;

            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out number))
            {
                return number;
            }

            return double.NaN;
        }

        private static int parseInt(string s)
        {
            int n;

            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out n))
            {
                return n;
            }

            return -1;
        }

        public class MachineStatus
        {
            public string Status;
            public double X;
            public double Y;
        }
    }
}
