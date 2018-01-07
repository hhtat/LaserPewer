﻿using System;
using System.Windows;

namespace LaserPewer.Utilities
{
    public class Precision
    {
        public static double Round3(double value)
        {
            return Math.Round(value, 3);
        }

        public static Point Round3(Point point)
        {
            return new Point(Round3(point.X), Round3(point.Y));
        }

        public static Size Round3(Size size)
        {
            return new Size(Round3(size.Width), Round3(size.Height));
        }
    }
}