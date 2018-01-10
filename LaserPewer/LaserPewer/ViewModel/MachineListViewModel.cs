using LaserPewer.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace LaserPewer.ViewModel
{
    public class MachineListViewModel : BaseViewModel
    {
        private readonly ObservableCollection<MachineProfileViewModel> profileViewModels;
        private readonly Dictionary<MachineList.IProfile, MachineProfileViewModel> profileToViewModels;
        public ICollectionView Profiles { get { return CollectionViewSource.GetDefaultView(profileViewModels); } }

        private MachineProfileViewModel _active;
        public MachineProfileViewModel Active
        {
            get { return _active; }
            set
            {
                if (value.Model != AppCore.MachineList.Active) AppCore.MachineList.Active = value.Model;
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

        public MachineListViewModel()
        {
            profileViewModels = new ObservableCollection<MachineProfileViewModel>();
            profileToViewModels = new Dictionary<MachineList.IProfile, MachineProfileViewModel>();
            Profiles.SortDescriptions.Add(new SortDescription("FriendlyName", ListSortDirection.Ascending));

            foreach (MachineList.IProfile profile in AppCore.MachineList.Profiles)
            {
                addProfile(profile);
            }
            if (AppCore.MachineList.Active != null) Active = profileToViewModels[AppCore.MachineList.Active];

            _activateCommand = new RelayCommand(_activateCommand_Execute);
            _duplicateCommand = new RelayCommand(_duplicateCommand_Execute);
            _deleteCommand = new RelayCommand(_deleteCommand_Execute);

            AppCore.MachineList.ProfileAdded += MachineProfiles_ProfileAdded;
            AppCore.MachineList.ProfileRemoved += MachineProfiles_ProfileRemoved;
            AppCore.MachineList.ActiveChanged += MachineProfiles_ActiveChanged;
        }

        private void addProfile(MachineList.IProfile profile)
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
            AppCore.MachineList.CreateProfile(
                vm.Model.FriendlyName + " (Duplicate)",
                vm.Model.TableSize,
                vm.Model.Origin,
                vm.Model.MaxFeedRate);
        }

        private void _deleteCommand_Execute(object parameter)
        {
            MachineProfileViewModel vm = parameter as MachineProfileViewModel;
            if (vm == null || vm.Model == AppCore.MachineList.Active) return;
            AppCore.MachineList.RemoveProfile(vm.Model);
        }

        private void MachineProfiles_ProfileAdded(object sender, MachineList.IProfile profile)
        {
            addProfile(profile);
        }

        private void MachineProfiles_ProfileRemoved(object sender, MachineList.IProfile profile)
        {
            MachineProfileViewModel vm = profileToViewModels[profile];
            profileViewModels.Remove(vm);
            profileToViewModels.Remove(profile);
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineList.IProfile profile, MachineList.IProfile old)
        {
            Active = profileToViewModels[profile];
        }
    }
}
