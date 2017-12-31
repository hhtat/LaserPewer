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

            workbench.TableSizeMM = new Size(370, 230);
            workbench.CenterMM = new Point(workbench.TableSizeMM.Width / 2.0, workbench.TableSizeMM.Height / 2.0);

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

            workbench.PointerMM = new Point(status.X, status.Y);
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

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void holdButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
