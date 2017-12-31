using System;
using System.Windows;
using System.Windows.Input;

namespace LaserPewer
{
    public class WorkbenchInput
    {
        private bool leftPressed;
        private Point startingPointMM;

        public WorkbenchInput(Workbench workbench)
        {
            workbench.MouseLeave += Workbench_MouseLeave;

            workbench.MouseDown += Workbench_MouseDown;
            workbench.MouseUp += Workbench_MouseUp;

            workbench.MouseMove += Workbench_MouseMove;

            workbench.MouseWheel += Workbench_MouseWheel;
        }

        private void Workbench_MouseLeave(object sender, MouseEventArgs e)
        {
            leftPressed = false;
        }

        private void Workbench_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                leftPressed = true;
                Workbench workbench = (Workbench)sender;
                startingPointMM = workbench.GetPointMMAtOffset(e.GetPosition((Workbench)sender));
            }
        }

        private void Workbench_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) leftPressed = false;
        }

        private void Workbench_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftPressed)
            {
                Workbench workbench = (Workbench)sender;
                Point offset = e.GetPosition(workbench);
                Point newOffset = workbench.GetOffsetAtPointMM(startingPointMM);
                workbench.Pan(new Point(newOffset.X - offset.X, newOffset.Y - offset.Y));
            }
        }

        private void Workbench_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Workbench workbench = (Workbench)sender;

            Point offset = e.GetPosition(workbench);
            Point originalMM = workbench.GetPointMMAtOffset(offset);
            workbench.Zoom *= Math.Pow(2.0, e.Delta / 480.0);
            Point newOffset = workbench.GetOffsetAtPointMM(originalMM);
            workbench.Pan(new Point(newOffset.X - offset.X, newOffset.Y - offset.Y));
        }
    }
}
