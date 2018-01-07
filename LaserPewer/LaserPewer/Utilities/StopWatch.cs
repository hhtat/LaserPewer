using System;
using System.Diagnostics;

namespace LaserPewer.Utilities
{
    public class StopWatch
    {
        private DateTime lastTime;

        public StopWatch()
        {
            Reset();
        }

        public void Reset()
        {
            lastTime = DateTime.UtcNow;
        }

        public void TraceLap(string message)
        {
            TimeSpan lapTime = DateTime.UtcNow - lastTime;
            Debug.WriteLine(lapTime.TotalMilliseconds + "ms - " + message);
            Reset();
        }
    }
}
