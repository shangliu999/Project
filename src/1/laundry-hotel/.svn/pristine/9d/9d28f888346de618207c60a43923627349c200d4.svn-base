using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
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
    public class InFactoryModel : ViewModelBase
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

        private int factoryTotal;
        public int FactoryTotal
        {
            get { return factoryTotal; }
            set
            {
                factoryTotal = value;
                this.RaisePropertyChanged("FactoryTotal");
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

        private ObservableCollection<FactoryTableModel> factoryTable;
        public ObservableCollection<FactoryTableModel> FactoryTable
        {
            get { return factoryTable; }
            set
            {
                factoryTable = value;
                this.RaisePropertyChanged("FactoryTable");
            }
        }
    }
    public class FactoryTableModel : ViewModelBase
    {
        private string invId;
        public string InvId
        {
            get { return invId; }
            set
            {
                invId = value;
                this.RaisePropertyChanged("InvId");
            }
        }

        private string invNo;
        public string InvNo
        {
            get { return invNo; }
            set
            {
                invNo = value;
                this.RaisePropertyChanged("InvNo");
            }
        }

        private string hotelName;
        public string HotelName
        {
            get { return hotelName; }
            set
            {
                hotelName = value;
                this.RaisePropertyChanged("ClassName");
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

        private List<string> tagNos;
        public List<string> TagNos
        {
            get { return tagNos; }
            set
            {
                tagNos = value;
                this.RaisePropertyChanged("TagNos");
            }
        }

        private int scanState;
        public int ScanState
        {
            get { return scanState; }
            set
            {
                scanState = value;
                this.RaisePropertyChanged("ScanState");
            }
        }
    }
}
