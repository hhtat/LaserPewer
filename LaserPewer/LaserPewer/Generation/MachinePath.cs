using System.Collections.Generic;
using System.Windows;

namespace LaserPewer.Generation
{
    public class MachinePath
    {
        private List<Travel> _travels;
        public IReadOnlyList<Travel> Travels { get { return _travels; } }

        private double currentPower;
        private double currentSpeed;
        private bool activeCut;

        public MachinePath()
        {
            _travels = new List<Travel>();
        }

        public void SetPowerAndSpeed(double power, double speed)
        {
            currentPower = power;
            currentSpeed = speed;
        }

        public void EndCut()
        {
            activeCut = false;
        }

        public void TravelTo(Point destination)
        {
            if (activeCut)
            {
                _travels.Add(new Travel(destination, currentPower, currentSpeed));
            }
            else
            {
                _travels.Add(new Travel(destination));
                activeCut = true;
            }
        }

        public class Travel
        {
            public readonly Point Destination;
            public readonly bool Rapid;
            public readonly double Power;
            public readonly double Speed;

            public Travel(Point destination)
            {
                Destination = destination;
                Rapid = true;
                Power = 0.0;
                Speed = 0.0;
            }

            public Travel(Point destination, double power, double speed)
            {
                Destination = destination;
                Rapid = false;
                Power = power;
                Speed = speed;
            }
        }
    }
}
