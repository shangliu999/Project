using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.Logistics
{
    public class ReceiveModel : ViewModelBase
    {
        /// <summary>
        /// 读写器状态 0 无法扫描 1 扫描中 2暂停
        /// </summary>
        private int readerState;
        public int ReaderState
        {
            get { return readerState; }
            set
            {
                readerState = value;
                this.RaisePropertyChanged("ReaderState");
            }
        }

        private Uri readerLight;
        public Uri ReaderLight
        {
            get
            {
                string path = ReaderState == 0 ? ApiController.NotScan : (readerState == 1 ? ApiController.Scan : ApiController.StopScan);
                return new Uri(path, UriKind.RelativeOrAbsolute);
            }
            set
            {
                readerLight = value;
                this.RaisePropertyChanged("ReaderLight");
            }
        }

        private int unRegisterTotal;
        public int UnRegisterTotal
        {
            get { return unRegisterTotal; }
            set
            {
                unRegisterTotal = value;
                this.RaisePropertyChanged("UnRegisterTotal");
            }
        }

        private int textileCount;
        public int TextileCount
        {
            get { return textileCount; }
            set
            {
                textileCount = value;
                this.RaisePropertyChanged("TextileCount");
            }
        }

        private int receivedTotal;
        public int ReceivedTotal
        {
            get { return receivedTotal; }
            set
            {
                receivedTotal = value;
                this.RaisePropertyChanged("ReceivedTotal");
            }
        }

        private int hotelId;
        public int HotelID
        {
            get { return hotelId; }
            set
            {
                hotelId = value;
                this.RaisePropertyChanged("HotelID");
            }
        }

        private string hotelName;
        public string HotelName
        {
            get { return hotelName; }
            set
            {
                hotelName = value;
                this.RaisePropertyChanged("HotelName");
            }
        }

        private int regionId;
        public int RegionID
        {
            get { return regionId; }
            set
            {
                regionId = value;
                this.RaisePropertyChanged("RegionID");
            }
        }

        private string regionName;
        public string RegionName
        {
            get { return regionName; }
            set
            {
                regionName = value;
                this.RaisePropertyChanged("RegionName");
            }
        }

        private string receiveTypeName;
        public string ReceiveTypeName
        {
            get { return receiveTypeName; }
            set
            {
                receiveTypeName = value;
                this.RaisePropertyChanged("ReceiveTypeName");
            }
        }

        private short receiveType;
        public short ReceiveType
        {
            get { return receiveType; }
            set
            {
                receiveType = value;
                this.RaisePropertyChanged("ReceiveType");
            }
        }

        private bool _BtnReceiveEnabled;
        public bool BtnReceiveEnabled
        {
            get { return _BtnReceiveEnabled; }
            set
            {
                _BtnReceiveEnabled = value;
                this.RaisePropertyChanged("BtnReceiveEnabled");
            }
        }

        private bool _PrintEnabled;
        public bool PrintEnabled
        {
            get { return _PrintEnabled; }
            set
            {
                _PrintEnabled = value;
                this.RaisePropertyChanged("PrintEnabled");
            }
        }

        private bool _SubmitEnabled;
        public bool SubmitEnabled
        {
            get { return _SubmitEnabled; }
            set
            {
                _SubmitEnabled = value;
                this.RaisePropertyChanged("SubmitEnabled");
            }
        }

        private bool _CancelEnabled;
        public bool CancelEnabled
        {
            get { return _CancelEnabled; }
            set
            {
                _CancelEnabled = value;
                this.RaisePropertyChanged("CancelEnabled");
            }
        }

        private Visibility _WaitVisibled;
        public Visibility WaitVisibled
        {
            get { return _WaitVisibled; }
            set
            {
                _WaitVisibled = value;
                this.RaisePropertyChanged("WaitVisibled");
            }
        }

        private string _WaitContent;
        public string WaitContent
        {
            get { return _WaitContent; }
            set
            {
                _WaitContent = value;
                this.RaisePropertyChanged("WaitContent");
            }
        }

        private ObservableCollection<string> receiveTags;
        public ObservableCollection<string> ReceiveTags
        {
            get { return receiveTags; }
            set
            {
                receiveTags = value;
                this.RaisePropertyChanged("ReceiveTags");
            }
        }

        private ObservableCollection<ReceiveTableModel> receiveTable;
        public ObservableCollection<ReceiveTableModel> ReceiveTable
        {
            get { return receiveTable; }
            set
            {
                receiveTable = value;
                this.RaisePropertyChanged("ReceiveTable");
            }
        }
    }

    public class ReceiveTableModel : ViewModelBase
    {
        private string brandName;
        public string BrandName
        {
            get { return brandName; }
            set
            {
                brandName = value;
                this.RaisePropertyChanged("BrandName");
            }
        }

        private string className;
        public string ClassName
        {
            get { return className; }
            set
            {
                className = value;
                this.RaisePropertyChanged("ClassName");
            }
        }

        private string sizeName;
        public string SizeName
        {
            get { return sizeName; }
            set
            {
                sizeName = value;
                this.RaisePropertyChanged("SizeName");
            }
        }

        private int textileCount;
        public int TextileCount
        {
            get { return textileCount; }
            set
            {
                textileCount = value;
                this.RaisePropertyChanged("TextileCount");
            }
        }
    }
}
