using LaserPewer.Model;
using System;
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

        private RelayCommand _generateCommand;
        public ICommand GenerateCommand { get { return _generateCommand; } }

        public ProgramGeneratorViewModel()
        {
            _generateCommand = new RelayCommand(parameter => AppCore.Generator.Generate());

            AppCore.Generator.SettingModified += ProgramGenerator_SettingModified;
        }

        private void ProgramGenerator_SettingModified(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(VectorPower));
            NotifyPropertyChanged(nameof(VectorSpeed));
        }
    }
}
