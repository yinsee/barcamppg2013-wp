using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Facebook.Client;
using System.Threading.Tasks;

namespace BarCamp.Pages
{
    public partial class FacebookLoginPage : PhoneApplicationPage
    {
        public FacebookLoginPage()
        {
            InitializeComponent();
            //invokes the Facebook login as soon as we navigate to the FacebookLoginPage
            this.Loaded += FacebookLoginPage_Loaded;
        }        
        //Authenticate
        #region Auth
        //checks if the user is already authenticated and if not, invokes the Authentication.
        async void FacebookLoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.isAuthenticated)
            {
                App.isAuthenticated = true;
                await Authenticate();
            }
        }

        private FacebookSession session;
        /*perform the Authentication 
         * and request read permissions 
         * for the user's profile and other data, 
         * and to navigate to the LandingPage when the login has succeeded.
         */

        private async Task Authenticate()
        {
            string message = String.Empty;
            try
            {
                session = await App.FacebookSessionClient.LoginAsync("user_about_me,read_stream");
                App.AccessToken = session.AccessToken;
                App.FacebookId = session.FacebookId;
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Pages/EditProfilePage.xaml?msg=" + App.FacebookId, UriKind.Relative)));
            }
            catch (InvalidOperationException e)
            {
                message = "Login failed! Exception details: " + e.Message;
                MessageBox.Show(message);
            }
        }
        #endregion

    }
}