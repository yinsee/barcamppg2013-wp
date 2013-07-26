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
using System.IO.IsolatedStorage;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace BarCamp
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public MainPage()
        {
            InitializeComponent();
            generateCode("Windows Phone 8 BarCamp");
            loadFriend();
            startCountdown();
            loadEventVenue();
            loadWebsite();

        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Terminate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // database Query
            var friendListInDB = from FriendListItem friendlist in friendListDB.FriendListItems
                                 select friendlist;
            FriendListItems = new ObservableCollection<FriendListItem>(friendListInDB);

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

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            friendListDB.SubmitChanges();
        }

        //private void btn_EditProfile_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/Pages/EditProfilePage.xaml?decode=" + decodeCode(), UriKind.Relative));
        //}
        private void img_addEditBtn_Tap(Object sender, RoutedEventArgs e) 
        {
            NavigationService.Navigate(new Uri("/Pages/EditProfilePage.xaml?decode=" + decodeCode(), UriKind.Relative));
        }
        private void rect_MapHome_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/FullScreenMap.xaml", UriKind.RelativeOrAbsolute));
        }
        #region QRCodeWriter
        WriteableBitmap bitmap;
        private void generateCode(string msg)
        {
            BarcodeWriter BW = new BarcodeWriter();
            BW.Format = BarcodeFormat.QR_CODE;
            bitmap = BW.Write(msg);
            image_QRCode.Source = bitmap;
        }
        private string decodeCode()
        {
            BarcodeReader BR = new BarcodeReader();
            Result result = BR.Decode(bitmap);
            if (result == null) { return "error"; }
            return "" + result.ToString();
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
        private void calculator(float d)
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
        private void loadEventVenue()
        {
            GeoCoordinate geoCoor = new GeoCoordinate(VENUE_LAT, VENUE_LONG);

            map_Home.Center = geoCoor;
            map_Home.ZoomLevel = 16;
            map_Home.PedestrianFeaturesEnabled = true;
            map_Home.CartographicMode = MapCartographicMode.Hybrid;
            map_Home.LandmarksEnabled = true;
            map_Home.Pitch = PITCH;
            map_Home.Heading = HEADING;

            pinEventVenue();

        }
        private void pinEventVenue()
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
        #region WebBrowser
        private void loadWebsite()
        {
            // show from cache
            loadFromCache(wb_Home_Sponsor, "sponsor");
            loadFromCache(wb_Agenda, "agenda");
            // refresh cache
            refreshCache(sponsorURL, "sponsor");
            refreshCache(agendaURL, "agenda");
        }

        private void loadFromCache(WebBrowser sender, String cachename)
        {
            // save to file
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.FileExists(cachename + ".html"))
            {
                sender.Navigate(new Uri(cachename + ".html", UriKind.Relative));
            }
            else
            {
                sender.NavigateToString("<html><body>Please connect to Internet.</body></html>");
            }
        }

        private void refreshCache(String url, String cachename)
        {
            var downloader = new WebClient();
            if (cachename == "sponsor")
                downloader.DownloadStringCompleted += cacheSponsor_DownloadStringCompleted;
            else
                downloader.DownloadStringCompleted += cacheAgenda_DownloadStringCompleted;
            downloader.DownloadStringAsync(new Uri(url, UriKind.Absolute));
        }

        private void cacheAgenda_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled) return;

            // save to file
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            StreamWriter file = new StreamWriter(new IsolatedStorageFileStream("agenda.html", System.IO.FileMode.Create, store));
            file.Write(e.Result);
            file.Close();

            // update webbrowser 
            //http://mikaelkoskinen.net/webbrowser-navigatetostring-broken-in-windows-phone-8-and-also-affects-windows-phone-7-apps-solution-included
            wb_Agenda.Navigate(new Uri("agenda.html", UriKind.Relative));
        }

        private void cacheSponsor_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled) return;

            // save to file
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            StreamWriter file = new StreamWriter(new IsolatedStorageFileStream("sponsor.html", System.IO.FileMode.Create, store));
            file.Write(e.Result);
            file.Close();

            // update webbrowser 
            //http://mikaelkoskinen.net/webbrowser-navigatetostring-broken-in-windows-phone-8-and-also-affects-windows-phone-7-apps-solution-included
            wb_Home_Sponsor.Navigate(new Uri("sponsor.html", UriKind.Relative));
        }
        #endregion

        #region FriendList
        private FriendListDataContext friendListDB;
        private ObservableCollection<FriendListItem> _friendListItems;
        public ObservableCollection<FriendListItem> FriendListItems
        {
            get
            {
                return _friendListItems;
            }
            set
            {
                if (_friendListItems != value)
                {
                    _friendListItems = value;
                    NotifyPropertyChanged("FriendListItems");
                }
            }
        }
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        string[] separators = new string[] { "||" };
        private void loadFriend()
        {
            // Connect to the database and instantiate data context.
            friendListDB = new FriendListDataContext(FriendListDataContext.DBConnectionString);
            // Data context and observable collection are children of the main page.
            this.DataContext = this;
        }
        private void addFriendToDb(string name, string phone, string email, string prof, string fbid)
        {
            FriendListItems.Add(new FriendListItem
            {
                FriendName = name,
                FriendPhone = phone,
                FriendEmail = email,
                FriendPro = prof,
                FriendFbId = fbid
            });
            friendListDB.FriendListItems.InsertOnSubmit(new FriendListItem
            {
                FriendName = name,
                FriendPhone = phone,
                FriendEmail = email,
                FriendPro = prof,
                FriendFbId = fbid
            });
            friendListDB.SubmitChanges();
        }
        private void processAndAdd(string msg)
        {
            string[] box = msg.Split(separators, StringSplitOptions.None);
            foreach (FriendListItem f in FriendListItems)
            {
                if (f.FriendPhone.Equals(box[1]) && f.FriendEmail.Equals(box[2]))
                {
                    MessageBox.Show("Existing friend.");
                    return;
                }
            }
            // to valid qrcode
            if (box.Count() == 5)
            {
                addFriendToDb(box[0], box[1], box[2], box[3], box[4]);
                MessageBox.Show("Friend added.");
            }
            else
            {
                MessageBox.Show("Accept only BarCamp QRcode.");
            }
        }
        private void lls_FriendList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // parse sender to list box
            LongListSelector listBox = sender as LongListSelector;

            // when list not empty and it has selected something
            if (listBox != null && listBox.SelectedItem != null)
            {
                // Get the FriendListItem that was tapped.
                FriendListItem sItem = (FriendListItem)listBox.SelectedItem;

                (Application.Current as App).app_friendListItem = sItem;

                NavigationService.Navigate(new Uri("/Pages/DetailPage.xaml?sitem=" + sItem.FriendPhone, UriKind.Relative));
                //MessageBox.Show("" + sItem.FriendListItemId + "hi" + sItem.FriendEmail);

                // Reset selected item to null
                listBox.SelectedItem = null;
            }
        }
        //private void btn_Add_Click(object sender, RoutedEventArgs e)
        //{
        //    //processAndAdd("||||||||");
        //    NavigationService.Navigate(new Uri("/Pages/ScanQRPage.xaml",UriKind.Relative));
        //    processAndAdd(App.StringGetFromScanner);
        //}
        private void img_scanBtn_Tap(object sender, RoutedEventArgs e)
        {
            //processAndAdd("||||||||");
            NavigationService.Navigate(new Uri("/Pages/ScanQRPage.xaml", UriKind.Relative));
            processAndAdd(App.StringGetFromScanner);
        }
        #endregion
    }
}