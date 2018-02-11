using LaserPewer.Generation;
using LaserPewer.Geometry;
using LaserPewer.Utilities;
using System;
using System.Collections.Generic;
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

        public void Generate(Drawing drawing, Vector offset, MachineProfiles.IProfile machineProfile)
        {
            if (drawing == null) return;

            List<Path> paths = new List<Path>();
            foreach (Path path in drawing.Paths)
            {
                paths.Add(Path.Offset(path, offset));
            }
            paths = Clipper.ClipPaths(paths, new Rect(new Point(0.0, 0.0),
                CornerMath.FarExtent(machineProfile.TableSize, machineProfile.Origin)));

            VectorPath = VectorGenerator.Generate(paths, VectorPower, VectorSpeed);
            GCodeProgram = GCodeGenerator.Generate(VectorPath, 1000.0, machineProfile.MaxFeedRate);

            Completed?.Invoke(this, null);
        }

        private void VectorGeneration_GenerationCompleted(object sender, EventArgs e)
        {
            Completed?.Invoke(this, null);
        }
    }
}
