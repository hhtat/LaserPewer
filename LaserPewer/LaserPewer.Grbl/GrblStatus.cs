using System.Globalization;

namespace LaserPewer.Grbl
{
    public class GrblStatus
    {
        public readonly MachineState State;
        public readonly double WPosX;
        public readonly double WPosY;

        private GrblStatus(MachineState state, double wPosX, double wPosY)
        {
            State = state;
            WPosX = wPosX;
            WPosY = wPosY;
        }

        public static GrblStatus Parse(string report)
        {
            if (!report.StartsWith("<") || !report.EndsWith(">")) return null;

            string[] tokens = report.Substring(1, report.Length - 2).Split('|');
            if (tokens.Length == 0) return null;

            MachineState state = MachineState.Unknown;
            if (!parseState(tokens[0], out state)) return null;

            double wPosX = double.NaN;
            double wPosY = double.NaN;
            foreach (string token in tokens)
            {
                if (token.StartsWith("WPos:"))
                {
                    string[] wPosTokens = token.Substring(5).Split(',');
                    if (wPosTokens.Length != 3) return null;
                    if (!double.TryParse(wPosTokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out wPosX)) return null;
                    if (!double.TryParse(wPosTokens[1], NumberStyles.Any, CultureInfo.InvariantCulture, out wPosY)) return null;
                }
            }

            return new GrblStatus(state, wPosX, wPosY);
        }

        private static bool parseState(string token, out MachineState state)
        {
            switch (token)
            {
                case "Idle":
                    state = MachineState.Idle;
                    return true;
                case "Run":
                    state = MachineState.Run;
                    return true;
                case "Hold":
                    state = MachineState.Hold;
                    return true;
                case "Hold:0":
                    state = MachineState.Hold0;
                    return true;
                case "Hold:1":
                    state = MachineState.Hold1;
                    return true;
                case "Jog":
                    state = MachineState.Jog;
                    return true;
                case "Alarm":
                    state = MachineState.Alarm;
                    return true;
                case "Door":
                    state = MachineState.Door;
                    return true;
                case "Door:0":
                    state = MachineState.Door0;
                    return true;
                case "Door:1":
                    state = MachineState.Door1;
                    return true;
                case "Door:2":
                    state = MachineState.Door2;
                    return true;
                case "Door:3":
                    state = MachineState.Door3;
                    return true;
                case "Check":
                    state = MachineState.Door;
                    return true;
                case "Home":
                    state = MachineState.Door;
                    return true;
                case "Sleep":
                    state = MachineState.Door;
                    return true;
                default:
                    state = MachineState.Unknown;
                    return false;
            }
        }

        public enum MachineState
        {
            Unknown,
            Idle,
            Run,
            Hold,
            Hold0,
            Hold1,
            Jog,
            Alarm,
            Door,
            Door0,
            Door1,
            Door2,
            Door3,
            Check,
            Home,
            Sleep,
        }
    }
}
