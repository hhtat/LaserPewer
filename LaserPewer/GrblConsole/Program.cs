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
                else if (line.StartsWith("h"))
                {
                    controller.TriggerHome();
                }
            }
        }
    }
}
