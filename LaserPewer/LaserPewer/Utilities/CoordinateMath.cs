using System;
using System.Windows;

namespace LaserPewer.Utilities
{
    public static class CoordinateMath
    {
        public static Point MinExtent(Size size, Corner origin)
        {
            switch (origin)
            {
                case Corner.TopLeft: return new Point(0.0, -size.Height);
                case Corner.TopRight: return new Point(-size.Width, -size.Height);
                case Corner.BottomLeft: return new Point(0.0, 0.0);
                case Corner.BottomRight: return new Point(-size.Width, 0.0);
            }

            throw new NotSupportedException();
        }

        public static Point MaxExtent(Size size, Corner origin)
        {
            switch (origin)
            {
                case Corner.TopLeft: return new Point(size.Width, 0.0);
                case Corner.TopRight: return new Point(0.0, 0.0);
                case Corner.BottomLeft: return new Point(size.Width, size.Height);
                case Corner.BottomRight: return new Point(0.0, size.Height);
            }

            throw new NotSupportedException();
        }

        public static Point FarExtent(Size size, Corner origin)
        {
            switch (origin)
            {
                case Corner.TopLeft: return new Point(size.Width, -size.Height);
                case Corner.TopRight: return new Point(-size.Width, -size.Height);
                case Corner.BottomLeft: return new Point(size.Width, size.Height);
                case Corner.BottomRight: return new Point(-size.Width, size.Height);
            }

            throw new NotSupportedException();
        }

        public static Point AtCorner(Corner corner, Size size, Corner origin)
        {
            Point minExtent = MinExtent(size, origin);
            Point maxExtent = MaxExtent(size, origin);

            switch (corner)
            {
                case Corner.TopLeft: return new Point(minExtent.X, maxExtent.Y);
                case Corner.TopRight: return new Point(maxExtent.X, maxExtent.Y);
                case Corner.BottomLeft: return new Point(minExtent.X, minExtent.Y);
                case Corner.BottomRight: return new Point(maxExtent.X, minExtent.Y);
            }

            throw new NotSupportedException();
        }
    }
}
