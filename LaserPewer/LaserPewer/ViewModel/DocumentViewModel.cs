using LaserPewer.Model;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace LaserPewer.ViewModel
{
    class DocumentViewModel : BaseViewModel
    {
        public string FriendlyName
        {
            get
            {
                if (AppCore.Document.FileName == null) return FIELD_PLACEHOLDER;
                return Path.GetFileName(AppCore.Document.FileName);
            }
        }

        public Drawing Drawing { get { return AppCore.Document.Drawing; } }

        public string Size
        {
            get
            {
                if (AppCore.Document.FileName == null) return FIELD_PLACEHOLDER;
                return AppCore.Document.Size.Width.ToString("F0", CultureInfo.InvariantCulture) + "x" +
                    AppCore.Document.Size.Height.ToString("F0", CultureInfo.InvariantCulture) + "mm";
            }
        }

        private readonly RelayCommand _openCommand;
        public ICommand OpenCommand { get { return _openCommand; } }

        public DocumentViewModel()
        {
            _openCommand = new RelayCommand(_openCommand_Execute);

            AppCore.Document.Modified += Document_Modified;
        }

        private void _openCommand_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Scalable Vector Graphics (*.svg)|*.svg";
            if (dialog.ShowDialog() ?? false)
            {
                AppCore.Document.LoadSVG(dialog.FileName);
                AppCore.Generator.Clear();
            }
        }

        private void Document_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(FriendlyName));
            NotifyPropertyChanged(nameof(Drawing));
            NotifyPropertyChanged(nameof(Size));
        }
    }
}
