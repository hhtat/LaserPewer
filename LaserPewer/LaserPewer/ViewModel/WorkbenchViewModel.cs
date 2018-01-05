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
            if (AppCore.MachineProfiles.Active != null)
            {
                MachineSize = AppCore.MachineProfiles.Active.TableSize;
                AppCore.MachineProfiles.Active.Modified += MachineProfile_Modified;
            }

            AppCore.MachineProfiles.ActiveChanged += MachineProfiles_ActiveChanged;
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;
        }

        private void MachineProfile_Modified(object sender, EventArgs e)
        {
            MachineSize = ((MachineProfileManager.Profile)sender).TableSize;
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineProfileManager.Profile profile, MachineProfileManager.Profile old)
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
