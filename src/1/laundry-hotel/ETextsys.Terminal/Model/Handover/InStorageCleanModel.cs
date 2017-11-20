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
    public class InStorageCleanModel : ViewModelBase
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

        private int storagedTotal;
        public int StoragedTotal
        {
            get { return storagedTotal; }
            set
            {
                storagedTotal = value;
                this.RaisePropertyChanged("StoragedTotal");
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

        private bool _BtnStorageEnabled;
        public bool BtnStorageEnabled
        {
            get { return _BtnStorageEnabled; }
            set
            {
                _BtnStorageEnabled = value;
                this.RaisePropertyChanged("BtnStorageEnabled");
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

        private int houseId;
        public int HouseID
        {
            get { return houseId; }
            set
            {
                houseId = value;
                this.RaisePropertyChanged("HouseID");
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

        private ObservableCollection<StorageTableModel> storageTable;
        public ObservableCollection<StorageTableModel> StorageTable
        {
            get { return storageTable; }
            set
            {
                storageTable = value;
                this.RaisePropertyChanged("StorageTable");
            }
        }

        /// <summary>
        /// 架子车
        /// </summary>
        private string _Truck;
        public string Truck
        {
            get { return _Truck; }
            set
            {
                _Truck = value;
                this.RaisePropertyChanged("Truck");
            }
        }
    }

    public class StorageTableModel : ViewModelBase
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
