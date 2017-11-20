using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.Business
{
    public class BillInquiry02Model : ViewModelBase
    {
        private Visibility _hotelInfoVisbility;
        public Visibility HotelVisbility
        {
            get { return _hotelInfoVisbility; }
            set
            {
                _hotelInfoVisbility = value;
                this.RaisePropertyChanged("HotelVisbility");
            }
        }

        private string _bags;
        public string Bags
        {
            get { return _bags; }
            set
            {
                _bags = value;
                this.RaisePropertyChanged("Bags");
            }
        }

        private string waitContent;
        public string WaitContent
        {
            get { return waitContent; }
            set
            {
                waitContent = value;
                RaisePropertyChanged("WaitContent");
            }
        }


        private string waitVisibled;
        public string WaitVisibled
        {
            get { return waitVisibled; }
            set
            {
                waitVisibled = value;
                RaisePropertyChanged("WaitVisibled");
            }
        }

        private string isEnabled;
        public string IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        private int invType;
        public int InvType
        {
            get { return invType; }
            set
            {
                invType = value;
                RaisePropertyChanged("InvType");
            }
        }

        private string invTypeName;
        public string InvTypeName
        {
            get { return invTypeName; }
            set
            {
                invTypeName = value;
                RaisePropertyChanged("InvTypeName");
            }
        }

        private int hotelId;
        public int HotelId
        {
            get { return hotelId; }
            set
            {
                hotelId = value;
                RaisePropertyChanged("HotelId");
            }
        }

        private string hotelName;
        public string HotelName
        {
            get { return hotelName; }
            set
            {
                hotelName = value;
                RaisePropertyChanged("HotelName");
            }
        }

        private int regionID;
        public int RegionID
        {
            get { return regionID; }
            set
            {
                regionID = value;
                RaisePropertyChanged("RegionID");
            }
        }

        private string regionName;
        public string RegionName
        {
            get { return regionName; }
            set
            {
                regionName = value;
                RaisePropertyChanged("RegionName");
            }
        }

        private string createTime;
        public string CreateTime
        {
            get { return createTime; }
            set
            {
                createTime = value;
                RaisePropertyChanged("CreateTime");
            }
        }

        private ObservableCollection<resposparameter> resetTable;
        public ObservableCollection<resposparameter> ResetTable
        {
            get { return resetTable; }
            set
            {
                resetTable = value;
                RaisePropertyChanged("ResetTable");
            }
        }
    }

    public class resposparameter : ViewModelBase
    {
        private string productName;
        public string ProductName
        {
            get { return productName; }
            set
            {
                productName = value;
                RaisePropertyChanged("ProductName");
            }
        }

        private string size;

        public string Size
        {
            get { return size; }
            set
            {
                size = value;
                RaisePropertyChanged("Size");
            }
        }

        private int number;

        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                RaisePropertyChanged("Number");
            }
        }


    }
}
