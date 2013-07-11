using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Input;
using System.Windows.Media;

namespace BarCamp
{
    public partial class IndoorMap : PhoneApplicationPage
    {
        public IndoorMap()
        {
            InitializeComponent();
        }
        #region Pinch Image
        private void Image_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Image img = (Image)this.FindName("img_SouthZoneMap");
            if (e.PinchManipulation != null)
            {
                var transform = (CompositeTransform)img.RenderTransform;

                // Scale Manipulation
                transform.ScaleX = e.PinchManipulation.CumulativeScale;
                transform.ScaleY = e.PinchManipulation.CumulativeScale;

                // Translate manipulation
                var originalCenter = e.PinchManipulation.Original.Center;
                var newCenter = e.PinchManipulation.Current.Center;
                transform.TranslateX = newCenter.X - originalCenter.X;
                transform.TranslateY = newCenter.Y - originalCenter.Y;

                // Rotation manipulation
                transform.Rotation = angleBetween2Lines(
                    e.PinchManipulation.Current,
                    e.PinchManipulation.Original);

                // end 
                e.Handled = true;
            }
        }
        // copied from http://www.developer.nokia.com/Community/Wiki/Real-time_rotation_of_the_Windows_Phone_8_Map_Control
        public static double angleBetween2Lines(PinchContactPoints line1, PinchContactPoints line2)
        {
            if (line1 != null && line2 != null)
            {
                double angle1 = Math.Atan2(line1.PrimaryContact.Y - line1.SecondaryContact.Y,
                                           line1.PrimaryContact.X - line1.SecondaryContact.X);
                double angle2 = Math.Atan2(line2.PrimaryContact.Y - line2.SecondaryContact.Y,
                                           line2.PrimaryContact.X - line2.SecondaryContact.X);
                return (angle1 - angle2) * 180 / Math.PI;
            }
            else { return 0.0; }
        }
        #endregion

        private void btn_BackHome_Click(object sender, RoutedEventArgs e)
        {
            // backentry not cleared
            //while (IndoorMap.RemoveBackEntry() != null)
            //{
            //    IndoorMap.RemoveBackEntry();
            //};
            NavigationService.Navigate(new Uri("/MainPage.xaml",UriKind.RelativeOrAbsolute));
        }

    }
}