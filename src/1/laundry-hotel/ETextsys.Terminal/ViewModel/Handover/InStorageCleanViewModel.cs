﻿using ETexsys.APIRequestModel;
using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Handover;
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

namespace ETextsys.Terminal.ViewModel.Handover
{
    public class InStorageCleanViewModel : ViewModelBase, IRFIDScan
    {
        private InStorageCleanModel model;
        public InStorageCleanModel Model
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

        private string GUID;

        /// <summary>
        /// 界面数据是否准备好
        /// </summary>
        private bool Prepare;

        private string InvNo { get; set; }

        private string CreateTime { get; set; }

        private DateTime ResetTime;
        #region 界面待选数据

        private ObservableCollection<ChooseModel> HouseList;

        #endregion

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private List<ResponseRFIDTagModel> TextileList;

        private List<ResponseTruckRFIDTagModel> TruckList;

        private List<string> ScanList;
        /// <summary>
        /// 已收货芯片码
        /// </summary>
        private List<ResponseRFIDTagModel> StoragedTagList;

        public InStorageCleanViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new InStorageCleanModel();
            this.model.StorageTable = new ObservableCollection<StorageTableModel>();
            this.model.SubmitEnabled = false;
            this.model.PrintEnabled = false;
            this.model.CancelEnabled = true;
            this.model.BtnStorageEnabled = false;
            this.model.WaitVisibled = Visibility.Hidden;
            this.model.StoragedTotal = 0;
            this.model.TruckTotal = 0;

            ResetTime = DateTime.Now;
            TextileList = new List<ResponseRFIDTagModel>();
            TruckList = new List<ResponseTruckRFIDTagModel>();
            ScanList = new List<string>();
            StoragedTagList = new List<ResponseRFIDTagModel>();
            _isSubmit = false;
            GUID = Guid.NewGuid().ToString();

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

            HouseList = new ObservableCollection<ChooseModel>();
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.StoreList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var store = JsonConvert.DeserializeObject<List<ResponseStoreModel>>(apiRtn.Result.ToString());
                    if (store != null)
                    {
                        store.Where(v => v.StoreType == 1).OrderBy(v => v.Sort).ThenBy(v => v.StoreName).ToList().ForEach(q =>
                        {
                            HouseList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.StoreName });
                        });
                    }
                }
            }

            if (HouseList.Count == 1)
            {
                this.model.HouseID = HouseList.FirstOrDefault().ChooseID;
                this.model.HouseName = HouseList.FirstOrDefault().ChooseName;
            }
            Prepare = true;
        }

        #region RFID Interface

        public void NoScanTag()
        {
            if (model.StorageTable.Count > 0 && !_isSubmit)
            {
                this.model.SubmitEnabled = true;
            }
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            var trucktags = rfidTags.Where(v => v.Type == TagType.Truck).Select(v => v.TagNo).ToList();
            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;

            var tag = TextileList.Select(v => v.TagNo).ToList();
            tags = tags.Where(v => !tag.Contains(v)).ToList();

            if (tags.Count > 0)
            {
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
                }
            }
            if (trucktags.Count > 0)
            {
                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = trucktags.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.LoadTextileByTruck, requestParam);
                if (apiRtn.ResultCode == 0)
                {
                    if (apiRtn.Result != null && apiRtn.OtherResult != null)
                    {
                        DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                        var temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                        if (temp != null && d > ResetTime)
                        {
                            TextileList.AddRange(temp);
                        }

                        tag = TextileList.Where(v => !ScanList.Contains(v.TagNo)).Select(v => v.TagNo).ToList();
                        ScanList.AddRange(tag);
                        List<TagModel> tList = new List<TagModel>();
                        foreach (var t in tag)
                        {
                            tList.Add(new TagModel { TagNo = t, Type = TagType.Textile });
                        }
                        ReaderController.Instance.ScanUtilities.UidList.AddRange(tList);
                    }
                }
            }
            TextileList = TextileList.OrderBy(v => v.TagNo).ThenByDescending(v => v.TruckTagNo).ToList();
            TextileList = TextileList.Where((x, y) => TextileList.FindIndex(z => z.TagNo == x.TagNo) == y).ToList();
            ScanList = ScanList.Distinct().ToList();

            StorageTableModel receiveModel = null;
            var query = from t in TextileList
                        where t.TextileState != (int)EnumTextileState.PutIn
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
            model.StorageTable.Clear();
            query.ToList().ForEach(q =>
            {
                receiveModel = new StorageTableModel();
                receiveModel.BrandName = q.brandName;
                receiveModel.ClassName = q.className;
                receiveModel.SizeName = q.sizeName;
                receiveModel.TextileCount = q.count;
                total += q.count;
                model.StorageTable.Add(receiveModel);
            });

            StoragedTagList = TextileList.Where(v => v.TextileState == (int)EnumTextileState.PutIn).ToList();
            this.model.StoragedTotal = StoragedTagList.Count;
            this.model.BtnStorageEnabled = StoragedTagList.Count > 0 ? true : false;

            this.model.UnRegisterTotal = ScanList.Count - total - this.model.StoragedTotal;
            this.model.TextileCount = total;

            TruckList.Clear();
            TextileList.Where(v => !string.IsNullOrWhiteSpace(v.TruckTagNo)).GroupBy(v => v.TruckTagNo).Select(v => v.Key).ToList().ForEach(
              x =>
              {
                  TruckList.Add(new ResponseTruckRFIDTagModel { TrunckRFIDTagNo = x });
              });
            this.model.TruckTotal = TruckList.Count;
            this.model.BtnTruckEnabled = TruckList.Count > 0 ? true : false;
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
            Border border = (Border)sender;
            int state = Convert.ToInt32(border.Tag);
            switch (state)
            {
                case 0:
                    if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
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
                    if (ReaderController.Instance.ScanUtilities != null)
                    {
                        ReaderController.Instance.ScanUtilities.StopScan();
                        System.Threading.Thread.Sleep(50);
                        this.model.ReaderState = 2;
                        this.model.ReaderLight = new Uri("../../Skins/Default/Images/stopscan.gif", UriKind.RelativeOrAbsolute);
                    }
                    break;
                case 2:
                    if (ReaderController.Instance.ScanUtilities != null)
                    {
                        ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                        {
                            model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                            model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                        });
                    }
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

                default:
                    return;
            }

            DownModal modal = new DownModal();
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
            if (this.model.HouseID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择租赁仓库.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.model.StorageTable.Count == 0)
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

            var list = StoragedTagList.Select(v => v.TagNo).ToList();

            ScanList.RemoveAll(v => list.Contains(v));

            RFIDInvoiceParamModel requestParam = new RFIDInvoiceParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.HotelID = 0;
            requestParam.RegionID = this.model.HouseID;
            requestParam.Quantity = this.model.TextileCount;
            requestParam.InvType = (int)EnumInvType.PutIn;
            requestParam.InvSubType = 22;
            requestParam.Tags = ScanList.ToArray();
            requestParam.CreateUserID = App.CurrentLoginUser.UserID;
            requestParam.CreateUserName = App.CurrentLoginUser.UName;
            requestParam.GUID = GUID;

            requestParam.Attach = new List<AttchModel>();

            foreach (var item in TruckList)
            {
                AttchModel am = new AttchModel();
                am.Type = "TruckNo";
                am.Value = item.TrunckRFIDTagNo;
                requestParam.Attach.Add(am);
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
                    EtexsysMessageBox.Show("提示", "净物入库失败", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string[] r = rtn.Result.ToString().Split('|');
                    InvNo = r[0];
                    CreateTime = r[1];
                    EtexsysMessageBox.Show("提示", "净物入库成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.model.PrintEnabled = true;
                }
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
            TruckList.Clear();
            StoragedTagList.Clear();
            this.model.SubmitEnabled = false;
            this.model.BtnStorageEnabled = false;
            this.model.PrintEnabled = false;
            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.TruckTotal = 0;
            this.model.StorageTable.Clear();
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
                _isSubmit = false;
                GUID = Guid.NewGuid().ToString(); 
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

                var query = from t in this.model.StorageTable
                            group t by new
                            {
                                t.ClassName,
                                t.SizeName
                            } into m
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

                List<string> trucks = null;
                if (TruckList != null && TruckList.Count > 0)
                {
                    trucks = TruckList.Select(c => c.TrunckRFIDTagNo).ToList();
                }

                for (int i = 0; i < _printCount; i++)
                {
                    att = new PrintAttachment();
                    att.Title = "净物入库单";
                    att.PrintTime = CreateTime;
                    att.DocumentNumber = InvNo;
                    att.HandlerName = App.CurrentLoginUser.UName;
                    att.RegionName = this.model.HouseName;
                    att.Total = this.model.TextileCount;
                    att.PrintType = 1;
                    att.PaperType = _printPaper;
                    att.TableColumns = tableAttr;
                    att.PrintDataTable = _printTable;
                    att.Trucks = trucks;
                    pq.Add(att);
                }

                pq.Print();

            }
            else
            {
                EtexsysMessageBox.Show("提示", "本次净物入库失败，请重新操作.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StorageAction(object sender)
        {
            ExecutedModal modal = new View.Choose.ExecutedModal();
            ExecutedViewModel viewModel = new Choose.ExecutedViewModel(modal.Close);
            viewModel.Model.Title = "已入库";
            viewModel.Model.TagList = this.StoragedTagList;
            modal.ContentRendered += viewModel.Modal_ContentRendered;
            modal.DataContext = viewModel;
            modal.ShowDialog();
        }

        private void TruckAction(object sender)
        {
            TruckModal modal = new TruckModal();
            TruckViewModel vm = new TruckViewModel(modal.Close);
            vm.Model.Title = "架子车";
            vm.Model.TagList = this.TextileList;
            modal.ContentRendered += vm.Modal_ContentRendered;
            modal.DataContext = vm;
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

        private RelayCommand<object> _StorageCommand;
        public ICommand StorageCommand
        {
            get
            {
                if (_StorageCommand == null)
                {
                    _StorageCommand = new RelayCommand<object>(StorageAction);
                }
                return _StorageCommand;
            }
        }

        private RelayCommand<object> _TruckCommand;
        public ICommand TruckCommand
        {
            get
            {
                if (_TruckCommand == null)
                {
                    _TruckCommand = new RelayCommand<object>(TruckAction);
                }
                return _TruckCommand;
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

        #endregion
    }
}
