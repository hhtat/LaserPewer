using LaserPewer.Generation;
using LaserPewer.Model;
using System;
using System.Windows;

namespace LaserPewer.ViewModel
{
    public class WorkbenchViewModel : BaseViewModel
    {
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

        public MachinePath MachinePath { get { return AppCore.Generator.VectorPath; } }

        public int MachinePathFrame { get { return MachinePath != null ? (int)Math.Round(MachinePathProgress * MachinePath.Travels.Count) : 0; } }

        private double _machinePathProgress;
        public double MachinePathProgress
        {
            get { return _machinePathProgress; }
            set
            {
                _machinePathProgress = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(MachinePathFrame));
            }
        }

        public WorkbenchViewModel()
        {
            if (AppCore.MachineList.Active != null)
            {
                ViewCenter = new Point(
                    AppCore.MachineList.Active.TableSize.Width / 2.0,
                    -AppCore.MachineList.Active.TableSize.Height / 2.0);
                AppCore.MachineList.Active.Modified += MachineProfile_Modified;
            }

            AppCore.MachineList.ActiveChanged += MachineProfiles_ActiveChanged;
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;

            AppCore.Document.Modified += Document_Modified;

            AppCore.Generator.Completed += Generator_Completed;
        }

        private void MachineProfile_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(MachineSize));
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineList.IProfile profile, MachineList.IProfile old)
        {
            ViewCenter = new Point(profile.TableSize.Width / 2.0, profile.TableSize.Height / 2.0);
            NotifyPropertyChanged(nameof(MachineSize));
            profile.Modified += MachineProfile_Modified;
            if (old != null) old.Modified -= MachineProfile_Modified;
        }

        private void Machine_StatusUpdated(object sender, GrblMachine.MachineStatus status)
        {
            MachinePosition = new Point(status.X, status.Y);
        }

        private void Document_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(Drawing));
        }

        private void Generator_Completed(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(MachinePath));
            MachinePathProgress = 1.0;
        }
    }
}
