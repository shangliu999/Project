﻿using ETexsys.APIRequestModel.Request;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class ScrapViewModel : ViewModelBase, IRFIDScan
    {
        private ScrapModel model;
        public ScrapModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        /// <summary>
        /// 获取选择数据线程
        /// </summary>
        private BackgroundWorker _worker;

        /// <summary>
        /// 界面数据是否准备好
        /// </summary>
        private bool Prepare;

        #region 界面待选数据

        private ObservableCollection<ChooseModel> HotelList;
        private ObservableCollection<ChooseModel> ScrapCauseList;
        private ObservableCollection<ChooseModel> TypeList;

        #endregion

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private List<string> ScanList;

        private List<ResponseRFIDTagModel> TextileList;

        private DateTime ResetTime;

        public ScrapViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new ScrapModel();
            this.model.ScrapTable = new ObservableCollection<ScrapTableModel>();
            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;
            this.model.WaitVisibled = Visibility.Hidden;
            this.model.HotelVisibled = Visibility.Hidden;

            ScanList = new List<string>();
            TextileList = new List<ResponseRFIDTagModel>();
            _isSubmit = false;
            ResetTime = DateTime.Now;

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
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private async void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Prepare = false;

            HotelList = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();

            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.HotelList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var hotel = JsonConvert.DeserializeObject<List<ResponseHotelModel>>(apiRtn.Result.ToString());
                    if (hotel != null)
                    {
                        hotel.OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.HotelName).ToList().ForEach(q =>
                       {
                           HotelList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.HotelName });
                       });
                    }
                }
            }

            TypeList = new ObservableCollection<ChooseModel>();
            TypeList.Add(new ChooseModel { ChooseID = 1, ChooseName = "工厂" });
            TypeList.Add(new ChooseModel { ChooseID = 2, ChooseName = "酒店" });

            ScrapCauseList = new ObservableCollection<ChooseModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.ScrapList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var scrap = JsonConvert.DeserializeObject<List<ResponseScrapModel>>(apiRtn.Result.ToString());
                    if (scrap != null)
                    {
                        scrap.OrderBy(v => v.ScrapName).ToList().ForEach(q =>
                        {
                            ScrapCauseList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.ScrapName });
                        });
                    }
                }
            }

            Prepare = true;

        }
        #region RFID Interface

        public void NoScanTag()
        {
            if (model.ScrapTable.Count > 0 && !_isSubmit)
            {
                this.model.SubmitEnabled = true;
            }
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            if (tags.Count == 0)
            {
                return;
            }
            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;

            RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
            requestParam.TagList = tags.ToArray();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.RequestTime = DateTime.Now;

            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RFIDTagAnalysis, requestParam);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null && apiRtn.OtherResult != null)
                {
                    DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                    var temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                    if (temp != null && d > ResetTime)
                    {
                        ScanList.AddRange(tags);
                        TextileList.AddRange(temp);
                    }
                }

                ScrapTableModel receiveModel = null;
                var query = from t in TextileList
                            select new
                            {
                                brandName = t.BrandName,
                                className = t.ClassName,
                                sizeName = t.SizeName,
                                hotelName = t.HotelName,
                                washtimes = t.Washtimes,
                                RFIDTagNo = t.TagNo,
                            };

                int total = 0;
                this.model.ScrapTable.Clear();
                query.ToList().ForEach(q =>
                {
                    receiveModel = new ScrapTableModel();
                    receiveModel.BrandName = q.brandName;
                    receiveModel.ClassName = q.className;
                    receiveModel.SizeName = q.sizeName;
                    receiveModel.HotelName = q.hotelName;
                    receiveModel.TextileWashtime = q.washtimes;
                    receiveModel.RFIDTagNo = q.RFIDTagNo;
                    total += 1;
                    this.model.ScrapTable.Add(receiveModel);
                });

                this.model.UnRegisterTotal = ScanList.Count - total;
                this.model.TextileCount = total;

            }

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
            if (!Prepare)
            {
                EtexsysMessageBox.Show("提示", "数据正在加载，请稍等...", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ObservableCollection<ChooseModel> list = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();
            string title = "";
            switch (sender)
            {
                case "Hotel":
                    list = HotelList;
                    title = "请选择酒店:";
                    break;
                case "Responsible":
                    list = TypeList;
                    title = "请选择责任方";
                    break;
                case "Cause":
                    list = ScrapCauseList;
                    title = "请选择报废原因:";
                    break;
                default:
                    return;
            }

            if (sender == "Hotel")
            {
                ChooseHotel hotel = new View.Choose.ChooseHotel();
                ChooseHotelViewModel hotelViewModal = new Choose.ChooseHotelViewModel(hotel.Close);
                hotelViewModal.Model.AllChooseList = list;
                hotelViewModal.Model.Title = title;
                hotelViewModal.RefreshPage();
                hotel.DataContext = hotelViewModal;
                hotel.ShowDialog();
                if (hotelViewModal.ChooseItem != null)
                {
                    this.model.ResponsibleID = hotelViewModal.ChooseItem.ChooseID;
                    this.model.ResponsibleName = hotelViewModal.ChooseItem.ChooseName;
                }
            }
            else
            {
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
                        case "Hotel":

                            break;
                        case "Responsible":
                            this.model.ResponsibleType = modalModel.ChooseItem.ChooseID;
                            this.model.ResponsibleTypeName = modalModel.ChooseItem.ChooseName;

                            this.model.ResponsibleID = 0;
                            this.model.ResponsibleName = "";

                            if (this.model.ResponsibleType == 1)
                            {
                                this.model.HotelVisibled = Visibility.Hidden;
                            }
                            else
                            {
                                this.model.HotelVisibled = Visibility.Visible;
                            }
                            break;
                        case "Cause":
                            this.model.ScrapID = Convert.ToInt16(modalModel.ChooseItem.ChooseID);
                            this.model.ScrapName = modalModel.ChooseItem.ChooseName;
                            break;
                    }
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
            if (this.model.ScrapID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择报废原因.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.ResponsibleType == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择责任方.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.ResponsibleType == 2 && this.model.ResponsibleID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择酒店.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.ScrapTable.Count == 0)
            {
                EtexsysMessageBox.Show("提示", "请扫描纺织品.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            _isSubmit = true;
            this.model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitContent = "正在玩命提交.";
            ResetTime = DateTime.Now;

            ScrapParamModel request = new ScrapParamModel();
            request.TimeStamp = ApiController.Instance.GetTimeStamp();
            request.UUID = ConfigController.MacCode;
            request.TerminalType = ConfigController.TerminalType;
            request.CreateUserID = App.CurrentLoginUser.UserID;
            request.CreateUserName = App.CurrentLoginUser.UName;
            request.ResponsibleType = this.model.ResponsibleType;
            request.HotelID = this.model.ResponsibleID;
            request.HotelName = this.model.ResponsibleName;
            request.ScrapID = this.model.ScrapID;
            request.ScrapName = this.model.ScrapName;
            request.Tags = ScanList.ToArray();

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.TextileScrap, request);

            this.model.WaitVisibled = Visibility.Hidden;
            if (rtn.ResultCode == 1)
            {
                EtexsysMessageBox.Show("提示", rtn.ResultMsg, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                EtexsysMessageBox.Show("提示", "报废成功.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelAction(object sender)
        {
            if (ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            ResetTime = DateTime.Now;
            TextileList.Clear();
            this.model.SubmitEnabled = false;
            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.ScrapTable.Clear();
            ScanList.Clear();
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



        #endregion
    }
}
