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

        public static MachineList MachineList { get { return Instance._machineList; } }
        public static GrblMachine Machine { get { return Instance._machine; } }
        public static Document Document { get { return Instance._document; } }
        public static ProgramGenerator Generator { get { return Instance._generator; } }
        public static GrblSender Sender { get { return Instance._sender; } }

        private readonly MachineList _machineList;
        private readonly GrblMachine _machine;
        private readonly Document _document;
        private readonly ProgramGenerator _generator;
        private readonly GrblSender _sender;

        private readonly PersistentSettings settings;
        private readonly DispatcherTimer settingsTimer;

        private AppCore()
        {
            _machineList = new MachineList();
            _machine = new GrblMachine();
            _document = new Document();
            _generator = new ProgramGenerator();
            _sender = new GrblSender(_machine);

            _machineList.ProfileAdded += _machineList_EventHandler;
            _machineList.ProfileRemoved += _machineList_EventHandler;
            _machineList.ProfileModified += _machineList_EventHandler;

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
                _machineList.CreateProfile("Default Machine", new Size(300.0, 200.0), Corner.TopLeft, 10000.0);
            }

            if (_machineList.Active == null)
            {
                _machineList.Active = _machineList.Profiles[0];
            }
        }

        public void Deinitialize()
        {
            settings.Save();
        }

        private void _machineList_EventHandler(object sender, MachineList.IProfile profile)
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
