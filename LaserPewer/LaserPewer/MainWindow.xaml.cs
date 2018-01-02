using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LaserPewer
{
    public partial class MainWindow : Window
    {
        private const string FIELD_PLACEHOLDER = "-";

        private MachineProfile currentProfile;
        private DispatcherTimer statusRequestTimer;

        public MainWindow()
        {
            InitializeComponent();

            refreshProfiles(AppCore.Profiles[0]);

            AppCore.Machine.MachineDisconnected += Machine_MachineDisconnected;
            AppCore.Machine.MachineReset += Machine_MachineReset;
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;
            AppCore.Machine.AlarmRaised += Machine_AlarmRaised;
            AppCore.Machine.MessageFeedback += Machine_MessageFeedback;

            statusRequestTimer = new DispatcherTimer();
            statusRequestTimer.Interval = TimeSpan.FromMilliseconds(500);
            statusRequestTimer.Tick += StatusRequestTimer_Tick;
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

            workbench.TableSizeMM = new Size(currentProfile.TableWidth, currentProfile.TableHeight);
            workbench.CenterMM = new Point(currentProfile.TableWidth / 2.0, currentProfile.TableHeight / 2.0);
        }

        private void Machine_MachineDisconnected(object sender, EventArgs e)
        {
            statusRequestTimer.Stop();
            statusTextBlock.Text = "Disconnected";
        }

        private void Machine_MachineReset(object sender, EventArgs e)
        {
            xTextBlock.Text = "0";
            yTextBlock.Text = "0";
            statusTextBlock.Text = FIELD_PLACEHOLDER;
            messageTextBlock.Text = FIELD_PLACEHOLDER;
        }

        private void Machine_StatusUpdated(object sender, GrblMachine.MachineStatus status)
        {
            xTextBlock.Text = status.X.ToString("F3", CultureInfo.InvariantCulture);
            yTextBlock.Text = status.Y.ToString("F3", CultureInfo.InvariantCulture);
            statusTextBlock.Text = status.Status;

            if (alarmTextBlock.Text != FIELD_PLACEHOLDER && status.Status != "Alarm")
            {
                alarmTextBlock.Text = FIELD_PLACEHOLDER;
            }

            workbench.PointerMM = new Point(status.X, -status.Y);
        }

        private void Machine_AlarmRaised(object sender, int alarm)
        {
            alarmTextBlock.Text = alarm.ToString();
        }

        private void Machine_MessageFeedback(object sender, string message)
        {
            messageTextBlock.Text = message;
        }

        private void StatusRequestTimer_Tick(object sender, EventArgs e)
        {
            AppCore.Machine.PollStatus();
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
                        statusRequestTimer.Start();
                        statusTextBlock.Text = "Unknown";
                    }
                    else
                    {
                        statusTextBlock.Text = "Connection Error";
                    }
                }
                else
                {
                    AppCore.Machine.Disconnect();
                }
            }

            Opacity = 1.0;
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            AppCore.Machine.Reset();
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            AppCore.Machine.Home();
        }

        private void unlockButton_Click(object sender, RoutedEventArgs e)
        {
            AppCore.Machine.Unlock();
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void holdButton_Click(object sender, RoutedEventArgs e)
        {
            AppCore.Machine.Hold();
        }

        private void workbench_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pointMM = workbench.GetPointMMAtOffset(e.GetPosition(workbench));
            AppCore.Machine.Jog(pointMM.X, -pointMM.Y, currentProfile.MaxFeedRate);
        }
    }
}
