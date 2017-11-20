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
    public class AssetsRegModel : ViewModelBase
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


        private int _assetsType;
        public int AssetsType
        {
            get { return _assetsType; }
            set
            {
                _assetsType = value;
                this.RaisePropertyChanged("AssetsType");
            }
        }

        private string _code;
        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;
                this.RaisePropertyChanged("Code");
            }
        }

        private string _assetsTypeName;
        public string AssetsTypeName
        {
            get { return _assetsTypeName; }
            set
            {
                _assetsTypeName = value;
                this.RaisePropertyChanged("AssetsTypeName");
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
}
