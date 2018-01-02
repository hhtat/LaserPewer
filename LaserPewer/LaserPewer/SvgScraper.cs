﻿using Svg;
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

        private const double TOLERANCE_MM_SQ = 0.1 * 0.1;
        private const int ADAPTIVE_BEZIER_MAX_DEPTH = 8;

        private double dpi;
        private double mmpd;
        public float DpiY { get { return (float)dpi; } set { dpi = value; mmpd = 25.4 / dpi; } }
        public SmoothingMode SmoothingMode { get; set; }
        public Matrix Transform { get; set; }

        private Stack<ISvgBoundable> boundables;
        private Region clip;

        private DrawingBuilder drawingBuilder;


        public SvgScraper()
        {
            DpiY = 96.0f;
            SmoothingMode = SmoothingMode.Default;
            Transform = new Matrix();

            boundables = new Stack<ISvgBoundable>();
            clip = new Region();

            drawingBuilder = new DrawingBuilder();
        }

        public Drawing CollectScraped()
        {
            return drawingBuilder.ToDrawing();
        }

        public void Dispose()
        {
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
        {
        }

        public void DrawImageUnscaled(Image image, Point location)
        {
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            GraphicsPath _path = (GraphicsPath)path.Clone();
            _path.Transform(Transform);

            PointF lastPoint = PointF.Empty;
            for (int i = 0; i < _path.PointCount; i++)
            {
                byte pointType = _path.PathTypes[i];
                PointF point = _path.PathPoints[i];

                switch (pointType & POINT_TYPE_MASK)
                {
                    case POINT_TYPE_START:
                        drawingBuilder.StartPath();
                        drawingBuilder.AddPoint(mmpd * point.X, mmpd * point.Y);
                        lastPoint = point;
                        break;
                    case POINT_TYPE_LINE:
                        drawingBuilder.AddPoint(mmpd * point.X, mmpd * point.Y);
                        lastPoint = point;
                        break;
                    case POINT_TYPE_BEZIER:
                        PointF point2 = _path.PathPoints[++i];
                        PointF point3 = _path.PathPoints[++i];
                        traceBezier(lastPoint, point, point2, point3);
                        lastPoint = point3;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if ((pointType & POINT_TYPE_FLAG_CLOSE) == POINT_TYPE_FLAG_CLOSE) drawingBuilder.ClosePath();
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

        private void traceBezier(PointF a, PointF b, PointF c, PointF d)
        {
            adaptiveBezier(mmpd * a.X, mmpd * a.Y, mmpd * b.X, mmpd * b.Y, mmpd * c.X, mmpd * c.Y, mmpd * d.X, mmpd * d.Y, 0);
            drawingBuilder.AddPoint(mmpd * d.X, mmpd * d.Y); ;
        }

        void adaptiveBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, int depth)
        {
            double x12 = (x1 + x2) / 2;
            double y12 = (y1 + y2) / 2;
            double x23 = (x2 + x3) / 2;
            double y23 = (y2 + y3) / 2;
            double x34 = (x3 + x4) / 2;
            double y34 = (y3 + y4) / 2;
            double x123 = (x12 + x23) / 2;
            double y123 = (y12 + y23) / 2;
            double x234 = (x23 + x34) / 2;
            double y234 = (y23 + y34) / 2;
            double x1234 = (x123 + x234) / 2;
            double y1234 = (y123 + y234) / 2;

            double dx = x4 - x1;
            double dy = y4 - y1;

            double d2 = Math.Abs(((x2 - x4) * dy - (y2 - y4) * dx));
            double d3 = Math.Abs(((x3 - x4) * dy - (y3 - y4) * dx));

            if ((d2 + d3) * (d2 + d3) < TOLERANCE_MM_SQ * (dx * dx + dy * dy) || depth >= ADAPTIVE_BEZIER_MAX_DEPTH)
            {
                drawingBuilder.AddPoint(x1234, y1234);
                return;
            }

            adaptiveBezier(x1, y1, x12, y12, x123, y123, x1234, y1234, depth + 1);
            adaptiveBezier(x1234, y1234, x234, y234, x34, y34, x4, y4, depth + 1);
        }
    }
}
