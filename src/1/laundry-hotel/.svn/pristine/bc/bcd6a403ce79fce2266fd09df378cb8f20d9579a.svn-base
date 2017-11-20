using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Choose
{
    public class ExecutedViewModel : ViewModelBase
    {
        Action _closeAction;

        private ExecutedModalModel model;
        public ExecutedModalModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        public ExecutedViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new ExecutedModalModel();
        }

        public void Modal_ContentRendered(object sender, EventArgs e)
        {
            var query = from t in this.model.TagList
                        select new
                        {
                            BrandName = t.BrandName,
                            ClassName = t.ClassName,
                            SizeName = t.SizeName,
                            RFID = t.TagNo,
                            HotelName = t.HotelName,
                            UpdateTime = t.UpdateTime
                        };

            ExecutedTableModel model = null;
            this.model.ExecutedTable = new System.Collections.ObjectModel.ObservableCollection<Terminal.Model.Choose.ExecutedTableModel>();
            query.ToList().ForEach(q =>
            {
                model = new ExecutedTableModel();
                model.RFID = q.RFID;
                model.BrandName = q.BrandName;
                model.ClassName = q.ClassName;
                model.SizeName = q.SizeName;
                model.UpdateTime = q.UpdateTime.Value.ToString("yyyy/MM/dd HH:mm");
                model.RegionName = q.HotelName;

                this.model.ExecutedTable.Add(model);
            });
        }

        #region Action

        private void CloseModalAction(string sender)
        {
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

        #endregion
    }
}
