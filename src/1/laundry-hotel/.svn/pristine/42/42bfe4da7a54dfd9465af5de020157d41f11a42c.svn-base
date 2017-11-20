using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Model.Choose;
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
    public class SendModel : ViewModelBase
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

        private Region hotel;
        public Region Hotel
        {
            get { return hotel; }
            set
            {
                hotel = value;
                this.RaisePropertyChanged("Hotel");
            }
        }

        private Region region;
        public Region Region
        {
            get { return region; }
            set
            {
                region = value;
                this.RaisePropertyChanged("Region");
            }
        }

        private string sendTaskStr;
        public string SendTaskStr
        {
            get { return sendTaskStr; }
            set
            {
                sendTaskStr = value;
                this.RaisePropertyChanged("SendTaskStr");
            }
        }

        private ObservableCollection<string> bags;
        public ObservableCollection<string> Bags
        {
            get { return bags; }
            set
            {
                bags = value;
                this.RaisePropertyChanged("Bags");
            }
        }

        private string bagsStr;
        public string BagsStr
        {
            get { return bagsStr; }
            set
            {
                bagsStr = value;
                this.RaisePropertyChanged("BagsStr");
            }
        }

        private Visibility _RegionVisibility;
        public Visibility RegionVisibility
        {
            get { return _RegionVisibility; }
            set
            {
                _RegionVisibility = value;
                this.RaisePropertyChanged("RegionVisibility");
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

        private bool _UnDoEnabled;
        public bool UnDoEnabled
        {
            get { return _UnDoEnabled; }
            set
            {
                _UnDoEnabled = value;
                this.RaisePropertyChanged("UnDoEnabled");
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
        
        private ObservableCollection<SendTableModel> sendTable;
        public ObservableCollection<SendTableModel> SendTable
        {
            get { return sendTable; }
            set
            {
                sendTable = value;
                this.RaisePropertyChanged("SendTable");
            }
        }
    }

    public class SendTableModel : ViewModelBase
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

        private int taskCount;
        public int TaskCount
        {
            get { return taskCount; }
            set
            {
                taskCount = value;
                this.RaisePropertyChanged("TaskCount");
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

        private int diffCount;

        public int DiffCount
        {
            get { return diffCount; }
            set
            {
                diffCount = value;
                this.RaisePropertyChanged("DiffCount");
            }
        }
    }
}
