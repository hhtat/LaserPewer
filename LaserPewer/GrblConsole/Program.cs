using LaserPewer.Grbl.StateMachine;
using System;

namespace GrblConsole
{
    class Program
    {
        public static void Main(string[] args)
        {
            Controller controller = new Controller();

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
            }
        }
    }
}
