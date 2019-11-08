using System.Windows;
using System;
using Microsoft.Win32;

namespace Rest_Reminder
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        internal MainWindow Mw
        {
            get;
            set;
        }

        protected Settings()
        {
            InitializeComponent();
            SetStartup(); //I'm lazy so set as startup application at every run 
            if (null == Mw)
            {
                Mw = new MainWindow(this);
                Mw.Activate();
                Mw.Show();
            }
            else
            {
                Mw.Activate();
                Mw.Show();
            }

            this.WindowState = WindowState.Minimized;
            this.Hide();      // Programmatically hides the window

            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Show(); //show the taskbar icon
            this.WindowState = WindowState.Normal; // show the window
            this.Left = desktopWorkingArea.Right / 2 - this.Width / 2;  // put the window in the middle of the screen
            this.Top = desktopWorkingArea.Bottom / 2 - this.Height / 2; // put the window in the middle of the screen
        }

        public Settings(MainWindow mw)
        {
            InitializeComponent();

            Mw = mw;

            if (null == Mw)
            {
                Mw = new MainWindow(this);
                Mw.Activate();
                Mw.Show();
            }
            else
            {
                Mw.Activate();
                Mw.Show();
            }

            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Show(); //show the taskbar icon
            this.WindowState = WindowState.Normal; // show the window
            this.Left = desktopWorkingArea.Right / 2 - this.Width / 2;  // put the window in the middle of the screen
            this.Top = desktopWorkingArea.Bottom / 2 - this.Height / 2; // put the window in the middle of the screen
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.worktime = Convert.ToInt32(worktime.Text);
            Properties.Settings.Default.delaytime = Convert.ToInt32(delaytime.Text);
            Properties.Settings.Default.Save();
            e.Cancel = true;  // cancels the window close    
            this.WindowState = WindowState.Minimized;
            this.Hide();      // Programmatically hides the window
            base.OnClosing(e);
        }


        
        private void SetStartup()
        {
            

               
                string exePath =
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
                Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)
                    ?.SetValue("Rest Reminder", exePath ?? throw new InvalidOperationException());
        }

}
}
