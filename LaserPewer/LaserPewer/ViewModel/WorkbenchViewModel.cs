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

        private Point _viewCenter;
        public Point ViewCenter
        {
            get { return _viewCenter; }
            private set { _viewCenter = value; NotifyPropertyChanged(); }
        }

        public Size MachineSize { get { return AppCore.MachineList.Active.TableSize; } }

        private Point _machinePosition;
        public Point MachinePosition
        {
            get { return _machinePosition; }
            private set { _machinePosition = value; NotifyPropertyChanged(); }
        }

        public Drawing Drawing { get { return AppCore.Document.Drawing; } }

        public WorkbenchViewModel()
        {
            if (AppCore.MachineList.Active != null)
            {
                ViewCenter = new Point(
                    AppCore.MachineList.Active.TableSize.Width / 2.0,
                    AppCore.MachineList.Active.TableSize.Height / 2.0);
                AppCore.MachineList.Active.Modified += MachineProfile_Modified;
            }

            AppCore.MachineList.ActiveChanged += MachineProfiles_ActiveChanged;
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;

            AppCore.Document.Modified += Document_Modified;
        }

        private void MachineProfile_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged("MachineSize");
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineList.IProfile profile, MachineList.IProfile old)
        {
            ViewCenter = new Point(profile.TableSize.Width / 2.0, profile.TableSize.Height / 2.0);
            NotifyPropertyChanged("MachineSize");
            profile.Modified += MachineProfile_Modified;
            if (old != null) old.Modified -= MachineProfile_Modified;
        }

        private void Machine_StatusUpdated(object sender, GrblMachine.MachineStatus status)
        {
            MachinePosition = new Point(status.X, status.Y);
        }

        private void Document_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Drawing");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
