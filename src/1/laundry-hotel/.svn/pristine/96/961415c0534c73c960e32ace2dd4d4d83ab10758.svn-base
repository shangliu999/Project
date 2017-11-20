using Etextsys.Terminal.Domain;
using Etextsys.Terminal.Model.Settings;
using ETextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Settings
{
    public class SettingMainViewModel : ViewModelBase
    {
        Action _closeAction;

        private SettingMainModel model;
        public SettingMainModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        public SettingMainViewModel(Action action)
        {
            this._closeAction = action;
            this.model = new SettingMainModel();
            this.model.BusinessBtnState = 1;
            this.model.Title = "业务设置";
        }

        #region Action

        private void CloseModalAction(string sender)
        {
            if (ConfigController.ReaderConfig != null)
            {
                if (ReaderController.Instance.ScanUtilities != null)
                {
                    ReaderController.Instance.ScanUtilities.StopScan();
                }
            }

            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void MenuAction(string sender)
        {
            switch (sender)
            {
                case "Business":
                    this.model.BgBtnState = 0;
                    this.model.ReaderBtnState = 0;
                    this.model.SystemBtnState = 0;
                    this.model.BusinessBtnState = 1;
                    this.model.Title = "业务设置";
                    break;
                case "BG":
                    this.model.ReaderBtnState = 0;
                    this.model.SystemBtnState = 0;
                    this.model.BusinessBtnState = 0;
                    this.model.BgBtnState = 1;
                    this.model.Title = "背景设置";
                    break;
                case "Reader":
                    this.model.BgBtnState = 0;
                    this.model.SystemBtnState = 0;
                    this.model.BusinessBtnState = 0;
                    this.model.ReaderBtnState = 1;
                    this.model.Title = "读写器设置";
                    break;
                case "System":
                    this.model.BgBtnState = 0;
                    this.model.ReaderBtnState = 0;
                    this.model.BusinessBtnState = 0;
                    this.model.SystemBtnState = 1;
                    this.model.Title = "系统设置";
                    break;
            }
        }

        #endregion

        #region commond

        private RelayCommand<string> _CloseModal;
        public ICommand CloseModal
        {
            get
            {
                if (_CloseModal == null)
                {
                    _CloseModal = new RelayCommand<string>(CloseModalAction);
                }
                return _CloseModal;
            }
        }

        private RelayCommand<string> _MenuCommand;
        public ICommand MenuCommand
        {
            get
            {
                if (_MenuCommand == null)
                {
                    _MenuCommand = new RelayCommand<string>(MenuAction);
                }
                return _MenuCommand;
            }
        }

        #endregion
    }
}
