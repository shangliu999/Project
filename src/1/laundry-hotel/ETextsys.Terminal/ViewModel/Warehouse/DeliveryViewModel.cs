﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.Utilities.PrintBase;
using ETextsys.Terminal.View.Choose;
using ETextsys.Terminal.ViewModel.Choose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class DeliveryViewModel : ViewModelBase, IRFIDScan
    {
        private DeliveryModel model;
        public DeliveryModel Model
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

        private DateTime ResetTime;


        #region 界面待选数据

        private ObservableCollection<ChooseModel> HouseList;
        private ObservableCollection<ChooseModel> TypeList;

        #endregion

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private List<ResponseRFIDTagModel> TextileList;

        private string InvNo { get; set; }

        private string CreateTime { get; set; }

        private List<string> ScanList;

        private string GUID;

        /// <summary>
        /// 已出库芯片码
        /// </summary>
        private List<ResponseRFIDTagModel> DeliveryedTagList;

        private List<ResponseDeliveryTaskModel> TaskList;

        private ObservableCollection<ChooseDeliveryItemModel> ChooseTaskList;

        private int HotelID;

        public DeliveryViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new DeliveryModel();
            this.model.DeliveryTable = new ObservableCollection<DeliveryTableModel>();
            this.model.SubmitEnabled = false;
            this.model.PrintEnabled = false;
            this.model.CancelEnabled = true;
            this.model.WaitVisibled = Visibility.Hidden;
            this.model.TaskVisibility = Visibility.Hidden;
            TextileList = new List<ResponseRFIDTagModel>();
            _isSubmit = false;
            ScanList = new List<string>();
            DeliveryedTagList = new List<ResponseRFIDTagModel>();
            TaskList = new List<ResponseDeliveryTaskModel>();
            ChooseTaskList = new ObservableCollection<ChooseDeliveryItemModel>();
            GUID = Guid.NewGuid().ToString();
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

            HouseList = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();


            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.StoreList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var store = JsonConvert.DeserializeObject<List<ResponseStoreModel>>(apiRtn.Result.ToString());
                    if (store != null)
                    {
                        store.Where(v => v.StoreType == 1 || v.StoreType == 2).OrderBy(v => v.Sort).ThenBy(v => v.StoreName).ToList().ForEach(q =>
                        {
                            HouseList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.StoreName });
                        });
                    }
                }
            }

            TypeList = new ObservableCollection<ChooseModel>();
            TypeList.Add(new ChooseModel { ChooseID = 1, ChooseName = "需求单" });
            TypeList.Add(new ChooseModel { ChooseID = 2, ChooseName = "报废补充" });
            TypeList.Add(new ChooseModel { ChooseID = 3, ChooseName = "纺织品更换" });


            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.DeliveryTask, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var temp = JsonConvert.DeserializeObject<List<ResponseDeliveryTaskModel>>(apiRtn.Result.ToString());
                    if (temp != null)
                    {
                        TaskList.AddRange(temp);
                    }
                }
            }

            ChooseDeliveryItemModel itemModel = null;
            foreach (var item in TaskList)
            {
                itemModel = new ChooseDeliveryItemModel();
                itemModel.HotelName = item.HotelName;
                itemModel.HotelID = item.HotelID;
                itemModel.No = item.DeliveryNo;
                itemModel.TaskCount = item.DeliveryCount;
                ChooseTaskList.Add(itemModel);
            }

            Prepare = true;
        }

        #region RFID Interface

        public void NoScanTag()
        {
            if (model.DeliveryTable.Count > 0 && !_isSubmit)
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

                var query = from t in TextileList
                            where t.TextileState == 0
                            group t by new
                            {
                                t1 = t.BrandSort,
                                t2 = t.BrandName,
                                t3 = t.ClassSort,
                                t4 = t.ClassName,
                                t5 = t.SizeSort,
                                t6 = t.SizeName
                            } into m
                            orderby m.Key.t1, m.Key.t2, m.Key.t3, m.Key.t4, m.Key.t5, m.Key.t6
                            select new
                            {
                                brandName = m.Key.t2,
                                className = m.Key.t4,
                                sizeName = m.Key.t6,
                                count = m.Count()
                            };

                int total = 0;

                query.ToList().ForEach(q =>
                {
                    DeliveryTableModel row = null;
                    if (string.IsNullOrEmpty(q.sizeName))
                    {
                        row = this.model.DeliveryTable.Where(v => v.BrandName == q.brandName && v.ClassName == q.className).FirstOrDefault();
                    }
                    else
                    {
                        row = this.model.DeliveryTable.Where(v => v.BrandName == q.brandName && v.ClassName == q.className && v.SizeName == q.sizeName).FirstOrDefault();
                    }

                    if (row == null)
                    {
                        DeliveryTableModel rowModel = new DeliveryTableModel();
                        rowModel.BrandName = q.brandName;
                        rowModel.ClassName = q.className;
                        rowModel.SizeName = q.sizeName;
                        rowModel.TaskCount = 0;
                        rowModel.TextileCount = q.count;
                        rowModel.DiffCount = rowModel.TextileCount - rowModel.TaskCount;
                        this.model.DeliveryTable.Add(rowModel);
                    }
                    else
                    {
                        row.TextileCount = q.count;
                        row.DiffCount = row.TextileCount - row.TaskCount;
                    }
                    total += q.count;
                });

                DeliveryedTagList = TextileList.Where(v => v.TextileState != 0).ToList();
                this.model.DeliveryedTotal = DeliveryedTagList.Count;
                this.model.BtnDeliveryedEnabled = DeliveryedTagList.Count > 0 ? true : false;


                this.model.UnRegisterTotal = ScanList.Count - total - model.DeliveryedTotal;
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
                case "House":
                    list = HouseList;
                    title = "请选择仓库:";
                    break;
                case "DeliveryType":
                    list = TypeList;
                    title = "请选择类型:";
                    break;
                case "Task":
                    title = "请选择出库任务";
                    break;
                default:
                    return;
            }

            if (sender == "Task")
            {
                ChooseDelivery hotel = new ChooseDelivery();
                ChooseDeliveryViewModel hotelViewModal = new ChooseDeliveryViewModel(hotel.Close);
                hotelViewModal.Model.AllChooseList = ChooseTaskList;
                hotelViewModal.Model.Title = title;
                hotelViewModal.RefreshPage();
                hotel.DataContext = hotelViewModal;
                hotel.ShowDialog();
                if (hotelViewModal.ChooseItem != null)
                {
                    this.model.TaskNo = hotelViewModal.ChooseItem.No;
                    HotelID = hotelViewModal.ChooseItem.HotelID;
                    var currentCase = TaskList.Where(v => v.DeliveryNo == hotelViewModal.ChooseItem.No).FirstOrDefault();

                    this.model.DeliveryTable.Clear();
                    DeliveryTableModel tableModel = null;
                    if (currentCase != null)
                    {
                        foreach (var item in currentCase.Detail)
                        {
                            tableModel = new DeliveryTableModel();
                            tableModel.BrandName = item.BrandName;
                            tableModel.ClassName = item.ClassName;
                            tableModel.SizeName = item.SizeName;
                            tableModel.TaskCount = item.TaskCount;
                            tableModel.TextileCount = 0;
                            tableModel.DiffCount = tableModel.TextileCount - tableModel.TaskCount;
                            this.model.DeliveryTable.Add(tableModel);
                        }

                        Refresh();
                    }
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
                        case "House":
                            this.model.HouseID = modalModel.ChooseItem.ChooseID;
                            this.model.HouseName = modalModel.ChooseItem.ChooseName;
                            break;
                        case "DeliveryType":
                            if (this.model.DeliveryType == Convert.ToInt16(modalModel.ChooseItem.ChooseID))
                            {
                                return;
                            }
                            this.model.DeliveryType = Convert.ToInt16(modalModel.ChooseItem.ChooseID);
                            this.model.DeliveryTypeName = modalModel.ChooseItem.ChooseName;
                            this.model.TaskNo = string.Empty;
                            if (this.model.DeliveryType == 1)
                            {
                                this.model.TaskVisibility = Visibility.Visible;
                            }
                            else
                            {
                                this.model.DeliveryTable.Clear();
                                Refresh();
                                this.model.TaskVisibility = Visibility.Collapsed;
                            }
                            break;
                    }
                }
            }
        }

        private void CloseModalAction(string sender)
        {
            if (ConfigController.ReaderConfig != null)
            {
                ReaderController.Instance.ScanUtilities.StopScan();
            }

            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private async void SubmitAction(object sender)
        {
            if (this.model.DeliveryType == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择出库类型.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.HouseID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择出库地.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.DeliveryType == 1 && this.model.TaskNo == "")
            {
                EtexsysMessageBox.Show("提示", "请选择出库依据.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.DeliveryTable.Count == 0)
            {
                EtexsysMessageBox.Show("提示", "请扫描纺织品.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.model.DeliveryType == 1)
            {
                #region 判断超欠

                bool isMore = false, isLess = false;
                DeliveryTableModel stm = null;
                for (int i = 0; i < model.DeliveryTable.Count; i++)
                {
                    stm = model.DeliveryTable[i];
                    if (stm.TextileCount - stm.TaskCount > 0)
                    {
                        isMore = true;
                        break;
                    }
                    else if (stm.TextileCount - stm.TaskCount < 0)
                    {
                        isLess = true;
                    }
                }
                if (isMore)
                {
                    EtexsysMessageBox.Show("提示", "出库不允许超配.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (isLess)
                {
                    bool? isOK = EtexsysMessageBox.Show("提示", "存在欠发的纺织品，确定提交吗？", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (isOK == false) return;
                }

                #endregion
            }


            _isSubmit = true;
            this.model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitContent = "正在玩命提交.";
            ResetTime = DateTime.Now;

            var list = DeliveryedTagList.Select(v => v.TagNo).ToList();

            ScanList.RemoveAll(v => list.Contains(v));

            RFIDInvoiceParamModel requestParam = new RFIDInvoiceParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.HotelID = this.model.DeliveryType == 1 ? HotelID : 0;
            requestParam.RegionID = this.model.HouseID;
            requestParam.Quantity = this.model.TextileCount;
            requestParam.InvType = 4;
            requestParam.InvSubType = this.model.DeliveryType;
            requestParam.Tags = ScanList.ToArray();
            requestParam.CreateUserID = App.CurrentLoginUser.UserID;
            requestParam.CreateUserName = App.CurrentLoginUser.UName;
            requestParam.GUID = GUID;

            if (this.model.DeliveryType == 1)
            {
                var attach = new List<AttchModel>();
                var attachModel = new AttchModel();
                attachModel.Type = "TaskNo";
                attachModel.Value = this.model.TaskNo;
                attach.Add(attachModel);
                requestParam.Attach = attach;
            }

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.InsertRFIDInvoice, requestParam);

            this.model.WaitVisibled = Visibility.Hidden;
            if (rtn.ResultCode == 1)
            {
                this.model.SubmitEnabled = true;
                EtexsysMessageBox.Show("提示", rtn.ResultMsg, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (rtn.Result == null)
                {
                    this.model.SubmitEnabled = true;
                    EtexsysMessageBox.Show("提示", "出库失败", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string[] r = rtn.Result.ToString().Split('|');
                    InvNo = r[0];
                    CreateTime = r[1];
                    EtexsysMessageBox.Show("提示", "出库成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.model.PrintEnabled = true;
                }
            }

        }

        private async void CancelAction(object sender)
        {
            if (ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            ResetTime = DateTime.Now;
            TextileList.Clear();
            DeliveryedTagList.Clear();
            this.model.DeliveryedTotal = 0;
            this.model.SubmitEnabled = false;
            this.model.BtnDeliveryedEnabled = false;
            this.model.PrintEnabled = false;
            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.DeliveryTable.Clear();
            InvNo = string.Empty;
            CreateTime = string.Empty;
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
                this.model.TaskNo = "";
                _isSubmit = false;
                GUID = Guid.NewGuid().ToString();

                TaskList.Clear();
                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.DeliveryTask, null);
                if (apiRtn.ResultCode == 0)
                {
                    if (apiRtn.Result != null)
                    {
                        var temp = JsonConvert.DeserializeObject<List<ResponseDeliveryTaskModel>>(apiRtn.Result.ToString());
                        if (temp != null)
                        {
                            TaskList.AddRange(temp);
                        }
                    }
                }

                ChooseDeliveryItemModel itemModel = null;
                ChooseTaskList.Clear();
                foreach (var item in TaskList)
                {
                    itemModel = new ChooseDeliveryItemModel();
                    itemModel.HotelName = item.HotelName;
                    itemModel.HotelID = item.HotelID;
                    itemModel.No = item.DeliveryNo;
                    itemModel.TaskCount = item.DeliveryCount;
                    ChooseTaskList.Add(itemModel);
                }
            }
            else
            {

                if (this.model.DeliveryType == 1 && this.model.TaskNo != "")
                {
                    var currentCase = TaskList.Where(v => v.DeliveryNo == this.model.TaskNo).FirstOrDefault();

                    this.model.DeliveryTable.Clear();
                    DeliveryTableModel tableModel = null;
                    if (currentCase != null)
                    {
                        foreach (var item in currentCase.Detail)
                        {
                            tableModel = new DeliveryTableModel();
                            tableModel.BrandName = item.BrandName;
                            tableModel.ClassName = item.ClassName;
                            tableModel.SizeName = item.SizeName;
                            tableModel.TaskCount = item.TaskCount;
                            tableModel.TextileCount = 0;
                            tableModel.DiffCount = tableModel.TextileCount - tableModel.TaskCount;

                            this.model.DeliveryTable.Add(tableModel);
                        }
                    }
                }
            }
        }

        private void PrintAction(object sender)
        {
            if (!string.IsNullOrEmpty(InvNo) && InvNo.Length > 5)
            {
                PrintQueue pq = new PrintQueue();
                PrintAttachment att = null;

                int _printCount = ConfigController.BusinessSettingConfig.OtherPrintCount;
                int _printPaper = ConfigController.SystemSettingConfig.SelectPrintPaper;
                List<TableColumnHeaderModel> tableAttr = GetPrintTableAttr();
                DataTable _printTable = CreatePrintTable();

                var query = from t in this.model.DeliveryTable
                            where t.TextileCount > 0
                            group t by new
                            {
                                t.ClassName,
                                t.SizeName
                            }
                            into m
                            select new
                            {
                                className = m.Key.ClassName,
                                sizeName = m.Key.SizeName,
                                count = m.Sum(v => v.TextileCount)
                            };


                DataRow row = null;
                query.ToList().ForEach(q =>
                {
                    row = _printTable.NewRow();
                    row["ClassName"] = q.className;
                    row["SizeName"] = q.sizeName;
                    row["Count"] = q.count;
                    _printTable.Rows.Add(row);
                });

                for (int i = 0; i < _printCount; i++)
                {
                    att = new PrintAttachment();
                    att.Title = "出库单";
                    att.PrintTime = CreateTime;
                    att.DocumentNumber = InvNo;
                    att.HandlerName = App.CurrentLoginUser.UName;
                    att.Total = this.model.TextileCount;
                    att.PrintType = 1;
                    att.PaperType = _printPaper;
                    att.TableColumns = tableAttr;
                    att.PrintDataTable = _printTable;
                    pq.Add(att);
                }

                pq.Print();

            }
            else
            {
                EtexsysMessageBox.Show("提示", "本次出库失败，请重新操作.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeliveryAction(object sender)
        {
            ExecutedModal modal = new View.Choose.ExecutedModal();
            ExecutedViewModel viewModel = new Choose.ExecutedViewModel(modal.Close);
            viewModel.Model.Title = "重复出库";
            viewModel.Model.TagList = this.DeliveryedTagList;
            modal.ContentRendered += viewModel.Modal_ContentRendered;
            modal.DataContext = viewModel;
            modal.ShowDialog();
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

        private RelayCommand<object> _PrintCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_PrintCommand == null)
                {
                    _PrintCommand = new RelayCommand<object>(PrintAction);
                }

                return _PrintCommand;
            }
        }

        private RelayCommand<object> _deliveryCommand;
        public ICommand DeliveryCommand
        {
            get
            {
                if (_deliveryCommand == null)
                {
                    _deliveryCommand = new RelayCommand<object>(DeliveryAction);
                }
                return _deliveryCommand;
            }
        }

        #endregion

        #region 私有方法

        private List<TableColumnHeaderModel> GetPrintTableAttr()
        {
            int _printPaper = ConfigController.SystemSettingConfig.SelectPrintPaper;

            List<TableColumnHeaderModel> tableAttr = new List<TableColumnHeaderModel>();
            TableColumnHeaderModel tableModel = null;
            if (_printPaper == 1)
            {
                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "名称";
                tableModel.Alignment = ColumnHAlignment.Left;
                tableModel.Width = 80;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "尺寸";
                tableModel.Alignment = ColumnHAlignment.Left;
                tableModel.Width = 60;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "数量";
                tableModel.Alignment = ColumnHAlignment.Right;
                tableModel.Width = 50;
                tableAttr.Add(tableModel);
            }
            else
            {
                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "名称";
                tableModel.Alignment = ColumnHAlignment.Left;
                tableModel.Width = 120;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "尺寸";
                tableModel.Alignment = ColumnHAlignment.Left;
                tableModel.Width = 100;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "数量";
                tableModel.Alignment = ColumnHAlignment.Right;
                tableModel.Width = 60;
                tableAttr.Add(tableModel);
            }
            return tableAttr;
        }

        private DataTable CreatePrintTable()
        {
            DataTable _printTable = new DataTable();

            DataColumn col = new DataColumn();
            col.DataType = typeof(string);
            col.ColumnName = "ClassName";
            col.DefaultValue = string.Empty;
            _printTable.Columns.Add(col);

            col = new DataColumn();
            col.DataType = typeof(string);
            col.ColumnName = "SizeName";
            col.DefaultValue = string.Empty;
            _printTable.Columns.Add(col);

            col = new DataColumn();
            col.DataType = typeof(int);
            col.ColumnName = "Count";
            col.DefaultValue = 0;
            _printTable.Columns.Add(col);

            return _printTable;
        }

        private void Refresh()
        {
            var query = from t in TextileList
                        where t.TextileState == 0
                        group t by new
                        {
                            t1 = t.BrandSort,
                            t2 = t.BrandName,
                            t3 = t.ClassSort,
                            t4 = t.ClassName,
                            t5 = t.SizeSort,
                            t6 = t.SizeName
                        } into m
                        orderby m.Key.t1, m.Key.t2, m.Key.t3, m.Key.t4, m.Key.t5, m.Key.t6
                        select new
                        {
                            brandName = m.Key.t2,
                            className = m.Key.t4,
                            sizeName = m.Key.t6,
                            count = m.Count()
                        };

            int total = 0;

            query.ToList().ForEach(q =>
            {
                DeliveryTableModel row = null;
                if (string.IsNullOrEmpty(q.sizeName))
                {
                    row = this.model.DeliveryTable.Where(v => v.BrandName == q.brandName && v.ClassName == q.className).FirstOrDefault();
                }
                else
                {
                    row = this.model.DeliveryTable.Where(v => v.BrandName == q.brandName && v.ClassName == q.className && v.SizeName == q.sizeName).FirstOrDefault();
                }
                if (row == null)
                {
                    DeliveryTableModel rowModel = new DeliveryTableModel();
                    rowModel.BrandName = q.brandName;
                    rowModel.ClassName = q.className;
                    rowModel.SizeName = q.sizeName;
                    rowModel.TaskCount = 0;
                    rowModel.TextileCount = q.count;
                    rowModel.DiffCount = rowModel.TextileCount - rowModel.TaskCount;

                    this.model.DeliveryTable.Add(rowModel);
                }
                else
                {
                    row.TextileCount = q.count;
                    row.DiffCount = row.TextileCount - row.TaskCount;
                }
                total += q.count;
            });

            DeliveryedTagList = TextileList.Where(v => v.TextileState != 0).ToList();
            this.model.DeliveryedTotal = DeliveryedTagList.Count;
            this.model.BtnDeliveryedEnabled = DeliveryedTagList.Count > 0 ? true : false;


            this.model.UnRegisterTotal = ScanList.Count - total - model.DeliveryedTotal;
            this.model.TextileCount = total;
        }

        #endregion
    }
}
