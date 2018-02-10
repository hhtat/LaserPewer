using LaserPewer.Grbl;
using LaserPewer.Shared;
using LaserPewer.Utilities;
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

        public static MachineProfiles MachineProfiles { get { return Instance._machineProfiles; } }
        public static LaserMachine Machine { get { return Instance._machine; } }
        public static Document Document { get { return Instance._document; } }
        public static ProgramGenerator Generator { get { return Instance._generator; } }

        private readonly MachineProfiles _machineProfiles;
        private readonly LaserMachine _machine;
        private readonly Document _document;
        private readonly ProgramGenerator _generator;

        private readonly PersistentSettings settings;
        private readonly DispatcherTimer settingsTimer;

        private AppCore()
        {
            _machineProfiles = new MachineProfiles();
            _machine = new GrblMachine();
            _document = new Document();
            _generator = new ProgramGenerator();

            _machineProfiles.ProfileAdded += _machineProfiles_EventHandler;
            _machineProfiles.ProfileRemoved += _machineProfiles_EventHandler;
            _machineProfiles.ProfileModified += _machineProfiles_EventHandler;

            settings = new PersistentSettings();
            settingsTimer = new DispatcherTimer();
            settingsTimer.Interval = TimeSpan.FromSeconds(10);
            settingsTimer.Tick += settingsTimer_Tick;
        }

        public void Initialize()
        {
            settings.Load();

            if (_machineProfiles.Profiles.Count == 0)
            {
                _machineProfiles.CreateProfile("Default Machine", new Size(300.0, 200.0), Corner.TopLeft, 10000.0);
            }

            if (_machineProfiles.Active == null)
            {
                _machineProfiles.Active = _machineProfiles.Profiles[0];
            }
        }

        public void Deinitialize()
        {
            settings.Save();
            _machine.Dispose();
        }

        private void _machineProfiles_EventHandler(object sender, MachineProfiles.IProfile profile)
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
