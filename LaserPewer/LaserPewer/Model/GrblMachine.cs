using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Threading;

namespace LaserPewer.Model
{
    public class GrblMachine
    {
        private GrblStreamer streamer;

        public bool Connected { get { return streamer != null && streamer.Connected; } }

        public event EventHandler MachineConnecting;
        public event EventHandler MachineConnected;
        public event EventHandler MachineDisconnected;
        public event EventHandler MachineReset;
        public event EventHandler MachineReadyToSend;

        public delegate void StatusUpdatedEventHandler(object sender, MachineStatus status);
        public event StatusUpdatedEventHandler StatusUpdated;

        public delegate void AlarmRaisedEventHandler(object sender, int alarm);
        public event AlarmRaisedEventHandler AlarmRaised;

        public delegate void MessageFeedbackEventHandler(object sender, string message);
        public event MessageFeedbackEventHandler MessageFeedback;

        private readonly DispatcherTimer statusPollingTimer;

        public GrblMachine()
        {
            statusPollingTimer = new DispatcherTimer();
            statusPollingTimer.Interval = TimeSpan.FromMilliseconds(500);
            statusPollingTimer.Tick += statusPollingTimer_Tick;
        }

        public bool Connect(string portName)
        {
            if (streamer != null) streamer.Disconnect();

            MachineConnecting?.Invoke(this, null);

            try
            {
                streamer = new GrblStreamer();
                streamer.MessageReceived += Streamer_MessageReceived;
                streamer.ReadyToSend += Streamer_ReadyToSend;
                streamer.Connect(portName);
                statusPollingTimer.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                MachineDisconnected?.Invoke(this, null);
                return false;
            }

            MachineConnected?.Invoke(this, null);
            return true;
        }

        public void Disconnect()
        {
            statusPollingTimer.Stop();
            if (streamer != null) streamer.Disconnect();
            MachineDisconnected?.Invoke(this, null);
        }

        public void Reset()
        {
            wrapStreamerCall(() => { streamer.SendReset(); return true; });
        }

        public void Home()
        {
            wrapStreamerCall(() => streamer.TrySendHomeRequest());
        }

        public void Unlock()
        {
            wrapStreamerCall(() => streamer.TrySendUnlockRequest());
        }

        public void Resume()
        {
            wrapStreamerCall(() => { streamer.SendResumeRequest(); return true; });
        }

        public void Hold()
        {
            wrapStreamerCall(() => { streamer.SendHoldRequest(); return true; });
        }

        public void Jog(double x, double y, double rate)
        {
            string line = string.Format(CultureInfo.InvariantCulture, "$J=G21 G90 X{0:F3} Y{1:F3} F{2:F3}", x, y, rate);
            wrapStreamerCall(() => streamer.TrySendCommand(line));
        }

        public SendResult SendGCode(string line)
        {
            return wrapStreamerCall(() => streamer.TrySendCommand(line));
        }

        private void statusPollingTimer_Tick(object sender, EventArgs e)
        {
            wrapStreamerCall(() => streamer.TrySendStatusRequest());
        }

        public static string ToGcode(double number)
        {
            return number.ToString("F3", CultureInfo.InvariantCulture);
        }

        private bool checkConnection()
        {
            if (!Connected)
            {
                statusPollingTimer.Stop();
                MachineDisconnected?.Invoke(this, null);
                return false;
            }

            return true;
        }

        private SendResult wrapStreamerCall(Func<bool> call)
        {
            if (checkConnection())
            {
                try
                {
                    return call() ? SendResult.Sent : SendResult.Retry;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    checkConnection();
                }
            }

            return SendResult.Failed;
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

        private void Streamer_ReadyToSend(object sender, EventArgs e)
        {
            MachineReadyToSend?.Invoke(this, null);
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

        public enum SendResult
        {
            Sent,
            Retry,
            Failed,
        }
    }
}
