using System;

namespace LaserPewer.GA
{
    public struct Interval
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Length { get { return End - Start; } }

        public Interval(Random random, int limit)
        {
            Start = random.Next(limit);
            End = random.Next(limit);

            if (End < Start)
            {
                int temp = Start;
                Start = End;
                End = temp;
            }

            End++;
        }
    }
}
