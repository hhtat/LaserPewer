﻿using LaserPewer.Model;
using LaserPewer.Utilities;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
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

        private readonly RelayCommand _moveCommand;
        public ICommand MoveCommand { get { return _moveCommand; } }

        public DocumentViewModel()
        {
            _openCommand = new RelayCommand(_openCommand_Execute);
            _moveCommand = new RelayCommand(
                _moveCommand_Execute,
                parameter => AppCore.Document.Drawing != null);

            AppCore.Document.Modified += Document_Modified;
        }

        private void _openCommand_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Scalable Vector Graphics (*.svg)|*.svg";
            if (dialog.ShowDialog() ?? false)
            {
                AppCore.Document.LoadSVG(dialog.FileName);
                Point tableTopLeft = CornerMath.AtCorner(Corner.TopLeft, AppCore.MachineProfiles.Active.TableSize, AppCore.MachineProfiles.Active.Origin);
                AppCore.Document.Offset = new Vector(tableTopLeft.X, tableTopLeft.Y);
            }
        }

        private void _moveCommand_Execute(object parameter)
        {
        }

        private void Document_Modified(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(FriendlyName));
            NotifyPropertyChanged(nameof(Drawing));
            NotifyPropertyChanged(nameof(Size));
            _moveCommand.NotifyCanExecuteChanged();
        }
    }
}
