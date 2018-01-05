﻿using LaserPewer.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LaserPewer.ViewModel
{
    public class MachineProfileViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public readonly MachineProfileManager.Profile Model;

        public string FriendlyName
        {
            get { return Model.FriendlyName; }
            set
            {
                if (value.Length > 0) Model.FriendlyName = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("ListDisplayName");
            }
        }

        public string ListDisplayName
        {
            get
            {
                if (Model != AppCore.MachineProfiles.Active) return FriendlyName;
                return FriendlyName + " (Active)";
            }
        }

        public double TableWidth
        {
            get { return Model.TableSize.Width; }
            set
            {
                if (value > 0.0 && value < 1000.0) Model.TableSize = new Size(value, TableHeight);
                NotifyPropertyChanged();
            }
        }

        public double TableHeight
        {
            get { return Model.TableSize.Height; }
            set
            {
                if (value > 0.0 && value < 1000.0) Model.TableSize = new Size(TableWidth, value);
                NotifyPropertyChanged();
            }
        }

        public double MaxFeedRate
        {
            get { return Model.MaxFeedRate; }
            set
            {
                if (value > 0.0 && value < 100000.0) Model.MaxFeedRate = value;
                NotifyPropertyChanged();
            }
        }

        public MachineProfileViewModel(MachineProfileManager.Profile profile)
        {
            Model = profile;
            Model.Modified += Profile_Modified;
            AppCore.MachineProfiles.ActiveChanged += MachineProfiles_ActiveChanged;
        }

        private void MachineProfiles_ActiveChanged(object sender, MachineProfileManager.Profile profile, MachineProfileManager.Profile old)
        {
            if (Model == profile || Model == old) NotifyPropertyChanged("ListDisplayName");
        }

        private void Profile_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged("FriendlyName");
            NotifyPropertyChanged("ListDisplayName");
            NotifyPropertyChanged("TableWidth");
            NotifyPropertyChanged("TableHeight");
            NotifyPropertyChanged("MaxFeedRate");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}