using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;

namespace ExtendedSplash
{
    public partial class ExtendedSplashScreen : PhoneApplicationPage
    {
        public ExtendedSplashScreen()
        {
            InitializeComponent();

            //Call MainPage from ExtendedSplashScreen after some delay
            Splash_Screen();
        }

        async void Splash_Screen()
        {
            await Task.Delay(TimeSpan.FromSeconds(3)); // set your desired delay
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); // call MainPage

        }
    }
}