using LaserPewer.Generation;
using System;
using System.Windows;

namespace LaserPewer.Model
{
    public class ProgramGenerator
    {
        public event EventHandler SettingModified;
        public event EventHandler Completed;

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
            GCodeProgram = null;
            Completed?.Invoke(this, null);
        }

        public void Generate(Drawing drawing, MachineList.IProfile machineProfile)
        {
            if (drawing == null) return;

            drawing = drawing.Clone();
            drawing.Clip(new Rect(
                0.0, -machineProfile.TableSize.Height,
                machineProfile.TableSize.Width, machineProfile.TableSize.Height));

            VectorPath = VectorGenerator.Generate(drawing.Paths, VectorPower, VectorSpeed);
            GCodeProgram = GCodeGenerator.Generate(VectorPath, 1000.0, machineProfile.MaxFeedRate);

            Completed?.Invoke(this, null);
        }

        private void VectorGeneration_GenerationCompleted(object sender, EventArgs e)
        {
            Completed?.Invoke(this, null);
        }
    }
}
