using System;
using System.Windows;
using System.Windows.Threading;

namespace LaserPewer
{
    public partial class MainWindow : Window
    {
        private GrblMachine machine;
        private DispatcherTimer statusRequestTimer;

        public MainWindow()
        {
            InitializeComponent();

            machine = new GrblMachine();
            machine.StatusUpdated += Machine_StatusUpdated;
            machine.Connect("COM4");

            statusRequestTimer = new DispatcherTimer();
            statusRequestTimer.Interval = TimeSpan.FromMilliseconds(500);
            statusRequestTimer.Tick += StatusRequestTimer_Tick;
            statusRequestTimer.Start();
        }

        private void Machine_StatusUpdated(object sender, GrblMachine.MachineStatus status)
        {
            xTextBlock.Text = status.X.ToString("F3");
            yTextBlock.Text = status.Y.ToString("F3");
            statusTextBlock.Text = status.Status;
        }

        private void StatusRequestTimer_Tick(object sender, EventArgs e)
        {
            machine.PollStatus();
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
    }
}
