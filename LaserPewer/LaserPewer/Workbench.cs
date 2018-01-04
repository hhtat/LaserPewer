using LaserPewer.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaserPewer
{
    public class Workbench : UserControl
    {
        public const double ZoomMin = 1.0;
        public const double ZoomMax = 50.0;

        private const int GRAPHICS_SIZE = 2048;

        public static readonly DependencyProperty TableSizeProperty =
            DependencyProperty.Register("TableSize", typeof(Size), typeof(Workbench),
                new PropertyMetadata(
                    new Size(0.0, 0.0),
                    (d, e) => ((Workbench)d).graphicsStale = true));

        public static readonly DependencyProperty ViewCenterProperty =
            DependencyProperty.Register("ViewCenter", typeof(Point), typeof(Workbench),
                new PropertyMetadata(
                    new Point(0.0, 0.0),
                    (d, e) => ((Workbench)d).graphicsStale = true));

        public static readonly DependencyProperty ViewZoomProperty =
            DependencyProperty.Register("ViewZoom", typeof(double), typeof(Workbench),
                new PropertyMetadata(
                    1.0,
                    (d, e) => ((Workbench)d).graphicsStale = true),
                (value) => ((double)value >= ZoomMin && (double)value <= ZoomMax));

        public static readonly DependencyProperty MachinePositionProperty =
            DependencyProperty.Register("MachinePosition", typeof(Point), typeof(Workbench),
                new PropertyMetadata(
                    new Point(0.0, 0.0),
                    (d, e) => ((Workbench)d).graphicsStale = true));

        public Size TableSize
        {
            get { return (Size)GetValue(TableSizeProperty); }
            set { SetValue(TableSizeProperty, value); }
        }

        public Point ViewCenter
        {
            get { return (Point)GetValue(ViewCenterProperty); }
            set { SetValue(ViewCenterProperty, value); }
        }

        public double ViewZoom
        {
            get { return (double)GetValue(ViewZoomProperty); }
            set { SetValue(ViewZoomProperty, value); }
        }

        public Point MachinePosition
        {
            get { return (Point)GetValue(MachinePositionProperty); }
            set { SetValue(MachinePositionProperty, value); }
        }

        public Document Document { get; private set; }

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

            Document = new Document();

            SizeChanged += WorkbenchControl_SizeChanged;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void Pan(Point offset)
        {
            Point centerOffset = GetOffsetAtPointMM(ViewCenter);
            ViewCenter = GetPointMMAtOffset(new Point(centerOffset.X + offset.X, centerOffset.Y + offset.Y));
        }

        public Point GetPointMMAtOffset(Point offset)
        {
            Point offsetGraphics = TranslatePoint(offset, mainImage);
            return new Point(
                ViewCenter.X + (offsetGraphics.X - graphicsCenter.X) / ViewZoom,
                ViewCenter.Y + (offsetGraphics.Y - graphicsCenter.Y) / ViewZoom);
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

            writeableBitmap.DrawLine(xi - 20, yi, xi - 5, yi, color);
            writeableBitmap.DrawLine(xi + 20, yi, xi + 5, yi, color);
            writeableBitmap.DrawLine(xi, yi - 20, xi, yi - 5, color);
            writeableBitmap.DrawLine(xi, yi + 20, xi, yi + 5, color);
            writeableBitmap.DrawEllipseCentered(xi, yi, 15, 15, color);
            writeableBitmap.DrawEllipseCentered(xi, yi, 10, 10, color);
        }

        private void fillRectMM(double x1, double y1, double x2, double y2, Color color)
        {
            writeableBitmap.FillRectangle(fwdXi(x1), fwdYi(y1), fwdXi(x2), fwdYi(y2), color);
        }

        private double fwdXd(double x)
        {
            return graphicsCenter.X + ViewZoom * (x - ViewCenter.X);
        }

        private double fwdYd(double y)
        {
            return graphicsCenter.Y + ViewZoom * (y - ViewCenter.Y);
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
            if (graphicsStale || Document.Stale)
            {
                using (writeableBitmap.GetBitmapContext())
                {
                    writeableBitmap.Clear();

                    fillRectMM(0.0, 0.0, TableSize.Width, TableSize.Height, Colors.White);

                    for (int x = 10; x < TableSize.Width; x += 10) drawLineMM(x, 0.0, x, TableSize.Height, Color.FromRgb(0xF0, 0xF0, 0xF0));
                    for (int y = 10; y < TableSize.Height; y += 10) drawLineMM(0.0, y, TableSize.Width, y, Color.FromRgb(0xF0, 0xF0, 0xF0));
                    for (int x = 50; x < TableSize.Width; x += 50) drawLineMM(x, 0.0, x, TableSize.Height, Color.FromRgb(0xE0, 0xE0, 0xE0));
                    for (int y = 50; y < TableSize.Height; y += 50) drawLineMM(0.0, y, TableSize.Width, y, Color.FromRgb(0xE0, 0xE0, 0xE0));

                    drawPointerMM(MachinePosition.X, MachinePosition.Y, Colors.Red);

                    foreach (Model.Drawing drawing in Document.Drawings)
                    {
                        foreach (Model.Drawing.Path path in drawing.Paths)
                        {
                            if (path.Points.Count >= 2)
                            {
                                Point prev = path.Points[0];
                                for (int i = 1; i < path.Points.Count + 1; i++)
                                {
                                    int j = i % path.Points.Count;
                                    if (j == 0 && !path.Closed) break;
                                    Point point = path.Points[j];
                                    drawLineMM(prev.X, prev.Y, point.X, point.Y, j % 2 == 0 ? Colors.Red : Colors.Green);
                                    prev = point;
                                }
                            }
                        }
                    }
                }

                graphicsStale = false;
                Document.Refresh();
            }
        }
    }
}
