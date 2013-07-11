using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using BarCamp.Resources;
using ZXing;
using System.Device.Location;
using System.Windows.Threading;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows.Media.Imaging;

namespace BarCamp
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            rootPivot.Title = "BarCamp Penang '13";
            generateCode("Windows Phone 8 BarCamp");
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            startCountdown();
            loadEventVenue();
            loadWebsite();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            // when done edit, go back to second page
            string strItemIndex;
            if (NavigationContext.QueryString.TryGetValue("goto", out strItemIndex))
            {
                rootPivot.SelectedIndex = Convert.ToInt32(strItemIndex);
                base.OnNavigatedTo(e);
            }
            // then update the qr code
            string msg = "";
            if (NavigationContext.QueryString.TryGetValue("msg", out msg))
            {
                generateCode(msg);
            }
        }

        private void btn_EditProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditProfilePage.xaml", UriKind.Relative));
        }

        private void rect_MapHome_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/FullScreenMap.xaml", UriKind.RelativeOrAbsolute));
        }
        #region QRCode
        public void generateCode(string msg)
        {
            BarcodeWriter BW = new BarcodeWriter();
            BW.Format = BarcodeFormat.QR_CODE;
            var bitmap = BW.Write(msg);
            //image_QRCode.Source = bitmap;
        }
        #endregion QRCode

        // day and time for event
        private int STARTDAY = 27;
        private int STARTTIME = 9;
        #region Countdown
        // clean up unused code and name the var properly here and xaml
        private string DAY = "";
        private string HOUR = "";
        private string MINUTE = "";
        //runs on the UI Thread
        private DispatcherTimer dispatcherTimer;
        private DateTime EndTime { get; set; }
        private void startCountdown()
        {
            if (this.dispatcherTimer == null)
            {
                this.dispatcherTimer = new DispatcherTimer();
                this.dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
                this.dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            }
            // countdown time YearMonthDate HourMinuteSecond
            this.EndTime = new DateTime(2013, 7, STARTDAY, STARTTIME, 0, 0);
            this.dispatcherTimer.Start();
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var remaining = DateTime.Now - this.EndTime;
            int remainingSeconds = (int)remaining.TotalSeconds;
            calculator((int)(remainingSeconds * -1));
            this.txtblk_Hour.Text = DAY;
            this.txtblk_Minute.Text = HOUR;
            if (remaining.TotalSeconds >= 0)
            {
                this.dispatcherTimer.Stop();
                //MessageBox.Show("Triggers alarm");
            }
        }
        public void calculator(float d)
        {
            //float d; 
            d = d / 60 / 60 / 24;

            int day, hour, minute, second;

            day = (int)d;
            d = (d - day) * 24;
            hour = (int)d;
            d = (d - hour) * 60;
            minute = (int)d;
            d = (d - minute) * 60;
            second = (int)d;

            //string retStr = day + "D" + hour + "H" + minute + "M" + second + "S";

            //hour = hour + (day * 24);
            if (day == 0 && hour == 0 && minute == 0)
            {
                DAY = "00";
                HOUR = "00";
                MINUTE = "00";
                return;
            }
            if (day == 0)
            {
                txtblk_HourLabel.Text = "Hours";
                txtblk_MinuteLabel.Text = "Minute";
                DAY = hour.ToString();
                HOUR = minute.ToString();
                return;
            }

            DAY = day.ToString();
            HOUR = hour.ToString();
            MINUTE = minute.ToString();
            //string retStr = hour + "H" + minute + "M";
            //return retStr;
        }

        #endregion

        // lat long
        // infotrek 5.333765,100.306693
        // QB 5.333877,100.306751
        private double VENUE_LAT = 5.333877;
        private double VENUE_LONG = 100.306751;
        private string EVENTVENUEPINTEXT = "We are here\n@Queensbay Mall";
        #region Map

        private double ALIGNPUSHPIN_LAT = -0.00158;//-0.00227; // for 0 pitch
        private double ALIGNPUSHPIN_LONG = -0.00267;//0.00034;
        private double PITCH = 35;
        private double HEADING = 245;
        public void loadEventVenue()
        {
            GeoCoordinate geoCoor = new GeoCoordinate(VENUE_LAT, VENUE_LONG);

            map_Home.Center = geoCoor;
            map_Home.ZoomLevel = 16;
            map_Home.PedestrianFeaturesEnabled = true;
            map_Home.CartographicMode = MapCartographicMode.Terrain;
            map_Home.LandmarksEnabled = true;
            map_Home.Pitch = PITCH;
            map_Home.Heading = HEADING;

            pinEventVenue();

        }
        public void pinEventVenue()
        {
            MapLayer mapLayer_EventVenue = new MapLayer();
            MapOverlay mapOverlay_EventVenue = new MapOverlay();
            Pushpin pushPin_EventVenue = new Pushpin();
            GeoCoordinate geoCoor_EventVenue = new GeoCoordinate(VENUE_LAT, VENUE_LONG);
            pushPin_EventVenue.Content = EVENTVENUEPINTEXT;
            mapOverlay_EventVenue.Content = pushPin_EventVenue;
            mapOverlay_EventVenue.GeoCoordinate = new GeoCoordinate(VENUE_LAT + ALIGNPUSHPIN_LAT, VENUE_LONG + ALIGNPUSHPIN_LONG);
            mapLayer_EventVenue.Add(mapOverlay_EventVenue);
            map_Home.Layers.Add(mapLayer_EventVenue);
            // not necessary
            //pushPin_EventVenue.Tag = "eventVenue";
            //pushPin_EventVenue.Name = "eventVenue";
        }
        #endregion

        //wb_Home_Sponsor
        string sponsorURL = "http://barcamppenang.org/sponsors.html";
        string agendaURL = "http://barcamppenang.org/agenda.html";
        private void loadWebsite()
        {

            bool isInternetAvailiable = NetworkInterface.GetIsNetworkAvailable();
            if (!isInternetAvailiable) {
                MessageBox.Show("Connect to internet to see the content");
                wb_Home_Sponsor.Visibility = System.Windows.Visibility.Collapsed;
                //wb_Agenda.Visibility = System.Windows.Visibility.Collapsed;
                pivot_Agenda.Visibility = System.Windows.Visibility.Collapsed;
                BitmapImage myBitmapImage = new BitmapImage(new Uri("/Images/Sponsor.jpg", UriKind.Relative));
                img_Sponsor.Source = myBitmapImage;
                return;
            }
            wb_Home_Sponsor.Loaded += (object sender, RoutedEventArgs e) =>
            {
                wb_Home_Sponsor.Navigate(new Uri(sponsorURL, UriKind.Absolute));
            };
            wb_Agenda.Loaded += (object sender, RoutedEventArgs e) =>
            {
                wb_Agenda.Navigate(new Uri(agendaURL, UriKind.Absolute));
            };
        }
    }
}