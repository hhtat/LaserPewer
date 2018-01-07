using LaserPewer.Generation;
using System;
using System.Windows;

namespace LaserPewer.Model
{
    public class ProgramGenerator
    {
        public event EventHandler SettingModified;
        public event EventHandler Generated;

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

        public MachinePath VectorPath { get; private set; }
        public string GCodeProgram { get; private set; }

        public ProgramGenerator()
        {
            VectorPower = 1.0;
            VectorSpeed = 1.0;
        }

        public void Clear()
        {
            VectorPath = null;
        }

        public void Generate(Drawing drawing, Size tableSize, double maxFeedRate)
        {
            if (drawing == null) return;

            drawing = drawing.Clone();
            drawing.Clip(new Rect(
                0.0, -tableSize.Height,
                tableSize.Width, tableSize.Height));

            VectorPath = VectorGenerator.Generate(drawing.Paths, VectorPower, VectorSpeed);
            GCodeProgram = GCodeGenerator.Generate(VectorPath, 1000.0, maxFeedRate);

            Generated?.Invoke(this, null);
        }

        private void VectorGeneration_GenerationCompleted(object sender, EventArgs e)
        {
            Generated?.Invoke(this, null);
        }
    }
}
