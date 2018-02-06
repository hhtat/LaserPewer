using LaserPewer.Generation;
using LaserPewer.Model;
using LaserPewer.Utilities;
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

        public Size MachineSize { get { return AppCore.MachineProfiles.Active.TableSize; } }

        public Corner MachineOrigin { get { return AppCore.MachineProfiles.Active.Origin; } }

        private Point _machinePosition;
        public Point MachinePosition
        {
            get { return _machinePosition; }
            private set { _machinePosition = value; NotifyPropertyChanged(); }
        }

        public Document Document { get { return AppCore.Document; } }

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
            if (AppCore.MachineProfiles.Active != null)
            {
                AppCore.MachineProfiles.Active.Modified += MachineProfile_Modified;
                updateProfile(AppCore.MachineProfiles.Active);
            }

            AppCore.MachineProfiles.ActiveChanged += MachineProfiles_ActiveChanged;
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;

            AppCore.Generator.Completed += Generator_Completed;
        }

        private void updateProfile(MachineProfiles.IProfile profile)
        {
            NotifyPropertyChanged(nameof(MachineSize));
            NotifyPropertyChanged(nameof(MachineOrigin));
            Point extent = CoordinateMath.FarExtent(profile.TableSize, profile.Origin);
            ViewCenter = new Point(extent.X / 2.0, extent.Y / 2.0);
        }

        private void MachineProfile_Modified(object sender, EventArgs e)
        {
            updateProfile(AppCore.MachineProfiles.Active);
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineProfiles.IProfile profile, MachineProfiles.IProfile old)
        {
            if (old != null) old.Modified -= MachineProfile_Modified;
            profile.Modified += MachineProfile_Modified;
            updateProfile(profile);
        }

        private void Machine_StatusUpdated(object sender, GrblMachine.MachineStatus status)
        {
            MachinePosition = new Point(status.X, status.Y);
        }

        private void Generator_Completed(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(MachinePath));
            MachinePathProgress = 1.0;
        }
    }
}
