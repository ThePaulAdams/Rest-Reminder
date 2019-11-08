using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Input;

// <summary>
// This software was created for personal use.
// If you wish to use this for commercial user please contact Paul Adams: admin@thepauladams.com
// </summary>

namespace Rest_Reminder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow
    {
        [DllImport("user32.dll")]
        private static extern void LockWorkStation(); // Allows us to lock the workstation

        private readonly NotifyIcon _ni;

        private readonly DispatcherTimer _timer; // count down timer
        internal TimeSpan Time; // Time value
        internal int Delayed; // if delayed 
        internal int Onbreak; // if on break
        internal int Breaks; // track breaks
        internal int Delays; // track delays
        readonly Settings _settings;
        internal double LeftPos;
        internal double TopPos;

        /// <inheritdoc />
        public MainWindow(Settings settings)
        {
            InitializeComponent();
            _settings = settings;
            var desktopWorkingArea = SystemParameters.WorkArea;
            LeftPos = desktopWorkingArea.Right / 2 - Width / 2; // put the window in the middle of the screen
            TopPos = desktopWorkingArea.Right / 2 - Width / 2; // put the window in the middle of the screen
            var cm = new ContextMenu();
            if (_ni == null) _ni = new NotifyIcon();
            MenuItem mibreak = new MenuItem();
            MenuItem misetting = new MenuItem();
            MenuItem midonate = new MenuItem();

            // Create the System Tray Icon
            // Initialize contextMenu
            cm.MenuItems.AddRange(new[] {mibreak, misetting, midonate});

            // Initialize menuItem1
            mibreak.Index = 0;
            mibreak.Text = Properties.Resources.MainWindow_MainWindow_Take_a_Break;
            mibreak.Click += Mibreak_Click;

            // Initialize menuItem2
            misetting.Index = 1;
            misetting.Text = Properties.Resources.MainWindow_MainWindow_Settings;
            misetting.Click += Misetting_Click;

            //Initialise menuItem3
            midonate.Index = 2;
            midonate.Text = Properties.Resources.MainWindow_MainWindow_Donate;
            midonate.Click += Midonate_Click;

            _ni.Icon = new System.Drawing.Icon(AppDomain.CurrentDomain.BaseDirectory + "coffee.ico");
            _ni.Visible = true;
            _ni.DoubleClick += delegate
            {
                //on double click show the window
                Show();
                WindowState = WindowState.Normal;
            };
            _ni.ContextMenu = cm;

            //Create even switched to detect when user locks/unlocks workstation
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            //remove the window close/minimize/maximize buttons
            WindowStyle = WindowStyle.None;

            //let us know when the window is loaded so we can minimize it and hide it
            Loaded += Window_Loaded;

            //set image to smile :)
            Image.Source = new BitmapImage(new Uri("smile.png", UriKind.Relative));

            //First Time Run
            //set the 55 minute timer value
            Time = TimeSpan.FromMinutes(Properties.Settings.Default.worktime);

            //start the countdown
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1),
                                         DispatcherPriority.Normal,
                                         delegate
                                         {
                                             TbTime.Text =
                                                 Time.ToString(); //set the time value string so the user can check how long until they are due a break.               
                                             if (Time == TimeSpan.Zero) _timer.Stop(); //when we hit 0 stop the clock
                                             Time = Time.Add(TimeSpan.FromSeconds(-1)); // otherwise countdown 1 second
                                             _ni.Text = Time.ToString(); // update the system tray icon hover text
                                             if (!(Time.TotalMinutes <= 0)) return;
                                             TbTime.Text = randomButtonText(); //change the timer to a GO REST message
                                             Image.Source = Delayed == 0
                                                 ? new BitmapImage(new Uri("tired.png", UriKind.Relative))
                                                 : new BitmapImage(new Uri("angry.png", UriKind.Relative));
                                             Show(); //show the task bar icon
                                             WindowState = WindowState.Normal; // show the window
                                             Left = desktopWorkingArea.Right / 2 -
                                                         Width / 2; // put the window in the middle of the screen
                                             Top = desktopWorkingArea.Bottom / 2 -
                                                        Height / 2; // put the window in the middle of the screen
                                         },
                                         System.Windows.Application.Current.Dispatcher ?? throw new InvalidOperationException());
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized) Hide(); // hide the icon when we minimise the window
            base.OnStateChanged(e);
        }

        //public void StartTimer()
        //{
        //    _timer.Start();
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //hide the window and icon after loading, the user doesn't need to see it
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
            WindowState = WindowState.Minimized;
        }

        private string randomButtonText() // generate a random go rest message
        {
            Random rnd = new Random();
            switch (rnd.Next(6))
            {
                case 0: return "You need to take a break";
                case 1: return "Rest a while";
                case 3: return "Stretch your legs";
                case 5: return "Rest for a moment.";
                default: return "You need to take a break.";
            }
        }

        private void Mibreak_Click(object sender, EventArgs e)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (e == null) throw new ArgumentNullException(nameof(e));
            _breakB_Click(sender, e as RoutedEventArgs);
        }


        private void Midonate_Click(object sender, EventArgs e)
        {
            string url = "";

            string business = "softwaredonations@gmail.com"; 
            string description = "Donation for Rest Reminder";   
            string country = "UK";                 
            string currency = "GBP";              

            url += "https://www.paypal.com/cgi-bin/webscr" +
                   "?cmd=" + "_donations" +
                   "&business=" + business +
                   "&lc=" + country +
                   "&item_name=" + description +
                   "&currency_code=" + currency +
                   "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }

        private void Misetting_Click(object sender, EventArgs e)
        {
            if (_settings == null) return;
            _settings.Activate();
            _settings.Show();
            _settings.WindowState = WindowState.Normal; // show the window
            _settings.Left = LeftPos; // put the window in the middle of the screen
            _settings.Top = TopPos; // put the window in the middle of the screen
        }

        private void _breakB_Click(object sender, RoutedEventArgs e) // user has told us they are taking a break
        {
            Time = TimeSpan.FromMinutes(9999); //set timer to infinite
            Delayed = 0; //set that they have not delayed the timer
            Onbreak = 1; //set that they have taken a break
            Breaks++;
            LockWorkStation(); //lock the workstation
        }

        private void DelayB_Click(object sender, RoutedEventArgs e) //user has told us they wish to delay their break
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            TimeSpan timeCheck = TimeSpan.FromMinutes(Time.TotalMinutes);
            if (timeCheck <= TimeSpan.FromMinutes(Properties.Settings.Default.delaytime)
            ) //if there is less than 10 minutes on the timer
            {
                Time = TimeSpan.FromMinutes(Properties.Settings.Default.delaytime); //give them 10 minutes until the next warning
                Delayed = 1; //set that they have delayed their break
                _timer.Start(); // start the timer again
                Image.Source =
                    new BitmapImage(new Uri("angry.png",
                                            UriKind.Relative)); //make sure they know you mean business with an angry face      
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
                WindowState = WindowState.Minimized; //hide the window
            }
            else if (timeCheck > TimeSpan.FromMinutes(Properties.Settings.Default.delaytime)
            ) //if they have more than 10 minutes left before a scheduled break
            {
                Image.Source =
                    new BitmapImage(new Uri("tired.png",
                                            UriKind
                                                .Relative)); // set their timer icon to a tired face and continue business as usual, they don't have a need to delay it yet.
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
                WindowState = WindowState.Minimized; //hide the window
            }

            Delays++;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    //I left my desk, we don't really care about this let the timer continue, we will do a check on unlock
                    break;
                // if they took a break and have come back
                case SessionSwitchReason.SessionUnlock when Onbreak == 1:
                {
                    Time = TimeSpan.FromMinutes(Properties.Settings.Default
                                                    .worktime); // reset the timer to 55 minutes for their next break
                    _timer.Start(); // start the timer again
                    //start the timer
                    Image.Source = new BitmapImage(new Uri("smile.png", UriKind.Relative)); //be happy again
                    var desktopWorkingArea = SystemParameters.WorkArea;
                    Left = desktopWorkingArea.Right - Width;
                    Top = desktopWorkingArea.Bottom - Height;
                    WindowState = WindowState.Minimized; //hide the window
                    break;
                }
                // if they did not delay it but there timer has run out, we can assume they walked away without telling us they took a break.
                case SessionSwitchReason.SessionUnlock when Time.TotalMinutes <= 0:
                {
                    Time = TimeSpan.FromMinutes(Properties.Settings.Default
                                                    .worktime); // reset the timer to 55 minutes for their next break
                    _timer.Start(); // start the timer again
                    //start the timer
                    Image.Source = new BitmapImage(new Uri("smile.png", UriKind.Relative)); //be happy again
                    var desktopWorkingArea = SystemParameters.WorkArea;
                    Left = desktopWorkingArea.Right - Width;
                    Top = desktopWorkingArea.Bottom - Height;
                    WindowState = WindowState.Minimized; //hide the window
                    break;
                }
                case SessionSwitchReason.SessionUnlock:
                {
                    if (!(Time.TotalMinutes >= 10)
                    ) { } //we can assume it locked while they had a conversation or worked on another machine.  do nothing and let the timer continue

                    break;
                }
            }
        }

        private void
            button_Click(
                object sender,
                RoutedEventArgs e) //replace the minimise button with this false minimise button, because we wanted to hide the close button.
        {
            //if they are due a break but just minimise this, act as if they have asked for a delay
            if (Time.TotalMinutes <= 0)
            {
                var desktopWorkingArea = SystemParameters.WorkArea;
                var timeCheck = TimeSpan.FromMinutes(Time.TotalMinutes);
                if (timeCheck <= TimeSpan.FromMinutes(Properties.Settings.Default.delaytime)
                ) //if there is less than delay time minutes on the timer
                {
                    Time = TimeSpan.FromMinutes(Properties.Settings.Default
                                                    .delaytime); //give them 10 minutes until the next warning
                    Delayed = 1; //set that they have delayed their break
                    _timer.Start(); // start the timer again
                    Image.Source =
                        new BitmapImage(new Uri("angry.png",
                                                UriKind
                                                    .Relative)); //make sure they know you mean business with an angry face      
                }
                else if (timeCheck > TimeSpan.FromMinutes(Properties.Settings.Default.delaytime)
                ) //if they have more than 10 minutes left before a scheduled break
                {
                    Image.Source =
                        new BitmapImage(new Uri("tired.png",
                                                UriKind
                                                    .Relative)); // set their timer icon to a tired face and continue business as usual, they don't have a need to delay it yet.
                }

                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
                WindowState = WindowState.Minimized; //hide the window
            }
            else
            {
                //otherwise just minimize the window
                var desktopWorkingArea = SystemParameters.WorkArea;
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
                WindowState = WindowState.Minimized;
            }
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            //prevent them from closing the window by acknowledging the action but throwing away the handler.
            e.Cancel = true;
        }

        private void Grid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
