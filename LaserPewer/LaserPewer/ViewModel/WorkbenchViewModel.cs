using LaserPewer.Model;
using System;
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
            if (AppCore.MachineList.Active != null)
            {
                MachineSize = AppCore.MachineList.Active.TableSize;
                AppCore.MachineList.Active.Modified += MachineProfile_Modified;
            }

            AppCore.MachineList.ActiveChanged += MachineProfiles_ActiveChanged;
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;
        }

        private void MachineProfile_Modified(object sender, EventArgs e)
        {
            MachineSize = ((MachineList.Profile)sender).TableSize;
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineList.Profile profile, MachineList.Profile old)
        {
            MachineSize = profile.TableSize;
            profile.Modified += MachineProfile_Modified;
            if (old != null) old.Modified -= MachineProfile_Modified;
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
