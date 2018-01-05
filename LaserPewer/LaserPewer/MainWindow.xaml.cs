using LaserPewer.Model;
using Svg;
using System.Windows;

namespace LaserPewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void machineProfilesManageButton_Click(object sender, RoutedEventArgs e)
        {
            MachineListWindow dialog = new MachineListWindow() { Owner = this };
            Opacity = 0.8;
            dialog.ShowDialog();
            Opacity = 1.0;
        }

        private void connectionButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionDialog dialog = new ConnectionDialog() { Owner = this };
            Opacity = 0.8;

            if (dialog.ShowDialog() ?? false)
            {
                if (dialog.SelectedPortName != null)
                {
                    if (AppCore.Machine.Connect(dialog.SelectedPortName))
                    {
                        //statusRequestTimer.Start();
                        //statusTextBlock.Text = "Unknown";
                    }
                    else
                    {
                        //statusTextBlock.Text = "Connection Error";
                    }
                }
                else
                {
                    AppCore.Machine.Disconnect();
                }
            }

            Opacity = 1.0;
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            SvgDocument svgDocument = SvgDocument.Open(@"C:\Users\hhtat\Desktop\test.svg");

            SvgScraper svgScraper = new SvgScraper();
            svgDocument.Draw(svgScraper);

            Drawing drawing = svgScraper.CreateDrawing();
            Size svgSize = new Size(
                Optimizer.Round3(svgScraper.GetWidth(svgDocument)),
                Optimizer.Round3(svgScraper.GetHeight(svgDocument)));
            drawing.Clip(new Rect(svgSize));

            //workbench.Document.Clear();
            //workbench.Document.Add(drawing);
        }

        //private void workbench_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    Point pointMM = workbench.GetPointMMAtOffset(e.GetPosition(workbench));
        //    AppCore.Machine.Jog(pointMM.X, -pointMM.Y, currentProfile.MaxFeedRate);
        //}
    }
}
