using System;
using System.Windows;
using System.Windows.Threading;

namespace LaserPewer.Model
{
    public class AppCore
    {
        private static AppCore singleton;
        public static AppCore Instance
        {
            get
            {
                if (singleton == null) singleton = new AppCore();
                return singleton;
            }
        }

        public static MachineList MachineList { get { return Instance._machineList; } }
        public static GrblMachine Machine { get { return Instance._machine; } }

        private readonly MachineList _machineList;
        private readonly GrblMachine _machine;

        private readonly PersistentSettings settings;
        private readonly DispatcherTimer settingsTimer;

        private AppCore()
        {
            _machineList = new MachineList();
            _machine = new GrblMachine();

            _machineList.ProfileAdded += _machineList_ProfileEventHandler;
            _machineList.ProfileRemoved += _machineList_ProfileEventHandler;
            _machineList.ProfileModified += _machineList_ProfileEventHandler;

            settings = new PersistentSettings();
            settingsTimer = new DispatcherTimer();
            settingsTimer.Interval = TimeSpan.FromSeconds(10);
            settingsTimer.Tick += settingsTimer_Tick;
        }

        public void Initialize()
        {
            settings.Load();

            if (_machineList.Profiles.Count == 0)
            {
                _machineList.AddProfile(
                    new MachineList.Profile("Default Machine", new Size(300.0, 200.0), 10000.0));
            }

            _machineList.Active = _machineList.Profiles[0];
        }

        public void Deinitialize()
        {
            settings.Save();
        }

        private void _machineList_ProfileEventHandler(object sender, MachineList.Profile profile)
        {
            settingsTimer.Start();
        }

        private void settingsTimer_Tick(object sender, EventArgs e)
        {
            settingsTimer.Stop();
            settings.Save();
        }
    }
}
