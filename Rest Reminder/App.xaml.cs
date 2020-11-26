using Rest_Reminder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace GD_HS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       
      
        private Settings _settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _settings = new Settings(null);
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;





            //_settings.Activate();
            //_settings.Show();
            //_settings.WindowState = WindowState.Normal; // show the window
            //_settings.Left = desktopWorkingArea.Right / 2; // put the window in the middle of the screen
            //_settings.Top = desktopWorkingArea.Bottom / 2; // put the window in the middle of the screen

            //_mw = _settings.Mw;
            //_mw.Activate();
            //_mw.Show();

        }
    }
}
