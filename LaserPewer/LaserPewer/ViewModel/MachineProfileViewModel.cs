using LaserPewer.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LaserPewer.ViewModel
{
    public class MachineProfileViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public readonly MachineProfileManager.Profile Model;

        private string _friendlyName;
        public string FriendlyName
        {
            get { return _friendlyName; }
            set { _friendlyName = value; NotifyPropertyChanged(); }
        }

        public MachineProfileViewModel(MachineProfileManager.Profile profile)
        {
            Model = profile;
            reloadProfile();
            Model.Modified += Profile_Modified;
        }

        private void reloadProfile()
        {
            FriendlyName = Model.FriendlyName;
        }

        private void Profile_Modified(object sender, EventArgs e)
        {
            reloadProfile();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
