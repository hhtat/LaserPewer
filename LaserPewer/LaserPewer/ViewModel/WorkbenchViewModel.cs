using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LaserPewer.ViewModel
{
    public class WorkbenchViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Size _machineSize;
        public Size MachineSize
        {
            get { return _machineSize; }
            private set { _machineSize = value; NotifyPropertyChanged(); }
        }

        private Point _machinePosition;
        public Point MachinePosition
        {
            get { return _machinePosition; }
            private set { _machinePosition = value; NotifyPropertyChanged(); }
        }

        public WorkbenchViewModel()
        {
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;
        }

        private void Machine_StatusUpdated(object sender, GrblMachine.MachineStatus status)
        {
            MachinePosition = new Point(status.X, status.Y);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
