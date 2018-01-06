using LaserPewer.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LaserPewer.ViewModel
{
    class DocumentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FriendlyName
        {
            get
            {
                if (AppCore.Document.FileName == null) return string.Empty;
                return Path.GetFileName(AppCore.Document.FileName);
            }
        }

        public Drawing Drawing { get { return AppCore.Document.Drawing; } }

        public Size Size { get { return AppCore.Document.Size; } }

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
            }
        }

        private void Document_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged("FriendlyName");
            NotifyPropertyChanged("Drawing");
            NotifyPropertyChanged("Size");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
