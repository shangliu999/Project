using Etextsys.Terminal.Domain;
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
    public class ScrapModel : ViewModelBase
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

        private Visibility _hotelVisibled;
        public Visibility HotelVisibled
        {
            get { return _hotelVisibled; }
            set
            {
                _hotelVisibled = value;
                this.RaisePropertyChanged("HotelVisibled");
            }
        }

        private int _scrapId;
        public int ScrapID
        {
            get { return _scrapId; }
            set
            {
                _scrapId = value;
                this.RaisePropertyChanged("ScrapID");
            }
        }

        private string _scrapName;
        public string ScrapName
        {
            get { return _scrapName; }
            set
            {
                _scrapName = value;
                this.RaisePropertyChanged("ScrapName");
            }
        }

        private int _ResponsibleType;
        public int ResponsibleType
        {
            get { return _ResponsibleType; }
            set
            {
                _ResponsibleType = value;
                this.RaisePropertyChanged("ResponsibleType");
            }
        }

        private string _ResponsibleTypeName;
        public string ResponsibleTypeName
        {
            get { return _ResponsibleTypeName; }
            set
            {
                _ResponsibleTypeName = value;
                this.RaisePropertyChanged("ResponsibleTypeName");
            }
        }

        private int _ResponsibleID;
        public int ResponsibleID
        {
            get { return _ResponsibleID; }
            set
            {
                _ResponsibleID = value;
                this.RaisePropertyChanged("ResponsibleID");
            }
        }

        private string _ResponsibleName;
        public string ResponsibleName
        {
            get { return _ResponsibleName; }
            set
            {
                _ResponsibleName = value;
                this.RaisePropertyChanged("ResponsibleName");
            }
        }

        private ObservableCollection<ScrapTableModel> _scrapTable;
        public ObservableCollection<ScrapTableModel> ScrapTable
        {
            get { return _scrapTable; }
            set
            {
                _scrapTable = value;
                this.RaisePropertyChanged("ScrapTable");
            }
        }
    }

    public class ScrapTableModel : ViewModelBase
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

        private string _rfidTagNo;
        public string RFIDTagNo
        {
            get { return _rfidTagNo; }
            set
            {
                _rfidTagNo = value;
                this.RaisePropertyChanged("RFIDTagNo");
            }
        }

        private string _hotelName;
        public string HotelName
        {
            get { return _hotelName; }
            set
            {
                _hotelName = value;
                this.RaisePropertyChanged("HotelName");
            }
        }

        private int _TextileWashtime;
        public int TextileWashtime
        {
            get { return _TextileWashtime; }
            set
            {
                _TextileWashtime = value;
                this.RaisePropertyChanged("TextileWashtime");
            }
        }
    }
}
