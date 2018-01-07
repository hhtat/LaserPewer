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

        public MachinePath VectorPath
        {
            get { return vectorGeneration != null ? vectorGeneration.VectorPath : null; }
        }

        private VectorGeneration vectorGeneration;

        public ProgramGenerator()
        {
            VectorPower = 1.0;
            VectorSpeed = 1.0;
        }

        public void Generate()
        {
            if (AppCore.Document.Drawing == null) return;

            Drawing drawing = AppCore.Document.Drawing.Clone();
            drawing.Clip(new Rect(
                0.0, -AppCore.MachineList.Active.TableSize.Height,
                AppCore.MachineList.Active.TableSize.Width, AppCore.MachineList.Active.TableSize.Height));

            vectorGeneration = new VectorGeneration(VectorPower, VectorSpeed, drawing.Paths);
            vectorGeneration.GenerationCompleted += VectorGeneration_GenerationCompleted;
            vectorGeneration.GenerateAsync();
        }

        private void VectorGeneration_GenerationCompleted(object sender, EventArgs e)
        {
            Generated?.Invoke(this, null);
        }
    }
}
