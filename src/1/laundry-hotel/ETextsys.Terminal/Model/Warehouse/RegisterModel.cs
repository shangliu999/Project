﻿using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.Warehouse
{
    public class RegisterModel : ViewModelBase
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

        private Int64 brandId;
        public Int64 BrandID
        {
            get { return brandId; }
            set
            {
                brandId = value;
                this.RaisePropertyChanged("BrandID");
            }
        }

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

        private Int64 classId;
        public Int64 ClassID
        {
            get { return classId; }
            set
            {
                classId = value;
                this.RaisePropertyChanged("ClassID");
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

        private Int64 sizeId;
        public Int64 SizeID
        {
            get { return sizeId; }
            set
            {
                sizeId = value;
                this.RaisePropertyChanged("SizeID");
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

        private Int64 fabricId;
        public Int64 FabricID
        {
            get { return fabricId; }
            set
            {
                fabricId = value;
                this.RaisePropertyChanged("FabricID");
            }
        }

        private string fabricName;
        public string FabricName
        {
            get { return fabricName; }
            set
            {
                fabricName = value;
                this.RaisePropertyChanged("FabricName");
            }
        }

        private Int64 colorId;
        public Int64 ColorID
        {
            get { return colorId; }
            set
            {
                colorId = value;
                this.RaisePropertyChanged("ColorID");
            }
        }

        private string colorName;
        public string ColorName
        {
            get { return colorName; }
            set
            {
                colorName = value;
                this.RaisePropertyChanged("ColorName");
            }
        }

        private Int64 storeId;
        public Int64 StoreID
        {
            get { return storeId; }
            set
            {
                storeId = value;
                this.RaisePropertyChanged("StoreID");
            }
        }

        private string storeName;
        public string StoreName
        {
            get { return storeName; }
            set
            {
                storeName = value;
                this.RaisePropertyChanged("StoreName");
            }
        }

        private Int64 textileBrandId;
        public Int64 TextileBrandId
        {
            get { return textileBrandId; }
            set
            {
                textileBrandId = value;
                this.RaisePropertyChanged("TextileBrandId");
            }
        }

        private string textileBrandName;
        public string TextileBrandName
        {
            get { return textileBrandName; }
            set
            {
                textileBrandName = value;
                this.RaisePropertyChanged("TextileBrandName");
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

        private Visibility _SizeVisbilied;
        public Visibility SizeVisbilied
        {
            get { return _SizeVisbilied; }
            set
            {
                _SizeVisbilied = value;
                this.RaisePropertyChanged("SizeVisbilied");
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

        private ObservableCollection<UnRegTableModel> unRegTable;
        public ObservableCollection<UnRegTableModel> UnRegTable
        {
            get { return unRegTable; }
            set
            {
                unRegTable = value;
                this.RaisePropertyChanged("UnRegTable");
            }
        }

        private ObservableCollection<RegTableModel> regTable;
        public ObservableCollection<RegTableModel> RegTable
        {
            get { return regTable; }
            set
            {
                regTable = value;
                this.RaisePropertyChanged("RegTable");
            }
        }
    }

    public class UnRegTableModel : ViewModelBase
    {
        private string rfid;
        public string RFID
        {
            get { return rfid; }
            set
            {
                rfid = value;
                this.RaisePropertyChanged("RFID");
            }
        }

        private int _RFIDWashingTime;
        public int RFIDWashingTime
        {
            get { return _RFIDWashingTime; }
            set
            {
                _RFIDWashingTime = value;
                this.RaisePropertyChanged("RFIDWashingTime");
            }
        }
    }

    public class RegTableModel : ViewModelBase
    {
        private string className;
        public string ClassName
        {
            get
            {
                return className;
            }
            set
            {
                className = value;
                this.RaisePropertyChanged("ClassName");
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
