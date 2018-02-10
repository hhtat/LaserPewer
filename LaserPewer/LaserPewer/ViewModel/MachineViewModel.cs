using LaserPewer.Model;
using LaserPewer.Shared;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace LaserPewer.ViewModel
{
    public class MachineViewModel : BaseViewModel
    {
        private string _positionX;
        public string PositionX
        {
            get { return _positionX; }
            private set
            {
                if (value != _positionX)
                {
                    _positionX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _positionY;
        public string PositionY
        {
            get { return _positionY; }
            private set
            {
                if (value != _positionY)
                {
                    _positionY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            private set
            {
                if (value != _status)
                {
                    _status = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _alarm;
        public string Alarm
        {
            get { return _alarm; }
            private set
            {
                if (value != _alarm)
                {
                    _alarm = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            private set
            {
                if (value != _message)
                {
                    _message = value;
                    NotifyPropertyChanged();
                }
            }
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

        private readonly RelayCommand _stopCommand;
        public ICommand StopCommand { get { return _stopCommand; } }

        private bool _programRunning;
        public bool ProgramRunning
        {
            get { return _programRunning; }
            private set
            {
                _programRunning = value;
                NotifyPropertyChanged();
            }
        }

        private string _programStatus;
        public string ProgramStatus
        {
            get { return _programStatus; }
            private set
            {
                _programStatus = value;
                NotifyPropertyChanged();
            }
        }

        private double _programProgress;
        public double ProgramProgress
        {
            get { return _programProgress; }
            private set
            {
                _programProgress = value;
                NotifyPropertyChanged();
            }
        }

        public MachineViewModel()
        {
            updateStatus(AppCore.Machine.State);

            _resetCommand = new RelayCommand(
                parameter => AppCore.Machine.ResetAsync(),
                parameter => AppCore.Machine.CanReset());
            _homeCommand = new RelayCommand(
                parameter => AppCore.Machine.HomeAsync(),
                parameter => AppCore.Machine.CanHome());
            _unlockCommand = new RelayCommand(
                parameter => AppCore.Machine.UnlockAsync(),
                parameter => AppCore.Machine.CanUnlock());
            _resumeCommand = new RelayCommand(parameter => { });
            _holdCommand = new RelayCommand(parameter => { });
            _startCommand = new RelayCommand(
                parameter => AppCore.Machine.RunAsync(AppCore.Generator.GCodeProgram),
                parameter => AppCore.Machine.CanRun() && AppCore.Generator.GCodeProgram != null);
            _stopCommand = new RelayCommand(
                parameter => AppCore.Machine.CancelAsync(),
                parameter => AppCore.Machine.CanCancel());

            AppCore.Machine.StateUpdated += Machine_StatusUpdated;
            AppCore.Generator.Completed += Generator_Completed;
        }

        private void updateStatus(LaserMachine.MachineState state)
        {
            PositionX = toDisplayString(state.X);
            PositionY = toDisplayString(state.Y);
            Status = state.Status;
            Alarm = "None";
            Message = "None";
        }

        private void Machine_StatusUpdated(LaserMachine sender, LaserMachine.MachineState state, bool invalidateCanDo)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                updateStatus(state);

                if (invalidateCanDo)
                {
                    _resetCommand.NotifyCanExecuteChanged();
                    _homeCommand.NotifyCanExecuteChanged();
                    _unlockCommand.NotifyCanExecuteChanged();
                    _resumeCommand.NotifyCanExecuteChanged();
                    _holdCommand.NotifyCanExecuteChanged();
                    _startCommand.NotifyCanExecuteChanged();
                    _stopCommand.NotifyCanExecuteChanged();
                }
            });
        }

        private void Generator_Completed(object sender, EventArgs e)
        {
            if (!ProgramRunning)
            {
                ProgramStatus = string.Empty;
                ProgramProgress = 0.0;
            }

            _startCommand.NotifyCanExecuteChanged();
        }

        private static string toDisplayString(double value)
        {
            return double.IsNaN(value) ? FIELD_PLACEHOLDER : value.ToString("F3", CultureInfo.InvariantCulture);
        }
    }
}
