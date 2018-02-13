using LaserPewer.Model;
using System;
using System.Windows;
using System.Windows.Input;

namespace LaserPewer.ViewModel
{
    public class ProgramGeneratorViewModel : BaseViewModel
    {
        public double VectorPower
        {
            get { return AppCore.Generator.VectorPower; }
            set
            {
                if (AppCore.Generator.VectorPower != value &&
                    value >= 0.0 && value <= 1.0)
                {
                    AppCore.Generator.VectorPower = value;
                }
                NotifyPropertyChanged();
            }
        }

        public double VectorSpeed
        {
            get { return AppCore.Generator.VectorSpeed; }
            set
            {
                if (AppCore.Generator.VectorSpeed != value &&
                    value >= 0.0 && value <= 1.0)
                {
                    AppCore.Generator.VectorSpeed = value;
                }
                NotifyPropertyChanged();
            }
        }

        public string GCode { get { return AppCore.Generator.GCodeProgram; } }

        private RelayCommand _generateCommand;
        public ICommand GenerateCommand { get { return _generateCommand; } }

        public ProgramGeneratorViewModel()
        {
            _generateCommand = new RelayCommand(_generateCommand_Execute, parameter => AppCore.Document.Drawing != null);

            AppCore.Generator.SettingModified += ProgramGenerator_SettingModified;
            AppCore.Generator.Generated += Generator_Completed;

            AppCore.Document.Modified += Document_Modified;
        }

        private void _generateCommand_Execute(object parameter)
        {
            if (AppCore.Generator.TryStart(AppCore.Document.Drawing, AppCore.Document.Offset, AppCore.MachineProfiles.Active))
            {
                ViewService.ShowGenerationDialog();
                AppCore.Generator.Stop();
            }
        }

        private void ProgramGenerator_SettingModified(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(VectorPower));
            NotifyPropertyChanged(nameof(VectorSpeed));
        }

        private void Generator_Completed(object sender, EventArgs e)
        {
            ViewService.InvokeAsync(() =>
            {
                NotifyPropertyChanged(nameof(GCode));
            });
        }

        private void Document_Modified(object sender, EventArgs e)
        {
            _generateCommand.NotifyCanExecuteChanged();
        }
    }
}
