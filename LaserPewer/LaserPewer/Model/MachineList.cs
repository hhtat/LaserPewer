﻿using System;
using System.Collections.Generic;
using System.Windows;

namespace LaserPewer.Model
{
    public class MachineList
    {
        public delegate void MachineListEventHandler(object sender, IProfile profile);
        public event MachineListEventHandler ProfileAdded;
        public event MachineListEventHandler ProfileRemoved;
        public event MachineListEventHandler ProfileModified;

        public delegate void MachineSwapEventHandler(object sender, IProfile profile, IProfile old);
        public event MachineSwapEventHandler ActiveChanged;

        private readonly List<IProfile> profiles;
        public IReadOnlyList<IProfile> Profiles { get { return profiles.AsReadOnly(); } }

        private IProfile _active;
        public IProfile Active
        {
            get { return _active; }
            set
            {
                if (!profiles.Contains(value)) throw new ArgumentException();
                IProfile old = _active;
                _active = value;
                ActiveChanged?.Invoke(this, value, old);
            }
        }

        public MachineList()
        {
            profiles = new List<IProfile>();
        }

        public void CreateProfile(string friendlyName, Size tableSize, double maxFeedRate)
        {
            Profile profile = new Profile(friendlyName, tableSize, maxFeedRate);
            profiles.Add(profile);
            profile.Modified += Profile_Modified;
            ProfileAdded?.Invoke(this, profile);
        }

        public void RemoveProfile(IProfile profile)
        {
            if (profile == Active) throw new ArgumentException();
            if (!profiles.Remove(profile)) throw new ArgumentException();
            profile.Modified -= Profile_Modified;
            ProfileRemoved?.Invoke(this, profile);
        }

        private void Profile_Modified(object sender, EventArgs e)
        {
            ProfileModified?.Invoke(this, (IProfile)sender);
        }

        public interface IProfile
        {
            event EventHandler Modified;

            string FriendlyName { get; set; }
            Size TableSize { get; set; }
            double MaxFeedRate { get; set; }
        }

        private class Profile : IProfile
        {
            public event EventHandler Modified;

            private string _friendlyName;
            public string FriendlyName
            {
                get { return _friendlyName; }
                set { _friendlyName = value; Modified?.Invoke(this, null); }
            }

            private Size _tableSize;
            public Size TableSize
            {
                get { return _tableSize; }
                set { _tableSize = value; Modified?.Invoke(this, null); }
            }

            private double _maxFeedRate;
            public double MaxFeedRate
            {
                get { return _maxFeedRate; }
                set { _maxFeedRate = value; Modified?.Invoke(this, null); }
            }

            public Profile(string friendlyName, Size tableSize, double maxFeedRate)
            {
                FriendlyName = friendlyName;
                TableSize = tableSize;
                MaxFeedRate = maxFeedRate;
            }
        }
    }
}