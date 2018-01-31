using System;
using System.Diagnostics;

namespace LaserPewer.Shared
{
    public class StopWatch
    {
        private DateTime lastTime;

        public StopWatch()
        {
            Reset();
        }

        public void Zero()
        {
            lastTime = DateTime.MinValue;
        }

        public void Reset()
        {
            lastTime = DateTime.UtcNow;
        }

        public TimeSpan Elapsed()
        {
            return DateTime.UtcNow - lastTime;
        }

        public bool Expired(TimeSpan timeout)
        {
            return Elapsed() >= timeout;
        }

        public void TraceLap(string message)
        {
            TimeSpan lapTime = DateTime.UtcNow - lastTime;
            Debug.WriteLine(lapTime.TotalMilliseconds + "ms - " + message);
            Reset();
        }
    }
}
