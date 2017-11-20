using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Choose
{
    public class DownModalViewModel : ViewModelBase
    {
        Action _closeAction;

        private DownModalModel model;
        public DownModalModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private ChooseModel _chooseItem;
        public ChooseModel ChooseItem
        {
            get { return _chooseItem; }
            set
            {
                _chooseItem = value;
            }
        }

        public DownModalViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new DownModalModel();
        }

        #region Action

        private void CloseModalAction(string sender)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void SelectedItemAction(object sender)
        {
            Button btn = sender as Button;
            _chooseItem = new ChooseModel();
            _chooseItem.ChooseID = Convert.ToInt32(btn.Tag);
            _chooseItem.ChooseName = btn.Content.ToString();

            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        #endregion

        #region Command


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

        private RelayCommand<object> _SelectionChangedCmd;
        public ICommand SelectionChangedCmd
        {
            get
            {
                if (_SelectionChangedCmd == null)
                {
                    _SelectionChangedCmd = new RelayCommand<object>(SelectedItemAction);
                }
                return _SelectionChangedCmd;
            }
        }

        #endregion
    }
}
