using System.Windows;
using System;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace Rest_Reminder
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        internal MainWindow Mw { get; set; }
        internal string name { get; set; }
        internal string email { get; set; }

        const string userRoot = "HKEY_CURRENT_USER";
        const string subkey = "RestReminder";
        const string keyName = userRoot + "\\" + subkey;

        protected Settings()
        {
            InitializeComponent();

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
            this.Hide(); // Programmatically hides the window

            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Show(); //show the taskbar icon
            this.WindowState = WindowState.Normal; // show the window
            this.Left = desktopWorkingArea.Right / 2 - this.Width / 2; // put the window in the middle of the screen
            this.Top = desktopWorkingArea.Bottom / 2 - this.Height / 2; // put the window in the middle of the screen
        }

        public Settings(MainWindow mw)
        {
            InitializeComponent();
            Mw = mw;
            

           
            if (null == Mw)
            {
                SetStartup(); //I'm lazy so set as startup application at every run 
                this.ButtonSave.IsEnabled = false;               

                string Activation = (string)Registry.GetValue(keyName, "Activation", null);
                if (string.IsNullOrEmpty(Activation))
                { 

                    var desktopWorkingArea = SystemParameters.WorkArea;
                    this.Show(); //show the taskbar icon
                    this.WindowState = WindowState.Normal; // show the window
                    this.Left = desktopWorkingArea.Right / 2 - this.Width / 2; // put the window in the middle of the screen
                    this.Top = desktopWorkingArea.Bottom / 2 - this.Height / 2; // put the window in the middle of the screen 
                }
                else
                {
                    this.ButtonSave.IsEnabled = true;
                    Mw = new MainWindow(this);
                    this.WindowState = WindowState.Minimized;
                    this.Hide(); // Programmatically hides the window

                    Mw.Activate();
                    Mw.Show();
                }
            }


            
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.worktime = Convert.ToInt32(worktime.Text);
            Properties.Settings.Default.delaytime = Convert.ToInt32(delaytime.Text);
            Properties.Settings.Default.Save();
            e.Cancel = true; // cancels the window close    
            this.WindowState = WindowState.Minimized;
            this.Hide(); // Programmatically hides the window
            base.OnClosing(e);
        }



        private void SetStartup()
        {
            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)
                ?.SetValue("Rest Reminder", exePath ?? throw new InvalidOperationException());
        }

        private void buttonSave_Click(object sender,RoutedEventArgs e) //replace the minimise button with this false minimise button, because we wanted to hide the close button.
        {
            this.WindowState = WindowState.Minimized;
            this.Hide(); // Programmatically hides the window
        }


        private void buttonActivate_Click (object sender, RoutedEventArgs e) 
        {
            string code = "";
            if(string.IsNullOrEmpty(this.ActivationKey.Text))
            {


            }
            else
            {

                string key = this.ActivationKey.Text;
                string cs = ConfigurationManager.ConnectionStrings["mySQLconnection"].ConnectionString;
                    //adams.paul.t@gmail.com041299  = 8d0c84c6403dc2603f91b468cb0213ca

                using var con = new MySqlConnection(cs);
                con.Open();

                var stm = "SELECT * FROM Licenses WHERE `ActivationCode` = '" + key + "'";
                var cmd = new MySqlCommand(stm, con);

                using MySqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                   name = rdr["name"].ToString();
                   email = rdr["email"].ToString();
                   code = rdr["ActivationCode"].ToString();
                }
                 

                                 
                if (string.IsNullOrEmpty(code)) {
                    //fml
                }
                else
                {
                    Registry.SetValue(keyName, "Activation", code, RegistryValueKind.String);

                    this.ButtonSave.IsEnabled = true;
                    Mw = new MainWindow(this);
                    this.WindowState = WindowState.Minimized;
                    this.Hide(); // Programmatically hides the window

                    Mw.Activate();
                    Mw.Show();
                }

            }


        }
    }
}
