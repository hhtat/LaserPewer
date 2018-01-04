using LaserPewer.Model;
using Svg;
using System.Windows;
using System.Windows.Controls;

namespace LaserPewer
{
    public partial class MainWindow : Window
    {
        private const string FIELD_PLACEHOLDER = "-";

        private MachineProfile currentProfile;

        public MainWindow()
        {
            InitializeComponent();

            refreshProfiles(AppCore.Profiles[0]);
        }

        private void refreshProfiles(MachineProfile selectedProfile)
        {
            if (selectedProfile == null)
            {
                selectedProfile = AppCore.Profiles[0];
            }

            machineListComboBox.Items.Clear();
            foreach (MachineProfile profile in AppCore.Profiles)
            {
                machineListComboBox.Items.Add(profile.FriendlyName);
            }

            machineListComboBox.SelectedIndex = AppCore.Instance.ProfileIndex(selectedProfile);
        }

        private void loadProfile(MachineProfile profile)
        {
            currentProfile = profile;

            //workbench.TableSizeMM = new Size(currentProfile.TableWidth, currentProfile.TableHeight);
            //workbench.CenterMM = new Point(currentProfile.TableWidth / 2.0, currentProfile.TableHeight / 2.0);
        }

        private void machineListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (machineListComboBox.SelectedIndex < 0 ||
                machineListComboBox.SelectedIndex > AppCore.Profiles.Count)
            {
                return;
            }

            loadProfile(AppCore.Profiles[machineListComboBox.SelectedIndex]);
        }

        private void machineManageButton_Click(object sender, RoutedEventArgs e)
        {
            MachineProfileDialog dialog = new MachineProfileDialog(currentProfile) { Owner = this };
            Opacity = 0.8;
            if (dialog.ShowDialog() ?? false) refreshProfiles(dialog.Profile);
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
