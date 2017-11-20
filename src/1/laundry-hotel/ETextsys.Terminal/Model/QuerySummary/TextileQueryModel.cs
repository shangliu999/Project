using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.QuerySummary
{
    public class TextileQueryModel : ViewModelBase
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

        private bool _exportEnabled;
        public bool ExportEnabled
        {
            get { return _exportEnabled; }
            set
            {
                _exportEnabled = value;
                this.RaisePropertyChanged("ExportEnabled");
            }
        }

        private bool _singleBtnChoosed;
        public bool SingleBtnChoosed
        {
            get { return _singleBtnChoosed; }
            set
            {
                _singleBtnChoosed = value;
                this.RaisePropertyChanged("SingleBtnChoosed");
            }
        }

        private bool _areaBtnChoosed;
        public bool AreaBtnChoosed
        {
            get { return _areaBtnChoosed; }
            set
            {
                _areaBtnChoosed = value;
                this.RaisePropertyChanged("AreaBtnChoosed");
            }
        }

        private Visibility _textileAreaVisibiled;
        public Visibility TextileAreaVisibiled
        {
            get { return _textileAreaVisibiled; }
            set
            {
                _textileAreaVisibiled = value;
                this.RaisePropertyChanged("TextileAreaVisibiled");
            }
        }

        private Visibility _textileSingleVisibiled;
        public Visibility TextileSingleVisibiled
        {
            get { return _textileSingleVisibiled; }
            set
            {
                _textileSingleVisibiled = value;
                this.RaisePropertyChanged("TextileSingleVisibiled");
            }
        }

        private ObservableCollection<TextileAreaTableModel> _textileAreaTable;
        public ObservableCollection<TextileAreaTableModel> TextileAreaTable
        {
            get { return _textileAreaTable; }
            set
            {
                _textileAreaTable = value;
                this.RaisePropertyChanged("TextileAreaTable");
            }
        }

        private ObservableCollection<TextileSingleTableModel> _textileSingleTable;
        public ObservableCollection<TextileSingleTableModel> TextileSingleTable
        {
            get { return _textileSingleTable; }
            set
            {
                _textileSingleTable = value;
                this.RaisePropertyChanged("TextileSingleTable");
            }
        }
    }

    public class TextileAreaTableModel : ViewModelBase
    {
        private string _className;
        public string ClassName
        {
            get { return _className; }
            set
            {
                _className = value;
                this.RaisePropertyChanged("ClassName");
            }
        }

        private string _sizeName;
        public string SizeName
        {
            get { return _sizeName; }
            set
            {
                _sizeName = value;
                this.RaisePropertyChanged("SizeName");
            }
        }

        private int _textileCount;
        public int TextileCount
        {
            get { return _textileCount; }
            set
            {
                _textileCount = value;
                this.RaisePropertyChanged("TextileCount");
            }
        }
    }

    public class TextileSingleTableModel : ViewModelBase
    {
        private int _textileId;
        public int TextileId
        {
            get { return _textileId; }
            set
            {
                _textileId = value;
                this.RaisePropertyChanged("TextileId");
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

        private string _costTime;
        public string CostTime
        {
            get { return _costTime; }
            set
            {
                _costTime = value;
                this.RaisePropertyChanged("CostTime");
            }
        }

        private int _RFIDWashtime;
        public int RFIDWashtime
        {
            get { return _RFIDWashtime; }
            set
            {
                _RFIDWashtime = value;
                this.RaisePropertyChanged("RFIDWashtime");
            }
        }
    }
}
