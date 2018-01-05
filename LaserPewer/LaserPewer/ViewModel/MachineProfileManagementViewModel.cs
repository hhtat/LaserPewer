using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using LaserPewer.Model;

namespace LaserPewer.ViewModel
{
    public class MachineProfileManagementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ObservableCollection<MachineProfileViewModel> profileViewModels;
        private readonly Dictionary<MachineProfileManager.Profile, MachineProfileViewModel> profileToViewModels;
        public ICollectionView Profiles { get { return CollectionViewSource.GetDefaultView(profileViewModels); } }

        private MachineProfileViewModel _active;
        public MachineProfileViewModel Active
        {
            get { return _active; }
            set
            {
                if (value.Model != AppCore.MachineProfiles.Active) AppCore.MachineProfiles.Active = value.Model;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        private readonly RelayCommand _activateCommand;
        public ICommand ActivateCommand { get { return _activateCommand; } }

        private readonly RelayCommand _duplicateCommand;
        public ICommand DuplicateCommand { get { return _duplicateCommand; } }

        private readonly RelayCommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand; } }

        public MachineProfileManagementViewModel()
        {
            profileViewModels = new ObservableCollection<MachineProfileViewModel>();
            profileToViewModels = new Dictionary<MachineProfileManager.Profile, MachineProfileViewModel>();
            Profiles.SortDescriptions.Add(new SortDescription("FriendlyName", ListSortDirection.Ascending));

            foreach (MachineProfileManager.Profile profile in AppCore.MachineProfiles.Profiles)
            {
                addProfile(profile);
            }
            if (AppCore.MachineProfiles.Active != null) Active = profileToViewModels[AppCore.MachineProfiles.Active];

            _activateCommand = new RelayCommand(_activateCommand_Execute);
            _duplicateCommand = new RelayCommand(_duplicateCommand_Execute);
            _deleteCommand = new RelayCommand(_deleteCommand_Execute);

            AppCore.MachineProfiles.ProfileAdded += MachineProfiles_ProfileAdded;
            AppCore.MachineProfiles.ProfileRemoved += MachineProfiles_ProfileRemoved;
            AppCore.MachineProfiles.ActiveChanged += MachineProfiles_ActiveChanged;
        }

        private void addProfile(MachineProfileManager.Profile profile)
        {
            MachineProfileViewModel vm = new MachineProfileViewModel(profile);
            profileViewModels.Add(vm);
            profileToViewModels.Add(profile, vm);
        }

        private void _activateCommand_Execute(object parameter)
        {
            MachineProfileViewModel vm = parameter as MachineProfileViewModel;
            if (vm == null) return;
            Active = vm;
        }

        private void _duplicateCommand_Execute(object parameter)
        {
            MachineProfileViewModel vm = parameter as MachineProfileViewModel;
            if (vm == null) return;
            AppCore.MachineProfiles.AddProfile(new MachineProfileManager.Profile(
                vm.Model.FriendlyName + " (Duplicate)", vm.Model.TableSize, vm.Model.MaxFeedRate));
        }

        private void _deleteCommand_Execute(object parameter)
        {
            MachineProfileViewModel vm = parameter as MachineProfileViewModel;
            if (vm == null || vm.Model == AppCore.MachineProfiles.Active) return;
            AppCore.MachineProfiles.RemoveProfile(vm.Model);
        }

        private void MachineProfiles_ProfileAdded(object sender, MachineProfileManager.Profile profile)
        {
            addProfile(profile);
        }

        private void MachineProfiles_ProfileRemoved(object sender, MachineProfileManager.Profile profile)
        {
            MachineProfileViewModel vm = profileToViewModels[profile];
            profileViewModels.Remove(vm);
            profileToViewModels.Remove(profile);
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineProfileManager.Profile profile, MachineProfileManager.Profile old)
        {
            Active = profileToViewModels[profile];
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
