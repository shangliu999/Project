using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Logistics;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ETextsys.Terminal.ViewModel.Logistics
{
    public class InFactoryViewModel : ViewModelBase, IRFIDScan
    {
        private InFactoryModel model;
        public InFactoryModel Model
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

        private string InvNo { get; set; }

        private string CreateTime { get; set; }

        private string GUID;

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private List<ResponseRFIDTagModel> TextileList;

        private List<string> ScanList;

        private DateTime ResetTime;

        public InFactoryViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new InFactoryModel();
            this.model.FactoryTable = new ObservableCollection<FactoryTableModel>();
            this.model.SubmitEnabled = false;
            this.model.PrintEnabled = false;
            this.model.CancelEnabled = true;
            this.model.WaitVisibled = Visibility.Hidden;
            this.model.FactoryTotal = 0;

            TextileList = new List<ResponseRFIDTagModel>();
            ScanList = new List<string>();
            _isSubmit = false;
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

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadTask();
        }

        #region RFID Interface

        public void NoScanTag()
        {
            if (model.FactoryTable.Count > 0 && !_isSubmit)
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
                            where t.TextileState != 5
                            group t by new
                            {
                                t1 = t.LastReceiveInvID,
                            } into m
                            select new
                            {
                                m.Key.t1,
                                count = m.Count()
                            };

                int total = 0;
                string invid = "";
                FactoryTableModel ftm = null;
                query.ToList().ForEach(q =>
                {
                    if (q.t1 == null)
                        invid = "";
                    else
                        invid = q.t1;

                    ftm = model.FactoryTable.FirstOrDefault(c => c.InvId.Equals(invid));
                    if (ftm != null)
                    {
                        ftm.TextileCount = q.count;
                        ftm.DiffCount = ftm.TextileCount - ftm.TaskCount;

                        ftm.ScanState = 1;
                        ftm.TagNos.AddRange(TextileList.Where(c => !string.IsNullOrEmpty(c.LastReceiveInvID) && c.LastReceiveInvID.Equals(invid)).Select(c => c.TagNo));
                    }
                    else
                    {
                        ftm = model.FactoryTable.FirstOrDefault(c => string.IsNullOrEmpty(c.InvId));
                        if (ftm == null)
                        {
                            //不在今日收货单中
                            ftm = new FactoryTableModel();
                            ftm.InvId = "";
                            ftm.TextileCount = q.count;
                            ftm.TagNos = new List<string>();
                            model.FactoryTable.Add(ftm);
                        }
                        else
                        {
                            ftm.TextileCount = q.count;
                            ftm.DiffCount = ftm.TextileCount - ftm.TaskCount;
                            ftm.ScanState = 1;
                        }
                    }

                    total += q.count;
                });

                List<FactoryTableModel> list = model.FactoryTable.OrderByDescending(c => c.ScanState).ThenByDescending(c => c.InvNo).ToList();
                model.FactoryTable.Clear();
                list.ForEach(c => { model.FactoryTable.Add(c); });

                model.FactoryTotal = TextileList.Where(v => v.TextileState == 5).Count();
                model.UnRegisterTotal = ScanList.Count - total - this.model.FactoryTotal;
                model.TextileCount = total;
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
            if (this.model.FactoryTable.Count == 0)
            {
                EtexsysMessageBox.Show("提示", "请扫描纺织品.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            #region 判断每个客户入厂90%（废弃）

            //var query = from c in model.FactoryTable
            //            group c by new
            //            {
            //                InvNo = c.InvNo.Substring(2, 8),
            //                c.HotelName
            //            } into m
            //            select new
            //            {
            //                m.Key.InvNo,
            //                m.Key.HotelName
            //            };

            //int taskcount, textilecount;
            //taskcount = textilecount = 0;
            //var q = query.ToList();
            //for (int i = 0; i < q.Count; i++)
            //{
            //    var c = q[i];
            //    var ienumer = model.FactoryTable.Where(x => x.InvNo.Substring(2, 8).Equals(c.InvNo) && x.HotelName.Equals(c.HotelName));
            //    taskcount = ienumer.Sum(x => x.TaskCount);
            //    textilecount = ienumer.Sum(x => x.TextileCount);

            //    if (taskcount * 0.9 - textilecount > 0)
            //    {
            //        EtexsysMessageBox.Show("提示", c.HotelName + "的入厂数量不足90%，请确认后重新扫描.", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return;
            //    }
            //}

            #endregion

            #region 判断每条单据

            int per = 90;
            if (ConfigController.SystemSettingConfig != null)
            {
                if (ConfigController.SystemSettingConfig.InFactoryPercentage != 0)
                {
                    per = ConfigController.SystemSettingConfig.InFactoryPercentage;
                }
            }
            double percentage = per * 0.01;

            //入厂数量达到90%的污物送洗单
            List<string> scanrecinvids = new List<string>();

            var list1 = model.FactoryTable.Where(c => c.TextileCount > 0).ToList();
            string erroeInvNos = string.Empty;
            for (int i = 0; i < list1.Count; i++)
            {
                var c = list1[i];
                if (c.TaskCount * percentage - c.TextileCount > 0)
                {
                    erroeInvNos += c.InvNo + ",";
                }
                scanrecinvids.Add(c.InvId);
            }

            if (!string.IsNullOrEmpty(erroeInvNos))
            {
                erroeInvNos = erroeInvNos.TrimEnd(',');

                string msg = string.Format("单据：{0}的污物复核数量不足{1}%，是否需要重新扫描？", erroeInvNos, per);
                bool? isOK = EtexsysMessageBox.Show("提示", msg, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (isOK == true)
                {
                    return;
                }
            }

            #endregion

            _isSubmit = true;
            this.model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitContent = "正在玩命提交.";
            ResetTime = DateTime.Now;
            var list = this.TextileList.Where(v => v.TextileState == 5).Select(v => v.TagNo).ToList();

            ScanList.RemoveAll(v => list.Contains(v));

            RFIDInvoiceParamModel requestParam = new RFIDInvoiceParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.HotelID = 0;
            requestParam.RegionID = 0;
            requestParam.Quantity = this.model.TextileCount;
            requestParam.InvType = 5;
            requestParam.InvSubType = 0;
            requestParam.Tags = ScanList.ToArray();
            requestParam.CreateUserID = App.CurrentLoginUser.UserID;
            requestParam.CreateUserName = App.CurrentLoginUser.UName;
            requestParam.GUID = GUID;
            requestParam.TaskInv = scanrecinvids;
            requestParam.Attach = new List<AttchModel>();

            foreach (var item in scanrecinvids)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    AttchModel am = new AttchModel();
                    am.Type = "TaskID";
                    am.Value = item;
                    requestParam.Attach.Add(am);
                }
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
                    EtexsysMessageBox.Show("提示", "污物复核失败", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string[] r = rtn.Result.ToString().Split('|');
                    InvNo = r[0];
                    CreateTime = r[1];
                    EtexsysMessageBox.Show("提示", "污物复核成功", MessageBoxButton.OK, MessageBoxImage.Information);
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
            model.SubmitEnabled = false;
            model.PrintEnabled = false;
            model.UnRegisterTotal = 0;
            model.TextileCount = 0;
            model.FactoryTotal = 0;
            model.FactoryTable.Clear();
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
            LoadTask();

            if (_isSubmit)
            {
                GUID = Guid.NewGuid().ToString();
                _isSubmit = false;
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

                var query = from t in TextileList
                            group t by new
                            {
                                t.ClassName,
                                t.SizeName
                            } into m
                            select new
                            {
                                className = m.Key.ClassName,
                                sizeName = m.Key.SizeName,
                                count = m.Count(),
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
                    att.Title = "污物复核单";
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
                EtexsysMessageBox.Show("提示", "本次污物复核失败，请重新操作.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DetailAction(object sender)
        {
            if (sender == null)
            {
                EtexsysMessageBox.Show("提示", "请选择一行记录。", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FactoryTableModel ftm = sender as FactoryTableModel;
            //if (string.IsNullOrEmpty(ftm.InvId))
            //{
            //    EtexsysMessageBox.Show("提示", "未查询到污物送洗记录。", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            TaskDetails modal = new TaskDetails();
            TaskDetailsViewModel viewModel = new TaskDetailsViewModel(modal.Close);
            viewModel.FactoryTableModel = ftm;
            if (!string.IsNullOrEmpty(ftm.InvNo))
            {
                viewModel.TextileList = TextileList.Where(c => ftm.TagNos.Contains(c.TagNo)).ToList();
            }
            else
            {
                viewModel.TextileList = TextileList.Where(c => string.IsNullOrEmpty(ftm.InvNo)).ToList();
            }
            viewModel.Model.Title = "任务详情";
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

        private RelayCommand<object> _DetailCommand;
        public ICommand DetailCommand
        {
            get
            {
                if (_DetailCommand == null)
                {
                    _DetailCommand = new RelayCommand<object>(DetailAction);
                }

                return _DetailCommand;
            }
        }

        #endregion

        #region 私有方法

        private async void LoadTask()
        {
            InfactoryTaskParamModel requestParam = new InfactoryTaskParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.HotelID = 0;//不根据酒店查询

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.InfactoryTask, requestParam);
            if (rtn.ResultCode == 0)
            {
                if (rtn.Result != null)
                {
                    var temp = JsonConvert.DeserializeObject<List<ResponseInfactoryTaskModel>>(rtn.Result.ToString());
                    if (temp != null)
                    {
                        FactoryTableModel ftm = null;
                        temp.ToList().ForEach(c =>
                        {
                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                SynchronizationContext.SetSynchronizationContext(new
                                    DispatcherSynchronizationContext(Application.Current.Dispatcher));
                                SynchronizationContext.Current.Send(pl =>
                                {
                                    ftm = new FactoryTableModel();

                                    ftm.InvId = c.InvID;
                                    ftm.InvNo = c.InvNo;
                                    ftm.HotelName = c.HotelName;
                                    ftm.RegionName = c.RegionName;
                                    ftm.TaskCount = c.TaskCount;
                                    ftm.TextileCount = c.TextileCount;
                                    ftm.DiffCount = c.DiffCount;

                                    ftm.TagNos = new List<string>();

                                    model.FactoryTable.Add(ftm);
                                }, null);
                            });
                        });
                    }
                }
            }
        }

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
