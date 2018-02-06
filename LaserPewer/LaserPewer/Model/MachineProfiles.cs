using LaserPewer.Utilities;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LaserPewer.Model
{
    public class MachineProfiles
    {
        public delegate void MachineProfilesEventHandler(object sender, IProfile profile);
        public event MachineProfilesEventHandler ProfileAdded;
        public event MachineProfilesEventHandler ProfileRemoved;
        public event MachineProfilesEventHandler ProfileModified;

        public delegate void MachineSwapEventHandler(object sender, IProfile profile, IProfile old);
        public event MachineSwapEventHandler ActiveChanged;

        private readonly List<IProfile> profiles;
        public IReadOnlyList<IProfile> Profiles { get { return profiles; } }

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

        public MachineProfiles()
        {
            profiles = new List<IProfile>();
        }

        public void CreateProfile(string friendlyName, Size tableSize, Corner origin, double maxFeedRate)
        {
            CreateProfile(Guid.NewGuid(), friendlyName, tableSize, origin, maxFeedRate);
        }

        public void CreateProfile(Guid uniqueId, string friendlyName, Size tableSize, Corner origin, double maxFeedRate)
        {
            Profile profile = new Profile(uniqueId, friendlyName, tableSize, origin, maxFeedRate);
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

            Guid UniqueId { get; }

            string FriendlyName { get; set; }
            Size TableSize { get; set; }
            Corner Origin { get; set; }
            double MaxFeedRate { get; set; }
        }

        private class Profile : IProfile
        {
            public event EventHandler Modified;

            public Guid UniqueId { get; private set; }

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

            private Corner _origin;
            public Corner Origin
            {
                get { return _origin; }
                set { _origin = value; Modified?.Invoke(this, null); }
            }

            private double _maxFeedRate;
            public double MaxFeedRate
            {
                get { return _maxFeedRate; }
                set { _maxFeedRate = value; Modified?.Invoke(this, null); }
            }

            public Profile(Guid uniqueId, string friendlyName, Size tableSize, Corner origin, double maxFeedRate)
            {
                UniqueId = uniqueId;
                FriendlyName = friendlyName;
                TableSize = tableSize;
                Origin = origin;
                MaxFeedRate = maxFeedRate;
            }
        }
    }
}
