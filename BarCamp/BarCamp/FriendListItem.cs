using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarCamp
{
    [Table]
    public class FriendListItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        
        private int _friendListItemId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int FriendListItemId
        {
            get
            {
                return _friendListItemId;
            }
            set
            {
                if (_friendListItemId != value)
                {
                    NotifyPropertyChanging("FriendListItemId");
                    _friendListItemId = value;
                    NotifyPropertyChanged("FriendListItemId");
                }
            }
        }

        private string _friendName;
        [Column]
        public string FriendName
        {
            get
            {
                return _friendName;
            }
            set
            {
                if (_friendName != value)
                {
                    NotifyPropertyChanging("FriendName");
                    _friendName = value;
                    NotifyPropertyChanged("FriendName");
                }
            }
        }

        private string _friendPhone;
        [Column]
        public string FriendPhone
        {
            get
            {
                return _friendPhone;
            }
            set
            {
                if (_friendPhone != value)
                {
                    NotifyPropertyChanging("FriendPhone");
                    _friendPhone = value;
                    NotifyPropertyChanged("FriendPhone");
                }
            }
        }

        private string _friendEmail;
        [Column]
        public string FriendEmail
        {
            get
            {
                return _friendEmail;
            }
            set
            {
                if (_friendEmail != value)
                {
                    NotifyPropertyChanging("FriendEmail");
                    _friendEmail = value;
                    NotifyPropertyChanged("FriendEmail");
                }
            }
        }

        // pro stands for professional
        private string _friendPro;
        [Column]
        public string FriendPro
        {
            get
            {
                return _friendPro;
            }
            set
            {
                if (_friendPro != value)
                {
                    NotifyPropertyChanging("FriendPro");
                    _friendPro = value;
                    NotifyPropertyChanged("FriendPro");
                }
            }
        }

        private string _friendFbId;
        [Column]
        public string FriendFbId
        {
            get
            {
                return _friendFbId;
            }
            set
            {
                if (_friendFbId != value)
                {
                    NotifyPropertyChanging("FriendFbId");
                    _friendFbId = value;
                    NotifyPropertyChanged("FriendFbId");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
}
