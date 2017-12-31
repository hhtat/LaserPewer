using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaserPewer
{
    public class Workbench : UserControl
    {
        private readonly int GRAPHICS_SIZE = 2048;

        private Size tableSizeMM;
        public Size TableSizeMM
        {
            get { return tableSizeMM; }
            set
            {
                if (tableSizeMM != value)
                {
                    tableSizeMM = value;
                    graphicsStale = true;
                }
            }
        }
        private Point centerMM;
        public Point CenterMM
        {
            get { return centerMM; }
            set
            {
                if (centerMM != value)
                {
                    centerMM = value;
                    graphicsStale = true;
                }
            }
        }
        private double zoom;
        public double Zoom
        {
            get { return zoom; }
            set
            {
                if (double.IsNaN(value)) return;
                if (value < 1.0) value = 1.0;
                if (value > 10.0) value = 10.0;
                if (zoom != value)
                {
                    zoom = value;
                    graphicsStale = true;
                }
            }
        }

        private Point pointerMM;
        public Point PointerMM
        {
            get { return pointerMM; }
            set
            {
                if (pointerMM != value)
                {
                    pointerMM = value;
                    graphicsStale = true;
                }
            }
        }

        private Image mainImage;
        private WriteableBitmap writeableBitmap;

        private WorkbenchInput input;

        private Point graphicsCenter;
        private bool graphicsStale;

        public Workbench()
        {
            mainImage = new Image();
            writeableBitmap = BitmapFactory.New(GRAPHICS_SIZE, GRAPHICS_SIZE);
            mainImage.Source = writeableBitmap;
            mainImage.Stretch = Stretch.None;
            Content = mainImage;

            input = new WorkbenchInput(this);

            TableSizeMM = new Size(200.0, 200.0);
            Zoom = 1.0;

            SizeChanged += WorkbenchControl_SizeChanged;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void Pan(Point offset)
        {
            Point centerOffset = GetOffsetAtPointMM(CenterMM);
            CenterMM = GetPointMMAtOffset(new Point(centerOffset.X + offset.X, centerOffset.Y + offset.Y));
        }

        public Point GetPointMMAtOffset(Point offset)
        {
            Point offsetGraphics = TranslatePoint(offset, mainImage);
            return new Point(
                CenterMM.X + (offsetGraphics.X - graphicsCenter.X) / Zoom,
                CenterMM.Y + (offsetGraphics.Y - graphicsCenter.Y) / Zoom);
        }

        public Point GetOffsetAtPointMM(Point pointMM)
        {
            Point offsetGraphics = new Point(fwdXd(pointMM.X), fwdYd(pointMM.Y));
            return mainImage.TranslatePoint(offsetGraphics, this);
        }

        private void drawLineMM(double x1, double y1, double x2, double y2, Color color)
        {
            writeableBitmap.DrawLine(fwdXi(x1), fwdYi(y1), fwdXi(x2), fwdYi(y2), color);
        }

        private void drawPointerMM(double x, double y, Color color)
        {
            int xi = fwdXi(x);
            int yi = fwdYi(y);

            writeableBitmap.DrawEllipseCentered(xi, yi, 10, 10, color);
            writeableBitmap.DrawEllipseCentered(xi, yi, 5, 5, color);
        }

        private void fillRectMM(double x1, double y1, double x2, double y2, Color color)
        {
            writeableBitmap.FillRectangle(fwdXi(x1), fwdYi(y1), fwdXi(x2), fwdYi(y2), color);
        }

        private double fwdXd(double x)
        {
            return graphicsCenter.X + Zoom * (x - CenterMM.X);
        }

        private double fwdYd(double y)
        {
            return graphicsCenter.Y + Zoom * (y - CenterMM.Y);
        }

        private int fwdXi(double x)
        {
            return (int)Math.Round(fwdXd(x));
        }

        private int fwdYi(double y)
        {
            return (int)Math.Round(fwdYd(y));
        }

        private void WorkbenchControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            graphicsCenter = new Point(
                (ActualWidth < GRAPHICS_SIZE ? ActualWidth : GRAPHICS_SIZE) / 2.0,
                (ActualHeight < GRAPHICS_SIZE ? ActualHeight : GRAPHICS_SIZE) / 2.0);
            graphicsStale = true;
        }

        private void CompositionTarget_Rendering(object sender, System.EventArgs e)
        {
            if (graphicsStale)
            {
                using (writeableBitmap.GetBitmapContext())
                {
                    writeableBitmap.Clear();

                    fillRectMM(0.0, 0.0, TableSizeMM.Width, TableSizeMM.Height, Colors.White);

                    for (int x = 10; x < TableSizeMM.Width; x += 10) drawLineMM(x, 0.0, x, TableSizeMM.Height, Colors.LightGray);
                    for (int y = 10; y < TableSizeMM.Height; y += 10) drawLineMM(0.0, y, TableSizeMM.Width, y, Colors.LightGray);
                    for (int x = 50; x < TableSizeMM.Width; x += 50) drawLineMM(x, 0.0, x, TableSizeMM.Height, Colors.Gray);
                    for (int y = 50; y < TableSizeMM.Height; y += 50) drawLineMM(0.0, y, TableSizeMM.Width, y, Colors.Gray);

                    drawPointerMM(pointerMM.X, pointerMM.Y, Colors.Red);
                }

                graphicsStale = false;
            }
        }
    }
}
