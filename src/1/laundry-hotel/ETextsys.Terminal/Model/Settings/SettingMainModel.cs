using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etextsys.Terminal.Model.Settings
{
    public class SettingMainModel : ViewModelBase
    {
        public SettingMainModel()
        {
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                this.RaisePropertyChanged("Title");
            }
        }

        private int _businessBtnState;
        public int BusinessBtnState
        {
            get { return _businessBtnState; }
            set
            {
                _businessBtnState = value;
                this.RaisePropertyChanged("BusinessBtnState");
            }
        }

        private int _bgBtnState;
        public int BgBtnState
        {
            get { return _bgBtnState; }
            set
            {
                _bgBtnState = value;
                this.RaisePropertyChanged("BgBtnState");
            }
        }

        private int _readerBtnState;
        public int ReaderBtnState
        {
            get { return _readerBtnState; }
            set
            {
                _readerBtnState = value;
                this.RaisePropertyChanged("ReaderBtnState");
            }
        }

        private int _systemBtnState;
        public int SystemBtnState
        {
            get { return _systemBtnState; }
            set
            {
                _systemBtnState = value;
                this.RaisePropertyChanged("SystemBtnState");
            }
        }
    }

}
