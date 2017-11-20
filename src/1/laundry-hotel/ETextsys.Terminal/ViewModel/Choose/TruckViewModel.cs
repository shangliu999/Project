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
    public class TruckViewModel : ViewModelBase
    {
        Action _closeAction;

        private TruckModalModel model;
        public TruckModalModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        public TruckViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new TruckModalModel();
        }

        public void Modal_ContentRendered(object sender, EventArgs e)
        {
            var query = from t in this.model.TagList
                        where t.TruckTagNo != "" && t.TruckTagNo != null
                        group t by new { t.TruckTagNo } into m
                        select new
                        {
                            TruckTagNo = m.Key.TruckTagNo,
                            TextileCount = m.Count()
                        };

            TruckTableModel model = null;
            this.model.TruckTable = new System.Collections.ObjectModel.ObservableCollection<Terminal.Model.Choose.TruckTableModel>();
            query.ToList().ForEach(q =>
            {
                model = new TruckTableModel();
                model.TruckRFID = q.TruckTagNo;
                model.TextileCount = q.TextileCount;

                this.model.TruckTable.Add(model);
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
