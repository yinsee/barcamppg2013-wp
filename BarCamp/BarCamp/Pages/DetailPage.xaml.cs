using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace BarCamp.Pages
{
    public partial class DetailPage : PhoneApplicationPage//, INotifyPropertyChanged
    {

        private FriendListDataContext friendListDB;

        public DetailPage()
        {
            InitializeComponent();
            // Connect to the database and instantiate data context.
            friendListDB = new FriendListDataContext(FriendListDataContext.DBConnectionString);
            // Data context and observable collection are children of the main page.
            this.DataContext = this;

        }

        FriendListItem FLI = new FriendListItem();
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            updateFields();
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            friendListDB.SubmitChanges();
        }

        public void updateFields()
        {
            FriendListItem friendListItem = (Application.Current as App).app_friendListItem;
            if (friendListItem == null)
            {
                MessageBox.Show("Error!");
                NavigationService.GoBack();
            }
            tb_Name.Text = friendListItem.FriendName;
            tb_Phone.Text = friendListItem.FriendPhone;
            tb_Email.Text = friendListItem.FriendEmail;
            tb_Pro.Text = friendListItem.FriendPro;
            tb_Fb.Text = "facebook.com/" + friendListItem.FriendFbId;

        }

    }
}