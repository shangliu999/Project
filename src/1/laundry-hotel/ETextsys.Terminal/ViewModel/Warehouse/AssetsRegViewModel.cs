﻿using ETexsys.APIRequestModel;
using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using ETextsys.Terminal.ViewModel.Choose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class AssetsRegViewModel : ViewModelBase, IRFIDScan
    {
        private AssetsRegModel model;
        public AssetsRegModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private List<string> BagList { get; set; }
        private List<string> TruckList { get; set; }

        #region 界面待选数据

        private ObservableCollection<ChooseModel> AssetsTypeList;

        #endregion

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private DateTime ResetTime;

        public AssetsRegViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new AssetsRegModel();
            this.model.UnRegTable = new ObservableCollection<UnRegTableModel>();
            this.model.RegTable = new ObservableCollection<RegTableModel>();
            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;
            this.model.WaitVisibled = Visibility.Hidden;
            ResetTime = DateTime.Now;

            _isSubmit = false;

            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                this.model.ReaderState = ConfigController.ReaderConfig.IsConnection ? 2 : 0;
                if (ConfigController.ReaderConfig.IsConnection)
                {
                    ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                    {
                        model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                        model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                    });
                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                }
            }
            AssetsTypeList = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();
            AssetsTypeList.Add(new ChooseModel { ChooseID = 1, ChooseName = "包" });
            AssetsTypeList.Add(new ChooseModel { ChooseID = 2, ChooseName = "架子车" });

            this.model.AssetsType = 1;
            BagList = new List<string>();
            TruckList = new List<string>();
        }


        #region RFID Interface

        public void ScanNew(List<TagModel> rfidTags)
        {
            var bTags = rfidTags.Where(v => v.Type == TagType.Bag).Select(v => v.TagNo).ToList();
            BagList.AddRange(bTags);
            var fTags = rfidTags.Where(v => v.Type == TagType.Truck).Select(v => v.TagNo).ToList();
            TruckList.AddRange(fTags);

            Refresh();
        }

        public void NoScanTag()
        {
            if (model.UnRegTable.Count > 0 && !_isSubmit)
            {
                this.model.SubmitEnabled = true;
            }
        }

        private async void Refresh()
        {
            RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();

            if (model.AssetsType == 1)
            {
                if (BagList.Count == 0) { return; }
                this.model.SubmitEnabled = false;
                this.model.CancelEnabled = true;

                requestParam.TagList = BagList.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtnb = await ApiController.Instance.DoPost(ApiController.Instance.BagRFIDTagAnalysis, requestParam);
                if (apiRtnb.ResultCode == 0)
                {
                    List<ResponseBagRFIDTagModel> temp = null;
                    if (apiRtnb.Result != null && apiRtnb.OtherResult != null)
                    {
                        DateTime d = Convert.ToDateTime(apiRtnb.OtherResult);
                        if (d > ResetTime)
                        {
                            temp = JsonConvert.DeserializeObject<List<ResponseBagRFIDTagModel>>(apiRtnb.Result.ToString());
                        }
                    }

                    model.UnRegTable.Clear();
                    for (int i = 0; i < requestParam.TagList.Length; i++)
                    {
                        if (temp == null || temp.Where(v => v.BagRFIDTagNo == requestParam.TagList[i]).Count() == 0)
                        {
                            string t = requestParam.TagList[i];
                            model.UnRegTable.Add(new UnRegTableModel { RFID = t, RFIDWashingTime = 0 });
                        }
                    }

                    if (temp != null && temp.Count > 0)
                    {
                        RegTableModel regModel = new RegTableModel();
                        regModel.ClassName = "包";
                        regModel.TextileCount = temp.Count;
                        this.model.RegTable.Clear();
                        this.model.RegTable.Add(regModel);
                        this.model.TextileCount = temp.Count;
                    }

                    this.model.UnRegisterTotal = model.UnRegTable.Count;
                }

            }
            else if (model.AssetsType == 2)
            {
                if (TruckList.Count == 0)
                {
                    this.model.UnRegTable = new ObservableCollection<UnRegTableModel>();
                    return;
                }
                this.model.SubmitEnabled = false;
                this.model.CancelEnabled = true;

                requestParam.TagList = TruckList.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtnb = await ApiController.Instance.DoPost(ApiController.Instance.TruckRFIDTagAnalysis, requestParam);
                if (apiRtnb.ResultCode == 0)
                {
                    List<ResponseTruckRFIDTagModel> temp = null;
                    if (apiRtnb.Result != null && apiRtnb.OtherResult != null)
                    {
                        DateTime d = Convert.ToDateTime(apiRtnb.OtherResult);
                        if (d > ResetTime)
                        {
                            temp = JsonConvert.DeserializeObject<List<ResponseTruckRFIDTagModel>>(apiRtnb.Result.ToString());
                        }
                    }

                    model.UnRegTable.Clear();
                    for (int i = 0; i < requestParam.TagList.Length; i++)
                    {
                        if (temp == null || temp.Where(v => v.TrunckRFIDTagNo == requestParam.TagList[i]).Count() == 0)
                        {
                            string t = requestParam.TagList[i];
                            model.UnRegTable.Add(new UnRegTableModel { RFID = t, RFIDWashingTime = 0 });
                        }
                    }

                    if (temp != null && temp.Count > 0)
                    {
                        RegTableModel regModel = new RegTableModel();
                        regModel.ClassName = "架子车";
                        regModel.TextileCount = temp.Count;
                        this.model.RegTable.Clear();
                        this.model.RegTable.Add(regModel);
                        this.model.TextileCount = temp.Count;
                    }
                    this.model.UnRegisterTotal = model.UnRegTable.Count;

                }
            }
            else { }
        }

        #endregion

        #region Action

        private void MediaEndedAction(MediaElement sender)
        {
            MediaElement media = (MediaElement)sender;
            media.Position = TimeSpan.FromMilliseconds(1);
            media.Play();
        }

        private void ReaderStateAction(Border sender)
        {
            if (ReaderController.Instance.ScanUtilities == null) { return; }
            Border border = (Border)sender;
            int state = Convert.ToInt32(border.Tag);
            switch (state)
            {
                case 0:
                    if (ConfigController.ReaderConfig != null)
                    {
                        ConfigController.ReaderConfig.IsConnection = ReaderController.Instance.Reader.Connect();
                        if (ConfigController.ReaderConfig.IsConnection)
                        {
                            ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                            {
                                model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                                model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                            });
                        }
                    }
                    break;
                case 1:
                    ReaderController.Instance.ScanUtilities.StopScan();
                    System.Threading.Thread.Sleep(50);
                    this.model.ReaderState = 2;
                    this.model.ReaderLight = new Uri("../../Skins/Default/Images/stopscan.gif", UriKind.RelativeOrAbsolute);
                    break;
                case 2:
                    ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                    {
                        model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                        model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                    });
                    break;
            }
        }

        private void ChooseAttrAction(string sender)
        {
            ObservableCollection<ChooseModel> list = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();
            string title = "";
            switch (sender)
            {
                case "AssetsType":
                    list = AssetsTypeList;
                    title = "请选择类型:";
                    break;
                default:
                    return;
            }


            DownModal modal = new View.Choose.DownModal();
            DownModalViewModel modalModel = new DownModalViewModel(modal.Close);
            modalModel.Model.ChooseList = list;
            modalModel.Model.Title = title;
            modal.DataContext = modalModel;
            modal.ShowDialog();
            if (modalModel.ChooseItem != null)
            {
                switch (sender)
                {
                    case "AssetsType":
                        this.model.AssetsType = modalModel.ChooseItem.ChooseID;
                        this.model.AssetsTypeName = modalModel.ChooseItem.ChooseName;
                        Refresh();
                        break;
                }
            }
        }

        private void CloseModalAction(string sender)
        {
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.StopScan();
            }

            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private async void SubmitAction(object sender)
        {
            if (this.model.AssetsType == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择类型.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.model.UnRegTable.Count == 0)
            {
                string msg = this.model.AssetsType == 1 ? "请扫描包" : "请扫描架子车";
                EtexsysMessageBox.Show("提示", msg, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(this.model.Code))
            {
                EtexsysMessageBox.Show("提示", "请输入编码。", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.model.UnRegTable.Count != 1)
            {
                EtexsysMessageBox.Show("提示", "一次只能绑定一个。", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //if (!_bw.IsBusy)
            //{
            _isSubmit = true;
            this.model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitContent = "正在玩命提交.";
            //    _bw.RunWorkerAsync();
            //}
            ResetTime = DateTime.Now;
            AssetsRegParamModel request = new AssetsRegParamModel();
            request.AssetsType = this.model.AssetsType;
            request.UserID = App.CurrentLoginUser.UserID;
            request.Tags = this.model.UnRegTable.Select(v => v.RFID).ToArray();
            request.TimeStamp = ApiController.Instance.GetTimeStamp();
            request.UUID = ConfigController.MacCode;
            request.TerminalType = ConfigController.TerminalType;
            request.Code = this.model.Code;

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.AssetsReg, request);

            this.model.WaitVisibled = Visibility.Hidden;
            if (rtn.ResultCode == 1)
            {
                EtexsysMessageBox.Show("提示", rtn.ResultMsg, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                EtexsysMessageBox.Show("提示", "登记成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelAction(object sender)
        {
            if (ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            ResetTime = DateTime.Now;
            TruckList.Clear();
            BagList.Clear();
            this.model.SubmitEnabled = false;
            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.UnRegTable.Clear();
            this.model.RegTable.Clear();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitVisibled = Visibility.Hidden;
            this.model.WaitContent = "刷新读写器.";
            System.Threading.Thread.Sleep(50);
            this.model.WaitVisibled = Visibility.Visible;
            ReaderController.Instance.ScanUtilities.StartScan(this, () =>
            {
                this.model.WaitVisibled = Visibility.Hidden;
                this.model.WaitContent = "正在玩命提交.";
                model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
            }, () =>
            {
                this.model.WaitVisibled = Visibility.Hidden;
                this.model.WaitContent = "正在玩命提交.";
            });
            if (_isSubmit)
            {
                _isSubmit = false; 
            }
        }

        private void TabAction(string sender)
        {
            if (sender == "Bag")
            {
                this.model.AssetsType = 1;
            }
            else
            {
                this.model.AssetsType = 2;
            }
            Refresh();
        }

        #endregion

        #region Command

        private RelayCommand<MediaElement> _MediaEndedCommand;
        public RelayCommand<MediaElement> MediaEndedCommand
        {
            get
            {
                if (_MediaEndedCommand == null)
                {
                    _MediaEndedCommand = new RelayCommand<MediaElement>(MediaEndedAction);
                }
                return _MediaEndedCommand;
            }
        }

        private RelayCommand<Border> _ReaderStateChanged;
        public RelayCommand<Border> ReaderStateChanged
        {
            get
            {
                if (_ReaderStateChanged == null)
                {
                    _ReaderStateChanged = new RelayCommand<Border>(ReaderStateAction);
                }
                return _ReaderStateChanged;
            }
        }

        private RelayCommand<string> _ChooseAttrChanged;
        public ICommand ChooseAttrChanged
        {
            get
            {
                if (_ChooseAttrChanged == null)
                {
                    _ChooseAttrChanged = new RelayCommand<string>(ChooseAttrAction);
                }
                return _ChooseAttrChanged;
            }
        }

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

        private RelayCommand<object> _SubmitCommand;
        public ICommand SubmitCommand
        {
            get
            {
                if (_SubmitCommand == null)
                {
                    _SubmitCommand = new RelayCommand<object>(SubmitAction);
                }
                return _SubmitCommand;
            }
        }

        private RelayCommand<object> _CancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand<object>(CancelAction);
                }
                return _CancelCommand;
            }
        }

        private RelayCommand<string> _TabCommand;
        public ICommand TabCommand
        {
            get
            {
                if (_TabCommand == null)
                {
                    _TabCommand = new RelayCommand<string>(TabAction);
                }
                return _TabCommand;
            }
        }

        #endregion
    }
}
