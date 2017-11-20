using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.Handover
{
    public class DeliveryCleanModel : ViewModelBase
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

        private int _TruckTotal;
        public int TruckTotal
        {
            get { return _TruckTotal; }
            set
            {
                _TruckTotal = value;
                this.RaisePropertyChanged("TruckTotal");
            }
        }

        private bool _BtnTruckEnabled;
        public bool BtnTruckEnabled
        {
            get { return _BtnTruckEnabled; }
            set
            {
                _BtnTruckEnabled = value;
                this.RaisePropertyChanged("BtnTruckEnabled");
            }
        }

        private string _deliveryTypeName;
        public string DeliveryTypeName
        {
            get { return _deliveryTypeName; }
            set
            {
                _deliveryTypeName = value;
                this.RaisePropertyChanged("DeliveryTypeName");
            }
        }

        private short _deliveryType;
        public short DeliveryType
        {
            get { return _deliveryType; }
            set
            {
                _deliveryType = value;
                this.RaisePropertyChanged("DeliveryType");
            }
        }

        private string _houseName;
        public string HouseName
        {
            get { return _houseName; }
            set
            {
                _houseName = value;
                this.RaisePropertyChanged("HouseName");
            }
        }

        private int _houseID;
        public int HouseID
        {
            get { return _houseID; }
            set
            {
                _houseID = value;
                this.RaisePropertyChanged("HouseID");
            }
        }

        private string _taskNo;
        public string TaskNo
        {
            get { return _taskNo; }
            set
            {
                _taskNo = value;
                this.RaisePropertyChanged("TaskNo");
            }
        }

        private bool _BtnDeliveryedEnabled;
        public bool BtnDeliveryedEnabled
        {
            get { return _BtnDeliveryedEnabled; }
            set
            {
                _BtnDeliveryedEnabled = value;
                this.RaisePropertyChanged("BtnDeliveryedEnabled");
            }
        }

        private int _deliveryedTotal;
        public int DeliveryedTotal
        {
            get { return _deliveryedTotal; }
            set
            {
                _deliveryedTotal = value;
                this.RaisePropertyChanged("DeliveryedTotal");
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

        private Visibility _taskVisibility;
        public Visibility TaskVisibility
        {
            get { return _taskVisibility; }
            set
            {
                _taskVisibility = value;
                this.RaisePropertyChanged("TaskVisibility");
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

        private ObservableCollection<DeliveryTableModel> deliveryTable;
        public ObservableCollection<DeliveryTableModel> DeliveryTable
        {
            get { return deliveryTable; }
            set
            {
                deliveryTable = value;
                this.RaisePropertyChanged("DeliveryTable");
            }
        }
    }

    public class DeliveryTableModel : ViewModelBase
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
