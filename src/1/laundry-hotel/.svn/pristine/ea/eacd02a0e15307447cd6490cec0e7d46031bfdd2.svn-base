using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Logistics;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.Utilities.PrintBase;
using ETextsys.Terminal.View.Choose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Logistics
{
    public class InsideBackWashingViewModel : ViewModelBase, IRFIDScan
    {
        private InsideBackWashingModel model;
        public InsideBackWashingModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

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

        public InsideBackWashingViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new InsideBackWashingModel();
            this.model.BackWashingTable = new ObservableCollection<BackWashingTableModel>();
            this.model.SubmitEnabled = false;
            this.model.PrintEnabled = false;
            this.model.CancelEnabled = true;
            this.model.WaitVisibled = Visibility.Hidden;

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
        }

        #region RFID Interface

        public void NoScanTag()
        {
            if (model.BackWashingTable.Count > 0 && !_isSubmit)
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
                        TextileList.AddRange(temp);
                        ScanList.AddRange(tags);
                    }
                }

                BackWashingTableModel receiveModel = null;
                var query = from t in TextileList
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
                this.model.BackWashingTable.Clear();
                query.ToList().ForEach(q =>
                {
                    receiveModel = new BackWashingTableModel();
                    receiveModel.BrandName = q.brandName;
                    receiveModel.ClassName = q.className;
                    receiveModel.SizeName = q.sizeName;
                    receiveModel.TextileCount = q.count;
                    total += q.count;
                    this.model.BackWashingTable.Add(receiveModel);
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
            if (this.model.BackWashingTable.Count == 0)
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

            RFIDInvoiceParamModel requestParam = new RFIDInvoiceParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.HotelID = 0;
            requestParam.RegionID = 0;
            requestParam.Quantity = this.model.TextileCount;
            requestParam.InvType = 8;
            requestParam.InvSubType = 0;
            requestParam.Tags = ScanList.ToArray();
            requestParam.CreateUserID = App.CurrentLoginUser.UserID;
            requestParam.CreateUserName = App.CurrentLoginUser.UName;
            requestParam.GUID = GUID;


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
                    EtexsysMessageBox.Show("提示", "配送中心返洗失败", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string[] r = rtn.Result.ToString().Split('|');
                    InvNo = r[0];
                    CreateTime = r[1];
                    EtexsysMessageBox.Show("提示", "配送中心返洗成功", MessageBoxButton.OK, MessageBoxImage.Information);
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
            this.model.SubmitEnabled = false;
            this.model.PrintEnabled = false;
            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.BackWashingTable.Clear();
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

                var query = from t in this.model.BackWashingTable
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

                for (int i = 0; i < _printCount; i++)
                {
                    att = new PrintAttachment();
                    att.Title = "配送中心返洗单";
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
                EtexsysMessageBox.Show("提示", "本次配送中心返洗失败，请重新操作.", MessageBoxButton.OK, MessageBoxImage.Information);
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
