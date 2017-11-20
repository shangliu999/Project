using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.QuerySummary;
using ETextsys.Terminal.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.QuerySummary
{
    public class TextileDetailViewModel : ViewModelBase
    {
        /// <summary>
        /// 获取选择数据线程
        /// </summary>
        private BackgroundWorker _worker;

        private TextileDetailModel model;
        public TextileDetailModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private List<ResponseTextileFlowModel> ReceiveSendList;
        private List<ResponseTextileFlowModel> AllList;

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        public TextileDetailViewModel(Action closeAction, TextileSingleTableModel obj)
        {
            this._closeAction = closeAction;
            this.model = new TextileDetailModel();
            this.model.FlowTable = new ObservableCollection<TextileFlowTableModel>();
            this.model.SingleBtnChoosed = true;
            this.model.AreaBtnChoosed = false;
            this.model.TextileSingleVisibiled = Visibility.Visible;
            this.model.TextileAreaVisibiled = Visibility.Collapsed;

            ReceiveSendList = new List<ResponseTextileFlowModel>();
            AllList = new List<ResponseTextileFlowModel>();

            this.model.ClassName = obj.ClassName;
            this.model.SizeName = obj.SizeName;
            this.model.CostTime = obj.CostTime;
            this.model.TextileWashtime = obj.TextileWashtime;
            this.model.RFIDTagNo = obj.RFIDTagNo;
            this.model.RFIDWashtime = obj.RFIDWashtime;
            this.model.TextileId = obj.TextileId;

            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ReceiveSendList.ForEach(q =>
            {
                this.model.FlowTable.Add(new TextileFlowTableModel { FlowName = q.FlowName, OperationTime = q.OperationTime, OperationUser = q.OperationUser, PositionName = q.PositionName });
            });
        }

        private async void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            TextileFlowParamModel requestParam = new TextileFlowParamModel { TextileId = this.model.TextileId, Type = 1 };
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;

            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.TextileFlow, requestParam);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var flow = JsonConvert.DeserializeObject<List<ResponseTextileFlowModel>>(apiRtn.Result.ToString());
                    if (flow != null)
                    {
                        ReceiveSendList = flow;
                    }
                }
            }

            requestParam = new TextileFlowParamModel { TextileId = this.model.TextileId, Type = 2 };
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;

            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.TextileFlow, requestParam);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var flow = JsonConvert.DeserializeObject<List<ResponseTextileFlowModel>>(apiRtn.Result.ToString());
                    if (flow != null)
                    {
                        AllList = flow;
                    }
                }
            }

        }

        #region Action


        private void CloseModalAction(string sender)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void SingleAction(object sender)
        {
            this.model.SingleBtnChoosed = true;
            this.model.AreaBtnChoosed = false;
            this.model.TextileSingleVisibiled = Visibility.Visible;
            this.model.TextileAreaVisibiled = Visibility.Collapsed;
            this.model.FlowTable = new ObservableCollection<TextileFlowTableModel>();
            ReceiveSendList.ForEach(q =>
            {
                this.model.FlowTable.Add(new TextileFlowTableModel { FlowName = q.FlowName, OperationTime = q.OperationTime, OperationUser = q.OperationUser, PositionName = q.PositionName });
            });
        }

        private void AreaAction(object sender)
        {
            this.model.SingleBtnChoosed = false;
            this.model.AreaBtnChoosed = true;
            this.model.TextileSingleVisibiled = Visibility.Collapsed;
            this.model.TextileAreaVisibiled = Visibility.Visible;
            this.model.FlowTable = new ObservableCollection<TextileFlowTableModel>();
            AllList.ForEach(q =>
            {
                this.model.FlowTable.Add(new TextileFlowTableModel { FlowName = q.FlowName, OperationTime = q.OperationTime, OperationUser = q.OperationUser, PositionName = q.PositionName });
            });
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


        private RelayCommand<object> _AreaCommand;
        public ICommand AreaCommand
        {
            get
            {
                if (_AreaCommand == null)
                {
                    _AreaCommand = new RelayCommand<object>(AreaAction);
                }
                return _AreaCommand;
            }
        }

        private RelayCommand<object> _SingleCommand;
        public ICommand SingleCommand
        {
            get
            {
                if (_SingleCommand == null)
                {
                    _SingleCommand = new RelayCommand<object>(SingleAction);
                }
                return _SingleCommand;
            }
        }


        #endregion
    }
}
