using System;
using System.Windows;

namespace LaserPewer
{
    public static class ViewService
    {
        public static void InvokeAsync(Action callback)
        {
            Application current = Application.Current;
            if (current != null)
            {
                current.Dispatcher.InvokeAsync(callback);
            }
        }

        public static void ShowGenerationDialog()
        {
            ShowDialog(new GenerationDialog());
        }

        private static void ShowDialog(Window dialog)
        {
            dialog.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.8;
            dialog.ShowDialog();
            Application.Current.MainWindow.Opacity = 1.0;
        }
    }
}
