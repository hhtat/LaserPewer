using LaserPewer.Generation;
using LaserPewer.Geometry;
using LaserPewer.Model;
using LaserPewer.Utilities;
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

        public static readonly DependencyProperty TableOriginProperty =
            DependencyProperty.Register("TableOrigin", typeof(Corner), typeof(Workbench),
                new PropertyMetadata(
                    Corner.TopLeft,
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
                value => ((double)value >= ZoomMin && (double)value <= ZoomMax));

        public static readonly DependencyProperty MachinePositionProperty =
            DependencyProperty.Register("MachinePosition", typeof(Point), typeof(Workbench),
                new PropertyMetadata(
                    new Point(0.0, 0.0),
                    (d, e) => ((Workbench)d).graphicsStale = true));

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(Document), typeof(Workbench),
                new PropertyMetadata(null, DocumentPropertyChanged));

        public static readonly DependencyProperty MachinePathProperty =
            DependencyProperty.Register("MachinePath", typeof(MachinePath), typeof(Workbench),
                new PropertyMetadata(
                    null,
                    (d, e) => ((Workbench)d).graphicsStale = true));

        public static readonly DependencyProperty MachinePathFrameProperty =
            DependencyProperty.Register("MachinePathFrame", typeof(int), typeof(Workbench),
                new PropertyMetadata(
                    0,
                    (d, e) => ((Workbench)d).graphicsStale = true));

        public Size TableSize
        {
            get { return (Size)GetValue(TableSizeProperty); }
            set { SetValue(TableSizeProperty, value); }
        }

        public Corner TableOrigin
        {
            get { return (Corner)GetValue(TableOriginProperty); }
            set { SetValue(TableOriginProperty, value); }
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

        public Document Document
        {
            get { return (Document)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public MachinePath MachinePath
        {
            get { return (MachinePath)GetValue(MachinePathProperty); }
            set { SetValue(MachinePathProperty, value); }
        }

        public int MachinePathFrame
        {
            get { return (int)GetValue(MachinePathFrameProperty); }
            set { SetValue(MachinePathFrameProperty, value); }
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

            SizeChanged += WorkbenchControl_SizeChanged;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void Pan(Vector delta)
        {
            ViewCenter = GetPointMMAtOffset(GetOffsetAtPointMM(ViewCenter) + delta);
        }

        public Point GetPointMMAtOffset(Point offset)
        {
            Point offsetGraphics = TranslatePoint(offset, mainImage);
            return new Point(
                ViewCenter.X + (offsetGraphics.X - graphicsCenter.X) / ViewZoom,
                ViewCenter.Y - (offsetGraphics.Y - graphicsCenter.Y) / ViewZoom);
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

        private void drawRectMM(double x1, double y1, double x2, double y2, Color color)
        {
            writeableBitmap.DrawRectangle(fwdXi(x1), fwdYi(y2), fwdXi(x2), fwdYi(y1), color);
        }

        private void fillRectMM(double x1, double y1, double x2, double y2, Color color)
        {
            writeableBitmap.FillRectangle(fwdXi(x1), fwdYi(y2), fwdXi(x2), fwdYi(y1), color);
        }

        private double fwdXd(double x)
        {
            return graphicsCenter.X + ViewZoom * (x - ViewCenter.X);
        }

        private double fwdYd(double y)
        {
            return graphicsCenter.Y - ViewZoom * (y - ViewCenter.Y);
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

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (graphicsStale)
            {
                using (writeableBitmap.GetBitmapContext())
                {
                    writeableBitmap.Clear();

                    Point minExtent = CornerMath.MinExtent(TableSize, TableOrigin);
                    Point maxExtent = CornerMath.MaxExtent(TableSize, TableOrigin);

                    fillRectMM(minExtent.X, minExtent.Y, maxExtent.X, maxExtent.Y, Colors.White);

                    int gridXMin = 50 * ((int)Math.Round(minExtent.X / 50.0) - 2);
                    int gridYMin = 50 * ((int)Math.Round(minExtent.Y / 50.0) - 2);
                    int gridXMax = 50 * ((int)Math.Round(maxExtent.X / 50.0) + 2);
                    int gridYMax = 50 * ((int)Math.Round(maxExtent.Y / 50.0) + 2);

                    for (int x = gridXMin + 10; x < gridXMax; x += 10) drawLineMM(x, gridYMin, x, gridYMax, Color.FromRgb(0xF0, 0xF0, 0xF0));
                    for (int y = gridYMin + 10; y < gridYMax; y += 10) drawLineMM(gridXMin, y, gridXMax, y, Color.FromRgb(0xF0, 0xF0, 0xF0));
                    for (int x = gridXMin + 50; x < gridXMax; x += 50) drawLineMM(x, gridYMin, x, gridYMax, Color.FromRgb(0xE0, 0xE0, 0xE0));
                    for (int y = gridYMin + 50; y < gridYMax; y += 50) drawLineMM(gridXMin, y, gridXMax, y, Color.FromRgb(0xE0, 0xE0, 0xE0));

                    drawRectMM(minExtent.X, minExtent.Y, maxExtent.X, maxExtent.Y, Color.FromRgb(0x80, 0x80, 0x80));
                    drawLineMM(gridXMin, 0.0, gridXMax, 0.0, Color.FromRgb(0x80, 0x80, 0x80));
                    drawLineMM(0.0, gridYMin, 0.0, gridYMax, Color.FromRgb(0x80, 0x80, 0x80));

                    drawPointerMM(MachinePosition.X, MachinePosition.Y, Colors.Red);

                    if (Document != null && Document.Drawing != null)
                    {
                        Vector offset = Document.Offset;

                        foreach (Path path in Document.Drawing.Paths)
                        {
                            if (path.Points.Count >= 2)
                            {
                                Point prev = path.Points[0] + offset;
                                for (int i = 1; i < path.Points.Count + 1; i++)
                                {
                                    if (i == path.Points.Count && !path.Closed) break;
                                    Point point = path.Points[i % path.Points.Count] + offset;
                                    drawLineMM(prev.X, prev.Y, point.X, point.Y, path.Closed ? Colors.Blue : Colors.Green);
                                    prev = point;
                                }
                            }
                        }
                    }

                    if (MachinePath != null)
                    {
                        Point prev = new Point(0.0, 0.0);
                        int indexCap = Math.Min(MachinePathFrame, MachinePath.Travels.Count);
                        for (int i = 0; i < indexCap; i++)
                        {
                            MachinePath.Travel travel = MachinePath.Travels[i];
                            drawLineMM(prev.X, prev.Y, travel.Destination.X, travel.Destination.Y, !travel.Rapid ? Colors.Red : Colors.Purple);
                            prev = travel.Destination;
                        }
                    }
                }

                graphicsStale = false;
            }
        }

        private static void DocumentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Workbench _this = (Workbench)d;
            if (e.OldValue != null) ((Document)e.OldValue).Modified -= _this.Document_Modified;
            if (e.NewValue != null) ((Document)e.NewValue).Modified += _this.Document_Modified;
            _this.graphicsStale = true;
        }

        private void Document_Modified(object sender, EventArgs e)
        {
            graphicsStale = true;
        }
    }
}
