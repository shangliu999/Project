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
    public class ChooseHotelViewModel : ViewModelBase
    {
        Action _closeAction;

        private ChooseHotelModel model;
        public ChooseHotelModel Model
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

        /// <summary>
        /// 一页显示条数
        /// </summary>
        private int PageCount = 20;

        /// <summary>
        /// 当前页码
        /// </summary>
        private int CurrentPageIndex = 1;

        /// <summary>
        /// 总条数
        /// </summary>
        private int PageTotal = 0;

        /// <summary>
        /// 总页码
        /// </summary>
        private int PageIndexTotal = 0;

        public ChooseHotelViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new ChooseHotelModel();
            this.model.ChooseList = new System.Collections.ObjectModel.ObservableCollection<Terminal.Model.Choose.ChooseModel>();
        }

        public void RefreshPage()
        {
            PageTotal = this.model.AllChooseList.Count;

            PageIndexTotal = PageTotal % PageCount == 0 ? PageTotal / PageCount : (PageTotal / PageCount) + 1;

            var query = this.model.AllChooseList.Skip((CurrentPageIndex - 1) * PageCount).Take(PageCount);

            if (CurrentPageIndex < PageIndexTotal)
            {
                this.model.RightEnabled = true;
            }
            else
            {
                this.model.RightEnabled = false;
            }

            if (CurrentPageIndex > 1)
            {
                this.model.LeftEnabled = true;
            }
            else
            {
                this.model.LeftEnabled = false;
            }
            
            this.model.ChooseList.Clear();
            ChooseModel chooseModel = null;
            query.ToList().ForEach(q =>
            {
                chooseModel = new ChooseModel();
                chooseModel.ChooseID = q.ChooseID;
                chooseModel.ChooseName = q.ChooseName;
                this.model.ChooseList.Add(chooseModel);
            });
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

        private void LeftAction(object sender)
        {
            CurrentPageIndex--;
            RefreshPage();
        }

        private void RightAction(object sender)
        {
            CurrentPageIndex++;
            RefreshPage();
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

        private RelayCommand<object> _LeftCommand;
        public ICommand LeftCommand
        {
            get
            {
                if (_LeftCommand == null)
                {
                    _LeftCommand = new RelayCommand<object>(LeftAction);
                }
                return _LeftCommand;
            }
        }

        private RelayCommand<object> _RightCommand;
        public ICommand RightCommand
        {
            get
            {
                if (_RightCommand == null)
                {
                    _RightCommand = new RelayCommand<object>(RightAction);
                }
                return _RightCommand;
            }
        }

        #endregion
    }
}
