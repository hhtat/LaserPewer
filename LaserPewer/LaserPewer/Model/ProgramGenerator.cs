using System;
using System.Windows.Threading;

namespace LaserPewer.Model
{
    public class ProgramGenerator
    {
        public event EventHandler SettingModified;

        private double _vectorPower;
        public double VectorPower
        {
            get { return _vectorPower; }
            set
            {
                _vectorPower = value;
                SettingModified?.Invoke(this, null);
            }
        }

        private double _vectorSpeed;
        public double VectorSpeed
        {
            get { return _vectorSpeed; }
            set
            {
                _vectorSpeed = value;
                SettingModified?.Invoke(this, null);
            }
        }

        public ProgramGenerator()
        {
            VectorPower = 1.0;
            VectorSpeed = 1.0;
        }
    }
}
