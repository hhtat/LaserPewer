using LaserPewer.Model;
using System;
using System.Windows;

namespace LaserPewer.ViewModel
{
    public class MachineProfileViewModel : BaseViewModel
    {
        public readonly MachineList.IProfile Model;

        public string FriendlyName
        {
            get { return Model.FriendlyName; }
            set
            {
                if (Model.FriendlyName != value && value.Length > 0) Model.FriendlyName = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ListDisplayName));
            }
        }

        public string ListDisplayName
        {
            get
            {
                if (Model != AppCore.MachineList.Active) return FriendlyName;
                return FriendlyName + " [Current]";
            }
        }

        public double TableWidth
        {
            get { return Model.TableSize.Width; }
            set
            {
                if (Model.TableSize.Width != value && value > 0.0 && value < 1000.0)
                {
                    Model.TableSize = new Size(value, TableHeight);
                }
                NotifyPropertyChanged();
            }
        }

        public double TableHeight
        {
            get { return Model.TableSize.Height; }
            set
            {
                if (Model.TableSize.Height != value && value > 0.0 && value < 1000.0)
                {
                    Model.TableSize = new Size(TableWidth, value);
                }
                NotifyPropertyChanged();
            }
        }

        public double MaxFeedRate
        {
            get { return Model.MaxFeedRate; }
            set
            {
                if (Model.MaxFeedRate != value && value > 0.0 && value < 100000.0)
                {
                    Model.MaxFeedRate = value;
                }
                NotifyPropertyChanged();
            }
        }

        public MachineProfileViewModel(MachineList.IProfile profile)
        {
            Model = profile;
            Model.Modified += Profile_Modified;
            AppCore.MachineList.ActiveChanged += MachineProfiles_ActiveChanged;
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineList.IProfile profile, MachineList.IProfile old)
        {
            if (Model == profile || Model == old) NotifyPropertyChanged(nameof(ListDisplayName));
        }

        private void Profile_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(FriendlyName));
            NotifyPropertyChanged(nameof(ListDisplayName));
            NotifyPropertyChanged(nameof(TableWidth));
            NotifyPropertyChanged(nameof(TableHeight));
            NotifyPropertyChanged(nameof(MaxFeedRate));
        }
    }
}
