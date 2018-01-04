using System;
using System.Collections.Generic;
using System.Windows;

namespace LaserPewer.Model
{
    public class MachineProfileManager
    {
        public delegate void ProfileEventHandler(object sender, Profile profile);
        public event ProfileEventHandler ProfileAdded;
        public event ProfileEventHandler ProfileRemoved;
        public event ProfileEventHandler ActiveChanged;

        private readonly List<Profile> profiles;
        public IReadOnlyList<Profile> Profiles { get { return profiles.AsReadOnly(); } }

        private Profile _active;
        public Profile Active
        {
            get { return _active; }
            set
            {
                if (!profiles.Contains(value)) throw new ArgumentException();
                _active = value;
                ActiveChanged?.Invoke(this, value);
            }
        }

        public MachineProfileManager()
        {
            profiles = new List<Profile>();
        }

        public void AddProfile(Profile profile)
        {
            if (profiles.Contains(profile)) throw new ArgumentException();
            profiles.Add(profile);
            ProfileAdded?.Invoke(this, profile);
        }

        public void RemoveProfile(Profile profile)
        {
            if (profile == Active) throw new ArgumentException();
            if (!profiles.Remove(profile)) throw new ArgumentException();
            ProfileRemoved?.Invoke(this, profile);
        }

        public class Profile
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
        }
    }
}
