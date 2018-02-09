﻿using LaserPewer.Model;
using LaserPewer.Shared;
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

        private readonly RelayCommand _stopCommand;
        public ICommand StopCommand { get { return _stopCommand; } }

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
            setDefaults();

            _resetCommand = new RelayCommand(parameter => AppCore.Machine.ResetAsync());
            _homeCommand = new RelayCommand(parameter => AppCore.Machine.HomeAsync());
            _unlockCommand = new RelayCommand(parameter => AppCore.Machine.UnlockAsync());
            _resumeCommand = new RelayCommand(parameter => { });
            _holdCommand = new RelayCommand(parameter => { });
            _startCommand = new RelayCommand(
                parameter => { },
                parameter => AppCore.Generator.GCodeProgram != null);
            _stopCommand = new RelayCommand(
                parameter => { });

            AppCore.Machine.StatusUpdated += Machine_StatusUpdated;
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

        private void Machine_StatusUpdated(object sender, LaserMachine.MachineStatus status)
        {
            PositionX = toDisplayString(status.X);
            PositionY = toDisplayString(status.Y);
            Status = status.Message;
        }

        private void Generator_Completed(object sender, EventArgs e)
        {
            if (AppCore.Sender.State == GrblSender.SenderState.Idle)
            {
                ProgramStatus = string.Empty;
                ProgramProgress = 0.0;
            }

            _startCommand.NotifyCanExecuteChanged();
        }

        private static string toDisplayString(double value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }
    }
}
