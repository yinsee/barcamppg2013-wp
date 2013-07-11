using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Windows.Devices.Geolocation;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Phone.Maps.Services;

namespace BarCamp
{
    public partial class FullScreenMap : PhoneApplicationPage
    {
        public FullScreenMap()
        {
            InitializeComponent();
            loadEventVenue();
            GPSFunction();

            //ShowMyLocationOnTheMap();

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            setToDefault();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            setToDefault();
        }
        private void appbarbtn_indoorMap_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/IndoorMap.xaml", UriKind.RelativeOrAbsolute));
        }
        // when nav to other page, set back those crap
        private void setToDefault()
        {
            EVENTVENUEPINTEXT = "";
            isMessageShown = false;
        }

        const double VENUE_LAT = 5.333877;
        const double VENUE_LONG = 100.306751;
        private string EVENTVENUEPINTEXT = "InfoTrek (Penang Office)\n1F-78(B), QB Mall\nBeside QB Mall Management Office";
        
        // lat long
        // infotrek 5.333765,100.306693
        // QB 5.333877,100.306751
        //private double ALIGNPUSHPIN_LAT = -0.00227;
        //private double ALIGNPUSHPIN_LONG = 0.00034;
        private GeoCoordinateWatcher watcher;
        private Pushpin pushPin_EventVenue = new Pushpin();
        private bool isMessageShown = true;
        private GeoCoordinate liveCoor = new GeoCoordinate();

        private void loadEventVenue()
        {
            //Geolocator MyGeolocator = new Geolocator();
            //MyGeolocator.DesiredAccuracyInMeters = 5;

            //Geoposition myGeoPosition = new Geoposition("11","");
            // QB  lat long 5.333765,100.306693
            //try{
            //    myGeoPosition = //await MyGeolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
            //}
            //catch (UnauthorizedAccessException){
            //    MessageBox.Show("Location is disabled in phone settings");
            //}

            GeoCoordinate geoCoor = new GeoCoordinate(VENUE_LAT, VENUE_LONG);

            //map_Fullscreen.Center = new GeoCoordinate(VENUE_LAT + ALIGNPUSHPIN_LAT, VENUE_LONG + ALIGNPUSHPIN_LONG);
            map_Fullscreen.Center = geoCoor;
            map_Fullscreen.ZoomLevel = 16;
            map_Fullscreen.PedestrianFeaturesEnabled = true;
            map_Fullscreen.CartographicMode = MapCartographicMode.Terrain;
            map_Fullscreen.LandmarksEnabled = true;

            pinEventVenue();

        }
        #region GPS
        public void GPSFunction()
        {
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                watcher.MovementThreshold = 20;
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
                watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            }
            watcher.Start();
        }
        // check availability of gps on device
        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    MessageBox.Show("Location Service is not enabled on the device");
                    break;

                case GeoPositionStatus.NoData:
                    MessageBox.Show(" The Location Service is working, but it cannot get location data.");
                    break;

            }
        }
        // for device movement
        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position.Location.IsUnknown)
            {
                MessageBox.Show("Please wait while your prosition is determined....");
                return;
            }
            liveCoor = new GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude);
            this.map_Fullscreen.Center = liveCoor;
            pinGPS("pushPin_GPS",  liveCoor);
            plotRoute();
        }
        #endregion

        #region Route
        List<GeoCoordinate> MyCoordinates = new List<GeoCoordinate>();
        RouteQuery MyQuery = null;
        GeocodeQuery Mygeocodequery = null;
        public void plotRoute() {
            // error on position change, redraw
            //GetCoordinates();
        }
        private void GetCoordinates()//private async void GetCoordinates()
        {
            // Get the phone's current location.
            //Geolocator MyGeolocator = new Geolocator();
            //MyGeolocator.DesiredAccuracyInMeters = 5;
            //Geoposition MyGeoPosition = null;
            try
            {
                //MyGeoPosition = await MyGeolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
                MyCoordinates.Add(liveCoor);//new GeoCoordinate(MyGeoPosition.Coordinate.Latitude, MyGeoPosition.Coordinate.Longitude));

                Mygeocodequery = new GeocodeQuery();
                Mygeocodequery.SearchTerm = VENUE_LAT + ", " + VENUE_LONG;
                Mygeocodequery.GeoCoordinate = liveCoor;//new GeoCoordinate(MyGeoPosition.Coordinate.Latitude, MyGeoPosition.Coordinate.Longitude);

                Mygeocodequery.QueryCompleted += Mygeocodequery_QueryCompleted;
                Mygeocodequery.QueryAsync();

            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Location is disabled in phone settings or capabilities are not checked.");
            }
            catch (Exception ex)
            {
                // Something else happened while acquiring the location.
                MessageBox.Show(ex.Message);
            }
        }
        void Mygeocodequery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                MyQuery = new RouteQuery();
                MyCoordinates.Add(e.Result[0].GeoCoordinate);
                MyQuery.Waypoints = MyCoordinates;
                MyQuery.QueryCompleted += MyQuery_QueryCompleted;
                MyQuery.QueryAsync();
                Mygeocodequery.Dispose();
            }
        }
        void MyQuery_QueryCompleted(object sender, QueryCompletedEventArgs<Route> e)
        {
            if (e.Error == null)
            {
                Route MyRoute = e.Result;
                MapRoute MyMapRoute = new MapRoute(MyRoute);
                map_Fullscreen.AddRoute(MyMapRoute);

                List<string> RouteList = new List<string>();
                foreach (RouteLeg leg in MyRoute.Legs)
                {
                    foreach (RouteManeuver maneuver in leg.Maneuvers)
                    {
                        RouteList.Add(maneuver.InstructionText);
                    }
                }

                MyQuery.Dispose();
            }
        }
        #endregion

        #region PushPin
        // pinGPS variable
        MapLayer mapLayer = new MapLayer();
        MapOverlay mapOverlay = new MapOverlay();

        private void pinGPS(string name, GeoCoordinate geoCoor)
        {
            Pushpin pushPin = (Pushpin)this.FindName("pushPin_GPS");
            if (pushPin == null)
            {// first time in
                pushPin = new Pushpin();
            }
            else
            {
                mapLayer.Remove(mapOverlay);
                map_Fullscreen.Layers.Remove(mapLayer);
                //return;
            }
            mapOverlay = new MapOverlay();
            mapLayer = new MapLayer();
            //RegisterName("TextBlock1", Var_TextBox);
            pushPin.Content = "You're here";
            pushPin.Name = "pushPin_GPS";
            mapOverlay.Content = pushPin;
            mapOverlay.GeoCoordinate = geoCoor;
            mapLayer.Add(mapOverlay);
            map_Fullscreen.Layers.Add(mapLayer);
            
            //GeoCoordinate geoCoor_EventVenue = new GeoCoordinate(VENUE_LAT, VENUE_LONG);
            //pushPin_EventVenue.Tag = "eventVenue";
            //Pushpin pushPin_EventVenue = (Pushpin)this.FindName("MyPushpin");
            //var pushPin_EventVenue = MapExtensions.GetChildren(map_Fullscreen).OfType<Pushpin>().First(p => p.Name == "MyPushpin");
        }
        private void pinEventVenue()
        {
            MapLayer mapLayer_EventVenue = new MapLayer();
            MapOverlay mapOverlay_EventVenue = new MapOverlay();
            GeoCoordinate geoCoor_EventVenue = new GeoCoordinate(VENUE_LAT, VENUE_LONG);
            pushPin_EventVenue.Content = EVENTVENUEPINTEXT;
            mapOverlay_EventVenue.Content = pushPin_EventVenue;
            mapOverlay_EventVenue.GeoCoordinate = geoCoor_EventVenue;
            mapLayer_EventVenue.Add(mapOverlay_EventVenue);
            map_Fullscreen.Layers.Add(mapLayer_EventVenue);
            pushPin_EventVenue.MouseLeftButtonDown += pushPin_EventVenue_MouseLeftButtonDown;

            //pushPin_EventVenue.Tag = "eventVenue";
            //pushPin_EventVenue.Name = "eventVenue";
            //Pushpin pushPin_EventVenue = (Pushpin)this.FindName("MyPushpin");
            //var pushPin_EventVenue = MapExtensions.GetChildren(map_Fullscreen).OfType<Pushpin>().First(p => p.Name == "MyPushpin");
        }
        void pushPin_EventVenue_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isMessageShown)
            {
                EVENTVENUEPINTEXT = "InfoTrek (Penang Office)\n1F-78(B), QB Mall\nBeside QB Mall Management Office";
                pushPin_EventVenue.Content = EVENTVENUEPINTEXT;
                isMessageShown = true;
                //return;
            }
            else {
                EVENTVENUEPINTEXT = "";
                pushPin_EventVenue.Content = EVENTVENUEPINTEXT;
                isMessageShown = false;
                //return;
            }
            
        }
        #endregion

        #region Sample
        public void sampleCode()
        {
            MapLayer layer0 = new MapLayer();
            MapOverlay overlay0 = new MapOverlay();
            MapOverlay overlay1 = new MapOverlay();
            MapOverlay overlay2 = new MapOverlay();
            Pushpin pushpin0 = new Pushpin();
            Pushpin pushpin1 = new Pushpin();
            Pushpin pushpin2 = new Pushpin();

            //Pushpin pushpin0 = (Pushpin)this.FindName("pushpin0");
            //Pushpin pushpin0 = MapExtensions.GetChildren(myMap).OfType<Pushpin>().First(p => p.Name == "pushpin0");
            //if (pushpin0 == null) { pushpin0 = new Pushpin(); }
            overlay0.GeoCoordinate = new GeoCoordinate(37.228510, -80.422860);
            pushpin0.GeoCoordinate = new GeoCoordinate(37.228510, -80.422860);
            pushpin1.GeoCoordinate = new GeoCoordinate(37.226399, -80.425271);
            pushpin2.GeoCoordinate = new GeoCoordinate(37.228900, -80.427450);
            overlay0.Content = pushpin0;
            overlay1.Content = pushpin1;
            overlay2.Content = pushpin2;
            layer0.Add(overlay0);
            layer0.Add(overlay1);
            layer0.Add(overlay2);
        }
        public void samplecodetwo()
        {
            MapLayer layer0 = new MapLayer();
            MapOverlay overlay0 = new MapOverlay();
            MapOverlay overlay1 = new MapOverlay();
            MapOverlay overlay2 = new MapOverlay();
            Pushpin pushpin0 = new Pushpin();
            Pushpin pushpin1 = new Pushpin();
            Pushpin pushpin2 = new Pushpin();
            //Pushpin pushpin0 = (Pushpin)this.FindName("pushpin0");
            //Pushpin pushpin0 = MapExtensions.GetChildren(myMap).OfType<Pushpin>().First(p => p.Name == "pushpin0");
            //if (pushpin0 == null) { pushpin0 = new Pushpin(); }
            pushpin0.GeoCoordinate = new GeoCoordinate(37.228510, -80.422860);
            pushpin1.GeoCoordinate = new GeoCoordinate(37.226399, -80.425271);
            pushpin2.GeoCoordinate = new GeoCoordinate(37.228900, -80.427450);
            overlay0.GeoCoordinate = new GeoCoordinate(37.228510, -80.422860);
            overlay0.Content = pushpin0;
            overlay1.Content = pushpin1;
            overlay2.Content = pushpin2;
            layer0.Add(overlay0);
            layer0.Add(overlay1);
            layer0.Add(overlay2);

            // Add the layer with the pins in to the map
            map_Fullscreen.Layers.Add(layer0);
            //ContentPanel.Children.Add(myMap);
        }
        private async void ShowMyLocationOnTheMap()
        {
            // Get my current location.
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            GeoCoordinate myGeoCoordinate =
                CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);
            // Make my current location the center of the Map.
            this.map_Fullscreen.Center = myGeoCoordinate;
            this.map_Fullscreen.ZoomLevel = 16;
            // Create a small circle to mark the current location.
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;
            // Create a MapOverlay to contain the circle.
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = myGeoCoordinate;
            // Create a MapLayer to contain the MapOverlay.
            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);
            // Add the MapLayer to the Map.
            map_Fullscreen.Layers.Add(myLocationLayer);
        }
        #endregion
        
    }
}