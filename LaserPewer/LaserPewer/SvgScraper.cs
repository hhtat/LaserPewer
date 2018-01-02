using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LaserPewer
{
    public class SvgScraper : ISvgRenderer
    {
        private const byte POINT_TYPE_MASK = 0x07;
        private const byte POINT_TYPE_START = 0x00;
        private const byte POINT_TYPE_LINE = 0x01;
        private const byte POINT_TYPE_BEZIER = 0x03;
        private const byte POINT_TYPE_FLAG_MARKER = 0x20;
        private const byte POINT_TYPE_FLAG_CLOSE = 0x80;

        public float DpiY { get { return 96.0f; } }
        public SmoothingMode SmoothingMode { get; set; }
        public Matrix Transform { get; set; }

        public readonly List<Drawing.Path> ScrapedPaths;

        private Stack<ISvgBoundable> boundables;
        private Region clip;

        public SvgScraper()
        {
            SmoothingMode = SmoothingMode.Default;
            Transform = new Matrix();

            ScrapedPaths = new List<Drawing.Path>();

            boundables = new Stack<ISvgBoundable>();
            clip = new Region();
        }

        public void Dispose()
        {
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
        {
        }

        public void DrawImageUnscaled(Image image, System.Drawing.Point location)
        {
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            GraphicsPath _path = (GraphicsPath)path.Clone();
            _path.Transform(Transform);

            List<System.Windows.Point> points = null;
            for (int i = 0; i < _path.PointCount; i++)
            {
                byte pointType = _path.PathTypes[i];
                PointF point = _path.PathPoints[i];

                switch (pointType & POINT_TYPE_MASK)
                {
                    case POINT_TYPE_START:
                        if (points != null && points.Count > 1)
                        {
                            ScrapedPaths.Add(new Drawing.Path(points));
                        }
                        points = new List<System.Windows.Point>();
                        break;
                    case POINT_TYPE_LINE:
                    case POINT_TYPE_BEZIER:
                        break;
                    default:
                        throw new NotSupportedException();
                }

                points.Add(new System.Windows.Point(25.4 * point.X / DpiY, 25.4 * point.Y / DpiY));

                if ((pointType & POINT_TYPE_FLAG_CLOSE) == POINT_TYPE_FLAG_CLOSE)
                {
                    points.Add(points[0]);
                }
            }

            if (points != null && points.Count > 1)
            {
                ScrapedPaths.Add(new Drawing.Path(points));
            }
        }

        public void FillPath(Brush brush, GraphicsPath path)
        {
        }

        public ISvgBoundable GetBoundable()
        {
            return boundables.Peek();
        }

        public void SetBoundable(ISvgBoundable boundable)
        {
            boundables.Push(boundable);
        }

        public ISvgBoundable PopBoundable()
        {
            return boundables.Pop();
        }

        public Region GetClip()
        {
            return clip;
        }

        public void SetClip(Region region, CombineMode combineMode = CombineMode.Replace)
        {
            switch (combineMode)
            {
                case CombineMode.Replace:
                    clip = region;
                    break;
                case CombineMode.Intersect:
                    clip.Intersect(region);
                    break;
                case CombineMode.Union:
                    clip.Union(region);
                    break;
                case CombineMode.Xor:
                    clip.Xor(region);
                    break;
                case CombineMode.Exclude:
                    clip.Exclude(region);
                    break;
                case CombineMode.Complement:
                    clip.Complement(region);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Append)
        {
            Transform.Translate(dx, dy, order);
        }

        public void RotateTransform(float fAngle, MatrixOrder order = MatrixOrder.Append)
        {
            Transform.Rotate(fAngle, order);
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Append)
        {
            Transform.Scale(sx, sy, order);
        }
    }
}
