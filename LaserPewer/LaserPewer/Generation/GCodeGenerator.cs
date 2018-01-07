using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LaserPewer.Generation
{
    public static class GCodeGenerator
    {
        public static string Generate(MachinePath machinePath, double maxSpindleRate, double maxFeedRate)
        {
            StringBuilder gcode = new StringBuilder();

            gcode.AppendLine("G21");
            gcode.AppendLine("G90");
            gcode.AppendLine("M4 S0");

            double currentPower = double.NaN;
            double currentSpeed = double.NaN;

            foreach (MachinePath.Travel travel in machinePath.Travels)
            {
                if (!travel.Rapid)
                {
                    if (travel.Power == currentPower && travel.Speed == currentSpeed)
                    {
                        gcode.AppendFormat(CultureInfo.InvariantCulture, "G1 X{0:F2} Y{1:F2}",
                            travel.Destination.X, travel.Destination.Y);
                    }
                    else
                    {
                        gcode.AppendFormat(CultureInfo.InvariantCulture, "G1 X{0:F2} Y{1:F2} S{2:F2} F{3:F2}",
                            travel.Destination.X, travel.Destination.Y, travel.Power * maxSpindleRate, travel.Speed * maxFeedRate);

                        currentPower = travel.Power;
                        currentSpeed = travel.Speed;
                    }
                }
                else
                {
                    gcode.AppendFormat(CultureInfo.InvariantCulture, "G0 X{0:F2} Y{1:F2}",
                        travel.Destination.X, travel.Destination.Y);
                }

                gcode.AppendLine();
            }

            gcode.AppendLine("M5");

            return gcode.ToString();
        }
    }
}
