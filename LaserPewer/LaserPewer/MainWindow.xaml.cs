using LaserPewer.Model;
using System.Windows;
using System.Windows.Input;

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
            MachineProfilesWindow dialog = new MachineProfilesWindow() { Owner = this };
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
                    AppCore.Machine.ConnectAsync(dialog.SelectedPortName);
                }
                else
                {
                    AppCore.Machine.DisconnectAsync();
                }
            }

            Opacity = 1.0;
        }

        private void workbench_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point point = workbench.GetPointMMAtOffset(e.GetPosition(workbench));
            //AppCore.Machine.Jog(point.X, point.Y, AppCore.MachineProfiles.Active.MaxFeedRate);
        }

        private void viewGCodeButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramViewerWindow dialog = new ProgramViewerWindow() { Owner = this };
            Opacity = 0.8;
            dialog.ShowDialog();
            Opacity = 1.0;
        }
    }
}
