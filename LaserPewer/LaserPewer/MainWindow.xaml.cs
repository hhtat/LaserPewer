using System;
using System.Globalization;
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
        private GrblMachine machine;
        private DispatcherTimer statusRequestTimer;

        public MainWindow()
        {
            InitializeComponent();

            foreach (MachineProfile profile in AppCore.Instance.Profiles)
            {
                machineListComboBox.Items.Add(profile.FriendlyName);
            }
            machineListComboBox.SelectedIndex = 0;

            machine = new GrblMachine();
            machine.MachineReset += Machine_MachineReset;
            machine.StatusUpdated += Machine_StatusUpdated;
            machine.AlarmRaised += Machine_AlarmRaised;
            machine.MessageFeedback += Machine_MessageFeedback;
            machine.Connect("COM4");
            machine.Reset();

            statusRequestTimer = new DispatcherTimer();
            statusRequestTimer.Interval = TimeSpan.FromMilliseconds(500);
            statusRequestTimer.Tick += StatusRequestTimer_Tick;
            statusRequestTimer.Start();
        }

        private void loadProfile(MachineProfile profile)
        {
            currentProfile = profile;

            workbench.TableSizeMM = new Size(currentProfile.TableWidth, currentProfile.TableHeight);
            workbench.CenterMM = new Point(currentProfile.TableWidth / 2.0, currentProfile.TableHeight / 2.0);
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
            machine.PollStatus();
        }

        private void machineListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (machineListComboBox.SelectedIndex < 0 || machineListComboBox.SelectedIndex > AppCore.Instance.Profiles.Count)
            {
                machineListComboBox.SelectedIndex = 0;
            }

            loadProfile(AppCore.Instance.Profiles[machineListComboBox.SelectedIndex]);
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            machine.Reset();
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            machine.Home();
        }

        private void unlockButton_Click(object sender, RoutedEventArgs e)
        {
            machine.Unlock();
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void holdButton_Click(object sender, RoutedEventArgs e)
        {
            machine.Hold();
        }

        private void workbench_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pointMM = workbench.GetPointMMAtOffset(e.GetPosition(workbench));
            machine.Jog(pointMM.X, -pointMM.Y, currentProfile.MaxFeedRate);
        }
    }
}
