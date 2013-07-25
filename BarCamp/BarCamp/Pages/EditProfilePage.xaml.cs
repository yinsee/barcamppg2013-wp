using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using Facebook;


namespace BarCamp
{
    public partial class EditProfilePage : PhoneApplicationPage
    {
        public EditProfilePage()
        {
            InitializeComponent();
            this.Loaded += EditProfilePage_Loaded;
            
        }
        void EditProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (txtBlk_FbId.Text.Length > 1)
            {
                img_fbConnectBtn.Visibility = Visibility.Collapsed;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string msg = "";// get fbid from fbloginpage
            if (NavigationContext.QueryString.TryGetValue("msg", out msg))
            {
                txtBlk_FbId.Text = msg;
                LoadUserInfo();
                shreadAndSet(App.ForQRCodeString);
                base.OnNavigatedTo(e);
            }
            else if (NavigationContext.QueryString.TryGetValue("decode", out msg)) { 
                shreadAndSet(msg);
                txtBlk_FbId.Text = App.FacebookId;
                fbInvoke();
            }
        }
        //private void btn_Submit_Click(object sender, RoutedEventArgs e)
        //{
        //    if (isEmpty())
        //    {
        //        return;
        //    }
        //    NavigationService.Navigate(new Uri("/MainPage.xaml?goto=1&msg=" + getInfo(), UriKind.Relative));
        //}
        private void img_fbConnectBtn_Tap(object sender, RoutedEventArgs e)
        {
            App.ForQRCodeString = getInfo();
            NavigationService.Navigate(new Uri("/Pages/FacebookLoginPage.xaml", UriKind.Relative));
        }
        private void img_saveUpateBtn_Tap(object sender, RoutedEventArgs e)
        {
            if (isEmpty())
            {
                return;
            }
            NavigationService.Navigate(new Uri("/MainPage.xaml?goto=1&msg=" + getInfo(), UriKind.Relative));
        }
        private void shreadAndSet(string msg)
        {
            string[] separators = new string[] { "||" };
            string[] shread = msg.Split(separators, StringSplitOptions.None);
            try
            {
                txtBox_Name.Text = shread[0];
                txtBox_Phone.Text = shread[1];
                txtBox_Email.Text = shread[2];
                txtBox_Profession.Text = shread[3];
            }
            catch (Exception e)
            {
                txtBox_Name.Text = string.Empty;
                txtBox_Phone.Text = string.Empty;
                txtBox_Email.Text = string.Empty;
                txtBox_Profession.Text = string.Empty;
                return;
            }
        }
        private string getInfo()
        {
            string forEncode = "";
            forEncode =
                txtBox_Name.Text + "||" +
                txtBox_Phone.Text + "||" +
                txtBox_Email.Text + "||" +
                txtBox_Profession.Text + "||" +
                txtBlk_FbId.Text;
            return forEncode;
        }
        private bool isEmpty()
        {
            if (txtBox_Name.Text.Equals(""))
            {
                MessageBox.Show("Please fill in your name");
                return true;
            } if (txtBox_Phone.Text.Equals(""))
            {
                MessageBox.Show("Please fill in your contact");
                return true;
            } if (txtBox_Email.Text.Equals(""))
            {
                MessageBox.Show("Please fill in your email");
                return true;
            } if (txtBox_Profession.Text.Equals(""))
            {
                MessageBox.Show("Please fill in your profession");
                return true;
            }
            return false;
        }
        
        //Personalize
        #region personalize
        /*retrieves the user profile data. 
         * It additionally creates a URL for the user's profile picture 
         * and sets it as the source of the image. 
         * This causes the image to automatically retrieve the profile picture and load it correctly.
        */
        private void LoadUserInfo()
        {
            var fb = new FacebookClient(App.AccessToken);

            fb.GetCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(e.Error.Message));
                    return;
                }

                var result = (IDictionary<string, object>)e.GetResultData();

                Dispatcher.BeginInvoke(() =>
                {
                    // try to mask it
                    fbInvoke();
                    //this.MyName.Text = String.Format("{0} {1}", (string)result["first_name"], (string)result["last_name"]);
                });
            };

            fb.GetTaskAsync("me");
        }
        private void fbInvoke() {
            var profilePictureUrl = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", App.FacebookId, "square", App.AccessToken);
            this.img_fbImg.Source = new BitmapImage(new Uri(profilePictureUrl));
        }
        #endregion
    }
}