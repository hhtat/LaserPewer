﻿using LaserPewer.Model;
using System.Windows;

namespace LaserPewer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppCore.Instance.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            AppCore.Instance.Deinitialize();
        }
    }
}
