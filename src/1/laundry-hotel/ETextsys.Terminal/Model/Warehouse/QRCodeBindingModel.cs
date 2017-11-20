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
    public class QRCodeBindingModel : ViewModelBase
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

        private Visibility _rfidReaderVisibility;
        public Visibility RFIDReaderVisibility
        {
            get { return _rfidReaderVisibility; }
            set
            {
                _rfidReaderVisibility = value;
                this.RaisePropertyChanged("RFIDReaderVisibility");
            }
        }

        private Visibility _qrcodeScanVisibility;
        public Visibility QRCodeScanVisibility
        {
            get { return _qrcodeScanVisibility; }
            set
            {
                _qrcodeScanVisibility = value;
                this.RaisePropertyChanged("QRCodeScanVisibility");
            }
        }

        private Visibility _qrcodeVisibility;
        public Visibility QRCodeVisibility
        {
            get { return _qrcodeVisibility; }
            set
            {
                _qrcodeVisibility = value;
                this.RaisePropertyChanged("QRCodeVisibility");
            }
        }

        private Uri _qrcodeState;
        public Uri QRCodeState
        {
            get { return _qrcodeState; }
            set
            {
                _qrcodeState = value;
                this.RaisePropertyChanged("QRCodeState");
            }
        }

        private string _rfidText;
        public string RFIDText
        {
            get { return _rfidText; }
            set
            {
                _rfidText = value;
                this.RaisePropertyChanged("RFIDText");
            }
        }

        private string _rfidTextColor;
        public string RFIDTextColor
        {
            get { return _rfidTextColor; }
            set
            {
                _rfidTextColor = value;
                this.RaisePropertyChanged("RFIDTextColor");
            }
        }

        private string _qrcodeTextColor;
        public string QRCodeTextColor
        {
            get { return _qrcodeTextColor; }
            set
            {
                _qrcodeTextColor = value;
                this.RaisePropertyChanged("QRCodeTextColor");
            }
        }

        private string _qrcodeText;
        public string QRCodeText
        {
            get { return _qrcodeText; }
            set
            {
                _qrcodeText = value;
                this.RaisePropertyChanged("QRCodeText");
            }
        }

        private string _rfidColor;
        public string RFIDColor
        {
            get { return _rfidColor; }
            set
            {
                _rfidColor = value;
                this.RaisePropertyChanged("RFIDColor");
            }
        }

        private string _qrcodeColor;
        public string QRCodeColor
        {
            get { return _qrcodeColor; }
            set
            {
                _qrcodeColor = value;
                this.RaisePropertyChanged("QRCodeColor");
            }
        }

        private Uri _bindingState;
        public Uri BindingState
        {
            get { return _bindingState; }
            set
            {
                _bindingState = value;
                this.RaisePropertyChanged("BindingState");
            }
        }

        private string _qrcode;
        public string QRCode
        {
            get { return _qrcode; }
            set
            {
                _qrcode = value;
                this.RaisePropertyChanged("QRCode");
            }
        }

        private string _rfidTag;
        public string RFIDTag
        {
            get { return _rfidTag; }
            set
            {
                _rfidTag = value;
                this.RaisePropertyChanged("RFIDTag");
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
    }
}
