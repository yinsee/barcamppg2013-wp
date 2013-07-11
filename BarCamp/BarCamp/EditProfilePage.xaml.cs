using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;


namespace BarCamp
{
    public partial class EditProfilePage : PhoneApplicationPage
    {
        public EditProfilePage()
        {
            InitializeComponent();
        }
        People newPerson = new People();
        private void btn_Submit_Click(object sender, RoutedEventArgs e)
        {
            getInfo();
            string msg = newPerson.getAll(newPerson);
            NavigationService.Navigate(new Uri("/MainPage.xaml?goto=1&msg=" + msg, UriKind.Relative));
        }
        public void getInfo(){
            newPerson.Name = txtBox_Name.Text;
            newPerson.Phone = txtBox_Phone.Text;            
            newPerson.Email = txtBox_Email.Text;
            newPerson.Profession = txtBox_Profession.Text;
        }
    }
}