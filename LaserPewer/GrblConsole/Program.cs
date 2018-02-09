using LaserPewer.Grbl.StateMachine;
using System;

namespace GrblConsole
{
    class Program
    {
        public static void Main(string[] args)
        {
            Controller controller = new Controller();
            controller.Start();

            while (true)
            {
                string line = Console.ReadLine();

                if (line.StartsWith("c"))
                {
                    string portName = line.Substring(1);
                    controller.TriggerConnect(portName);
                }
                else if (line.StartsWith("d"))
                {
                    controller.TriggerDisconnect();
                }
                else if (line.StartsWith("r"))
                {
                    controller.TriggerReset();
                }
                else if (line.StartsWith("z"))
                {
                    controller.TriggerCancel();
                }
                else if (line.StartsWith("u"))
                {
                    controller.TriggerUnlock();
                }
                else if (line.StartsWith("h"))
                {
                    controller.TriggerHome();
                }
                else if (line.StartsWith("j"))
                {
                    controller.TriggerJog("G21 G90 X100 Y-100 F100");
                }
                else if (line.StartsWith("g"))
                {
                    controller.TriggerJog("G21 G90 X0 Y0 F100");
                }
                else if (line.StartsWith(">"))
                {
                    controller.TriggerRun(testProgram);
                }
            }
        }

        private const string testProgram =
            "G21\n" +
            "G90\n" +
            "M4 S0\n" +
            "G0 X110.00 Y-10.00\n" +
            "G1 X10.00 Y-10.00 S1000.00 F5000\n" +
            "G1 X10.00 Y-60.00\n" +
            "G1 X110.00 Y-60.00\n" +
            "G1 X110.00 Y-10.00\n" +
            "G1 X110.00 Y-10.00\n" +
            "M5";
    }
}
