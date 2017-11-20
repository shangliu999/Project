using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.Warehouse
{
    public class RFIDReplaceModel : ViewModelBase
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

        private bool _replaceBtnEnable;
        public bool ReplaceBtnEnable
        {
            get { return _replaceBtnEnable; }
            set
            {
                _replaceBtnEnable = value;
                this.RaisePropertyChanged("ReplaceBtnEnable");
            }
        }

        private string _oldRFIDTagNo;
        public string OldRFIDTagNo
        {
            get { return _oldRFIDTagNo; }
            set
            {
                _oldRFIDTagNo = value;
                this.RaisePropertyChanged("OldRFIDTagNo");
            }
        }

        private string _newRFIDTagNo;
        public string NewRFIDTagNo
        {
            get { return _newRFIDTagNo; }
            set
            {
                _newRFIDTagNo = value;
                this.RaisePropertyChanged("NewRFIDTagNo");
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

        private Visibility _oldReaderIconVisibled;
        public Visibility OldReaderIconVisibled
        {
            get { return _oldReaderIconVisibled; }
            set
            {
                _oldReaderIconVisibled = value;
                this.RaisePropertyChanged("OldReaderIconVisibled");
            }
        }

        private Visibility _newReaderIconVisibled;
        public Visibility NewReaderIconVisibled
        {
            get { return _newReaderIconVisibled; }
            set
            {
                _newReaderIconVisibled = value;
                this.RaisePropertyChanged("NewReaderIconVisibled");
            }
        }

        private Visibility _oldIconVisibled;
        public Visibility OldIconVIsibled
        {
            get { return _oldIconVisibled; }
            set
            {
                _oldIconVisibled = value;
                this.RaisePropertyChanged("OldIconVIsibled");
            }
        }

        private Visibility _newIconVisibled;
        public Visibility NewIconVIsibled
        {
            get { return _newIconVisibled; }
            set
            {
                _newIconVisibled = value;
                this.RaisePropertyChanged("NewIconVIsibled");
            }
        }

        private Uri _oldIconSource;
        public Uri OldIconSource
        {
            get { return _oldIconSource; }
            set
            {
                _oldIconSource = value;
                this.RaisePropertyChanged("OldIconSource");
            }
        }

        private Uri _newIconSource;
        public Uri NewIconSource
        {
            get { return _newIconSource; }
            set
            {
                _newIconSource = value;
                this.RaisePropertyChanged("NewIconSource");
            }
        }

        private string _prompt;
        public string Prompt
        {
            get { return _prompt; }
            set
            {
                _prompt = value;
                this.RaisePropertyChanged("Prompt");
            }
        }

        private string _brandName;
        public string BrandName
        {
            get { return _brandName; }
            set
            {
                _brandName = value;
                this.RaisePropertyChanged("BrandName");
            }
        }

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

        private string _WashingTime;
        public string WashingTime
        {
            get { return _WashingTime; }
            set
            {
                _WashingTime = value;
                this.RaisePropertyChanged("WashingTime");
            }
        }

        private string _left;
        public string Left
        {
            get { return _left; }
            set
            {
                _left = value;
                this.RaisePropertyChanged("Left");
            }
        }

        private string _RFIDWashingTime;
        public string RFIDWashingTime
        {
            get { return _RFIDWashingTime; }
            set
            {
                _RFIDWashingTime = value;
                this.RaisePropertyChanged("RFIDWashingTime");
            }
        }

        private string _CostTime;
        public string CostTime
        {
            get { return _CostTime; }
            set
            {
                _CostTime = value;
                this.RaisePropertyChanged("CostTime");
            }
        }
    }
}
