using LaserPewer.Geometry;
using LaserPewer.Model;
using LaserPewer.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace LaserPewer.Generation
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

        private List<Path> paths;
        private double maxFeed;

        private bool stop;
        private Thread thread;

        public ProgramGenerator()
        {
            VectorPower = 1.0;
            VectorSpeed = 1.0;
        }

        public bool TryStart(Drawing drawing, Vector offset, MachineProfiles.IProfile machineProfile)
        {
            if (drawing == null) return false;
            if (thread != null) return false;

            paths = new List<Path>();
            foreach (Path path in drawing.Paths)
            {
                paths.Add(Path.Offset(path, offset));
            }
            paths = Clipper.ClipPaths(paths, new Rect(new Point(0.0, 0.0),
                CornerMath.FarExtent(machineProfile.TableSize, machineProfile.Origin)));
            maxFeed = machineProfile.MaxFeedRate;

            stop = false;
            thread = new Thread(threadStart);
            thread.Start();

            return true;
        }

        public void Stop()
        {
            stop = true;
        }

        private void threadStart()
        {
            VectorGenerator vectorGenerator = new VectorGenerator(paths);

            while (!stop)
            {
                if (vectorGenerator.Step(TimeSpan.FromSeconds(1.0)))
                {
                    VectorPath = vectorGenerator.Generate(VectorPower, VectorSpeed);
                    GCodeProgram = GCodeGenerator.Generate(VectorPath, 1000.0, maxFeed);

                    Generated?.Invoke(this, null);
                }
            }

            thread = null;
        }
    }
}
