using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Model.Settings
{
    public class BusinessSettingModel : ViewModelBase
    {
        private int _sendPrintCount;
        public int SendPrintCount
        {
            get { return _sendPrintCount; }
            set
            {
                _sendPrintCount = value;
                this.RaisePropertyChanged("SendPrintCount");
            }
        }

        private int _reveicePrintCount;
        public int ReveicePrintCount
        {
            get { return _reveicePrintCount; }
            set
            {
                _reveicePrintCount = value;
                this.RaisePropertyChanged("ReveicePrintCount");
            }
        }

        private int _otherPrintCount;
        public int OtherPrintCount
        {
            get { return _otherPrintCount; }
            set
            {
                _otherPrintCount = value;
                this.RaisePropertyChanged("OtherPrintCount");
            }
        }
    }
}
