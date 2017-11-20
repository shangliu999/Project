﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Logistics;
using ETextsys.Terminal.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Choose
{
    public class TaskDetailsViewModel : ViewModelBase
    {
        Action _closeAction;

        private TaskDetailsModel model;
        public TaskDetailsModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private FactoryTableModel _FactoryTableModel;
        public FactoryTableModel FactoryTableModel
        {
            get { return _FactoryTableModel; }
            set
            {
                _FactoryTableModel = value;
                this.RaisePropertyChanged("FactoryTableModel");
            }
        }

        private List<ResponseRFIDTagModel> _TextileList;
        public List<ResponseRFIDTagModel> TextileList
        {
            get { return _TextileList; }
            set
            {
                _TextileList = value;
                this.RaisePropertyChanged("TextileList");
            }
        }

        private List<ResponseDetail> _list;

        public TaskDetailsViewModel(Action closeAction)
        {
            _closeAction = closeAction;
            model = new TaskDetailsModel();
            this.model.TaskDetailsTable = new ObservableCollection<TaskDetailsTableModel>();
            _list = new List<ResponseDetail>();
        }

        public async void Modal_ContentRendered(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.FactoryTableModel.InvNo))
            {
                #region 存在污物送洗单据

                DetailsParamModel detai = new DetailsParamModel();
                detai.OrderNumber = this.FactoryTableModel.InvNo;
                detai.TerminalType = ConfigController.TerminalType;
                detai.TimeStamp = ApiController.Instance.GetTimeStamp();
                detai.UUID = ConfigController.MacCode;
                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.Details, detai);
                if (apiRtn.Result != null)
                {
                    var temp = JsonConvert.DeserializeObject<List<ResponseDetail>>(apiRtn.Result.ToString());
                    if (temp != null)
                    {
                        _list.AddRange(temp);
                    }
                }

                var query = from t in TextileList
                            group t by new { t.ClassName, t.SizeName }
                            into m
                            select new
                            {
                                m.Key.ClassName,
                                m.Key.SizeName,
                                cnt = m.Count(),
                            };

                TaskDetailsTableModel model = null;
                _list.ForEach(q =>
                {
                    model = new TaskDetailsTableModel();

                    model.ClassName = q.ClassName;
                    model.SizeName = q.Size;
                    model.TaskCount = q.Number;

                    if (!string.IsNullOrEmpty(q.Size))
                    {
                        var first = query.FirstOrDefault(c => c.ClassName.Equals(q.ClassName) && !string.IsNullOrEmpty(c.SizeName) && c.SizeName.Equals(q.Size));
                        if (first != null)
                        {
                            model.TextileCount = first.cnt;
                        }
                    }
                    else
                    {
                        var first = query.FirstOrDefault(c => c.ClassName.Equals(q.ClassName));
                        if (first != null)
                            model.TextileCount = first.cnt;
                    }

                    this.model.TaskDetailsTable.Add(model);
                });

                #endregion
            }
            else
            {
                var query = from t in TextileList
                            group t by new { t.ClassName, t.SizeName }
                           into m
                            select new
                            {
                                m.Key.ClassName,
                                m.Key.SizeName,
                                cnt = m.Count(),
                            };

                TaskDetailsTableModel model = null;
                query.ToList().ForEach(q =>
                {
                    model = new TaskDetailsTableModel();
                    model.ClassName = q.ClassName;
                    model.SizeName = q.SizeName;
                    model.TaskCount = 0;
                    model.TextileCount = q.cnt;

                    this.model.TaskDetailsTable.Add(model);
                });
            }
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
