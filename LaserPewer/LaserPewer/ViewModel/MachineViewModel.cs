using LaserPewer.Model;
using System;
using System.Globalization;
using System.Windows.Input;

namespace LaserPewer.ViewModel
{
    public class MachineViewModel : BaseViewModel
    {
        private string _positionX;
        public string PositionX
        {
            get { return _positionX; }
            private set { _positionX = value; NotifyPropertyChanged(); }
        }

        private string _positionY;
        public string PositionY
        {
            get { return _positionY; }
            private set { _positionY = value; NotifyPropertyChanged(); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            private set { _status = value; NotifyPropertyChanged(); }
        }

        private string _alarm;
        public string Alarm
        {
            get { return _alarm; }
            private set { _alarm = value; NotifyPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            private set { _message = value; NotifyPropertyChanged(); }
        }

        private readonly RelayCommand _resetCommand;
        public ICommand ResetCommand { get { return _resetCommand; } }

        private readonly RelayCommand _homeCommand;
        public ICommand HomeCommand { get { return _homeCommand; } }

        private readonly RelayCommand _unlockCommand;
        public ICommand UnlockCommand { get { return _unlockCommand; } }

        private readonly RelayCommand _resumeCommand;
        public ICommand ResumeCommand { get { return _resumeCommand; } }

        private readonly RelayCommand _holdCommand;
        public ICommand HoldCommand { get { return _holdCommand; } }

        private readonly RelayCommand _startCommand;
        public ICommand StartCommand { get { return _startCommand; } }

        public MachineViewModel()
        {
            setDefaults();

            _resetCommand = new RelayCommand(parameter => AppCore.Machine.Reset(), parameter => AppCore.Machine.Connected);
            _homeCommand = new RelayCommand(parameter => AppCore.Machine.Home(), parameter => AppCore.Machine.Connected);
            _unlockCommand = new RelayCommand(parameter => AppCore.Machine.Unlock(), parameter => AppCore.Machine.Connected);
            _resumeCommand = new RelayCommand(parameter => AppCore.Machine.Resume(), parameter => AppCore.Machine.Connected);
            _holdCommand = new RelayCommand(parameter => AppCore.Machine.Hold(), parameter => AppCore.Machine.Connected);
            _startCommand = new RelayCommand(
                parameter => AppCore.Sender.Start(AppCore.Generator.GCodeProgram),
                parameter => AppCore.Machine.Connected && AppCore.Generator.GCodeProgram != null);

            AppCore.Machine.MachineConnecting += Machine_MachineConnecting;
            AppCore.Machine.MachineConnected += Machine_MachineConnected;
            AppCore.Machine.MachineDisconnected += Machine_MachineDisconnected;
            AppCore.Machine.MachineReset += Machine_MachineReset;
            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;
            AppCore.Machine.AlarmRaised += Machine_AlarmRaised;
            AppCore.Machine.MessageFeedback += Machine_MessageFeedback;

            AppCore.Generator.Completed += Generator_Completed;
        }

        private void setDefaults()
        {
            PositionX = "000.000";
            PositionY = "000.000";
            Status = "Disconnected";
            Alarm = "None";
            Message = "None";
        }

        private void Machine_MachineConnecting(object sender, EventArgs e)
        {
            Status = "Connecting";
        }

        private void Machine_MachineConnected(object sender, EventArgs e)
        {
            Status = "Connected";

            _resetCommand.NotifyCanExecuteChanged();
            _homeCommand.NotifyCanExecuteChanged();
            _unlockCommand.NotifyCanExecuteChanged();
            _resumeCommand.NotifyCanExecuteChanged();
            _holdCommand.NotifyCanExecuteChanged();
            _startCommand.NotifyCanExecuteChanged();
        }

        private void Machine_MachineDisconnected(object sender, EventArgs e)
        {
            Status = "Disconnected";

            _resetCommand.NotifyCanExecuteChanged();
            _homeCommand.NotifyCanExecuteChanged();
            _unlockCommand.NotifyCanExecuteChanged();
            _resumeCommand.NotifyCanExecuteChanged();
            _holdCommand.NotifyCanExecuteChanged();
            _startCommand.NotifyCanExecuteChanged();
        }

        private void Machine_MachineReset(object sender, EventArgs e)
        {
            setDefaults();
        }

        private void Machine_StatusUpdated(object sender, GrblMachine.MachineStatus status)
        {
            PositionX = toDisplayString(status.X);
            PositionY = toDisplayString(status.Y);
            Status = status.Status;

            if (Alarm != string.Empty && status.Status != "Alarm")
            {
                Alarm = string.Empty;
            }
        }

        private void Machine_AlarmRaised(object sender, int alarm)
        {
            Alarm = alarm.ToString();
        }

        private void Machine_MessageFeedback(object sender, string message)
        {
            Message = message;
        }


        private void Generator_Completed(object sender, EventArgs e)
        {
            _startCommand.NotifyCanExecuteChanged();
        }

        private static string toDisplayString(double value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }
    }
}
