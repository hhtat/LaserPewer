using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using LaserPewer.Model;

namespace LaserPewer.ViewModel
{
    public class MachineProfileManagerViewModel : INotifyPropertyChanged
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

        public MachineProfileManagerViewModel()
        {
            profileViewModels = new ObservableCollection<MachineProfileViewModel>();
            profileToViewModels = new Dictionary<MachineProfileManager.Profile, MachineProfileViewModel>();
            Profiles.SortDescriptions.Add(new SortDescription("FriendlyName", ListSortDirection.Ascending));

            foreach (MachineProfileManager.Profile profile in AppCore.MachineProfiles.Profiles)
            {
                addProfile(profile);
            }
            if (AppCore.MachineProfiles.Active != null) Active = profileToViewModels[AppCore.MachineProfiles.Active];

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
