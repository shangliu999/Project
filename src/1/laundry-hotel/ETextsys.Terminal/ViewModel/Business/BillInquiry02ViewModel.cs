using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Business;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.Utilities.PrintBase;
using ETextsys.Terminal.View.Business;
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

namespace ETextsys.Terminal.ViewModel.Business
{
    public class BillInquiry02ViewModel
    {
        private BackgroundWorker _worker;
        private ObservableCollection<ChooseModel> HotelList;
        private ObservableCollection<ChooseModel> FloorList;
        private List<ResponseRegionModel> AllRegionList;
        private List<string> BagList;
        private bool Prepare;
        Action _closeAction;

        string CreateTime;

        string PrintInvTypeName;

        int Total;

        private BillInquiry02Model model;
        public BillInquiry02Model Model
        {
            get { return model; }
            set { model = value; }
        }

        private RelayCommand<object> print;
        public ICommand Print
        {
            get
            {
                if (print == null)
                {
                    print = new RelayCommand<object>(PrintAction);
                }
                return print;
            }
        }
        public void PrintAction(object parameter)
        {

            PrintInvTypeName = model.InvTypeName + "汇总";
            model.WaitContent = "正在打印...";
            model.WaitVisibled = "Visible";
            PrintQueue pq = new PrintQueue();
            PrintAttachment att = null;
            int _printCount = ConfigController.BusinessSettingConfig.OtherPrintCount;
            int _printPaper = ConfigController.SystemSettingConfig.SelectPrintPaper;
            List<TableColumnHeaderModel> tableAttr = GetPrintTableAttr();
            DataTable _printTable = CreatePrintTable();
            Total = 0;

            var query = from t in model.ResetTable
                        group t by new
                        {
                            t.ProductName,
                            t.Size
                        } into m
                        select new
                        {
                            croductName = m.Key.ProductName,
                            size = m.Key.Size,
                            number = m.Sum(v => v.Number)
                        };
            DataRow row = null;
            query.ToList().ForEach(q =>
            {
                row = _printTable.NewRow();
                row["ProductName"] = q.croductName;
                row["Size"] = q.size;
                row["Number"] = q.number;
                Total += q.number;
                _printTable.Rows.Add(row);
            });

            for (int i = 0; i < _printCount; i++)
            {
                att = new PrintAttachment();
                att.Title = PrintInvTypeName;
                att.PrintTime = model.CreateTime;
                if (model.InvType == 1 || model.InvType == 2)
                {
                    if (string.IsNullOrWhiteSpace(model.RegionName))
                    {
                        att.RegionName = model.HotelName;
                    }
                    else
                    {
                        att.RegionName = model.HotelName + "/" + model.RegionName;
                    }
                }
                //att.HandlerName = App.CurrentLoginUser.UName;
                att.PrintType = 1;
                att.PaperType = _printPaper;
                att.TableColumns = tableAttr;
                att.Total = Total;
                att.BagCodes = BagList;
                att.PrintDataTable = _printTable;
                pq.Add(att);
            }
            pq.Print();
            model.WaitVisibled = "Hidden";
        }

        private RelayCommand<DataGrid> selectBtn;
        public ICommand SelectBtn
        {
            get
            {
                if (selectBtn == null)
                {
                    selectBtn = new RelayCommand<DataGrid>(SelectBtnAction);
                }
                return selectBtn;
            }
        }
        public async void SelectBtnAction(DataGrid parameter)
        {
            if (model.InvTypeName == null)
            {
                EtexsysMessageBox.Show("提示", "请选择单据类型.", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (model.InvType == 1 || model.InvType == 2)
            {
                if (model.HotelId == 0)
                {
                    EtexsysMessageBox.Show("提示", "请选择酒店名.", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            model.WaitContent = "正在加载...";
            model.WaitVisibled = "Visible";
            if (parameter.Items.Count != 0)
            {
                model.ResetTable.Clear();
            }
            SummaryParamModel summary = new SummaryParamModel();
            summary.InvType = model.InvType;
            summary.CreateTime = Convert.ToDateTime(model.CreateTime);
            summary.HotelID = model.HotelId;
            summary.RegionID = model.RegionID;
            summary.UUID = ConfigController.MacCode;
            summary.TimeStamp = ApiController.Instance.GetTimeStamp();
            summary.TerminalType = ConfigController.TerminalType;
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.Summary, summary);
            if (apiRtn != null)
            {
                var temp = JsonConvert.DeserializeObject<List<ResponseSummary>>(apiRtn.Result.ToString());
                resposparameter respos = new resposparameter();
                var query = from t in temp
                            select t;
                query.ToList().ForEach(q =>
                {
                    respos = new resposparameter();
                    respos.Number = q.Number;
                    respos.ProductName = q.ProductName;
                    respos.Size = q.Size;
                    model.ResetTable.Add(respos);
                });
                model.Bags = "";
                BagList.Clear();
                if (apiRtn.OtherResult != null)
                {
                    var b = JsonConvert.DeserializeObject<List<string>>(apiRtn.OtherResult.ToString());
                    if (b != null)
                    {
                        StringBuilder bag = new StringBuilder();
                        foreach (var item in b)
                        {
                            bag.AppendFormat("{0},", item);
                        }

                        if (bag.Length > 1)
                        {
                            model.Bags = "包号：" + bag.ToString().Substring(0, bag.Length - 1);
                        }
                        BagList = b;
                    }
                }
            }
            if (parameter.Items.Count != 0)
            {
                model.IsEnabled = "true";
            }
            if (parameter.Items.Count == 0)
            {
                model.IsEnabled = "false";
            }
            model.WaitVisibled = "Hidden";
        }

        private RelayCommand<object> getTime;
        public ICommand GetTime
        {
            get
            {
                if (getTime == null)
                {
                    getTime = new RelayCommand<object>(GetTimeAction);
                }
                return getTime;
            }
        }
        public void GetTimeAction(object paramte)
        {
            ETextsys.Terminal.View.Business.Calendar calendar = new ETextsys.Terminal.View.Business.Calendar();
            System.Windows.Controls.Calendar a = new System.Windows.Controls.Calendar();
            CalendarViewModel calendarViewModel = calendar.DataContext as CalendarViewModel;
            calendar.DataContext = calendarViewModel;
            calendar.ShowDialog();
            if (calendarViewModel.Model.Time != null)
            {
                model.CreateTime = calendarViewModel.Model.Time;
            }
        }

        private RelayCommand<string> _ChooseAttrChanged;
        public ICommand ChooseAttrChanged
        {
            get
            {
                if (_ChooseAttrChanged == null)
                {
                    _ChooseAttrChanged = new RelayCommand<string>(chooseAttrAction);
                }
                return _ChooseAttrChanged;
            }
        }
        public void chooseAttrAction(string sender)
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
                    title = "酒店:";
                    break;
                case "Floor":
                    if (model.HotelId != 0)
                    {
                        list = FloorList;
                    }
                    else
                    {
                        EtexsysMessageBox.Show("提示", "请先选择酒店名.", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    title = "楼层";
                    break;
                case "Time":
                    title = "时间";
                    break;
                case "DocumentType":
                    list.Add(new ChooseModel() { ChooseID = 1, ChooseName = "污物送洗单" });
                    list.Add(new ChooseModel() { ChooseID = 2, ChooseName = "净物配送单" });
                    list.Add(new ChooseModel() { ChooseID = 22, ChooseName = "净物入库" });
                    list.Add(new ChooseModel() { ChooseID = 21, ChooseName = "回厂送洗" });
                    list.Add(new ChooseModel() { ChooseID = 24, ChooseName = "洗涤入库" });
                    list.Add(new ChooseModel() { ChooseID = 20, ChooseName = "净物出库" });
                    list.Add(new ChooseModel() { ChooseID = 4, ChooseName = "成品出库" });
                    list.Add(new ChooseModel() { ChooseID = 50, ChooseName = "污物复核" });
                    list.Add(new ChooseModel() { ChooseID = 51, ChooseName = "污物入厂" });
                    list.Add(new ChooseModel() { ChooseID = 80, ChooseName = "配送返洗" });
                    list.Add(new ChooseModel() { ChooseID = 81, ChooseName = "工厂返洗" });
                    list.Add(new ChooseModel() { ChooseID = 6, ChooseName = "订单" });
                    list.Add(new ChooseModel() { ChooseID = 7, ChooseName = "退回" });
                    title = "单据类型";
                    break;
                default:
                    break;
            }

            if (sender == "Time")
            {
                ETextsys.Terminal.View.Business.Calendar calendar = new ETextsys.Terminal.View.Business.Calendar();
                calendar.ShowDialog();
                CalendarViewModel calendarViewModel = calendar.DataContext as CalendarViewModel;
                if (calendarViewModel.Model.Time != null)
                {
                    model.CreateTime = calendarViewModel.Model.Time;
                }
            }
            else if (sender == "Hotel")
            {
                ChooseHotel hotel = new ChooseHotel();
                ChooseHotelViewModel hotelViewModel = new ChooseHotelViewModel(hotel.Close);
                FloorList = new ObservableCollection<ChooseModel>();
                hotelViewModel.Model.AllChooseList = list;
                hotelViewModel.Model.Title = title;
                hotelViewModel.RefreshPage();
                hotel.DataContext = hotelViewModel;
                hotel.ShowDialog();
                if (hotelViewModel.ChooseItem != null)
                {
                    switch (sender)
                    {
                        case "Hotel":
                            this.model.HotelId = hotelViewModel.ChooseItem.ChooseID;
                            this.model.HotelName = hotelViewModel.ChooseItem.ChooseName;
                            FloorList = new ObservableCollection<ChooseModel>();
                            this.model.RegionID = 0;
                            this.model.RegionName = null;

                            var query = from t in AllRegionList where t.HotelID == this.model.HotelId select new { regionName = t.RegionName, sort = t.Sort, id = t.ID };

                            query.OrderBy(v => v.sort).ThenBy(v => v.regionName).ToList().ForEach(q =>
                            {
                                FloorList.Add(new Terminal.Model.Choose.ChooseModel { ChooseID = q.id, ChooseName = q.regionName });
                            });

                            if (FloorList.Count == 1)
                            {
                                this.model.RegionID = FloorList[0].ChooseID;
                                this.model.RegionName = FloorList[0].ChooseName;
                            }

                            break;
                        case "Time":
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                DownModal model = new DownModal();
                DownModalViewModel modelModel = new DownModalViewModel(model.Close);
                modelModel.Model.ChooseList = list;
                modelModel.Model.Title = title;
                model.DataContext = modelModel;
                model.ShowDialog();
                if (modelModel.ChooseItem != null)
                {
                    switch (sender)
                    {
                        case "Floor":
                            this.model.RegionID = modelModel.ChooseItem.ChooseID;
                            this.model.RegionName = modelModel.ChooseItem.ChooseName;
                            break;
                        case "DocumentType":
                            this.model.InvType = modelModel.ChooseItem.ChooseID;
                            this.model.InvTypeName = modelModel.ChooseItem.ChooseName;
                            if (this.model.InvType == 1 || this.model.InvType == 2)
                            {
                                this.model.HotelVisbility = Visibility.Visible;
                            }
                            else
                            {
                                this.model.HotelVisbility = Visibility.Hidden;
                                this.model.RegionID = 0;
                                this.model.HotelId = 0;
                                this.model.RegionName = "";
                                this.model.HotelName = "";
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        //public void chooseAttrAction(object paramte)
        //{
        //    ObservableCollection<ChooseModel> list = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();
        //    list.Add(new ChooseModel() { ChooseID = 1, ChooseName = "污物送洗单" });
        //    list.Add(new ChooseModel() { ChooseID = 2, ChooseName = "净物配送单" });
        //    list.Add(new ChooseModel() { ChooseID = 22, ChooseName = "净物入库" });
        //    list.Add(new ChooseModel() { ChooseID = 21, ChooseName = "回厂送洗" });
        //    list.Add(new ChooseModel() { ChooseID = 24, ChooseName = "洗涤入库" });
        //    list.Add(new ChooseModel() { ChooseID = 20, ChooseName = "净物出库" });
        //    list.Add(new ChooseModel() { ChooseID = 4, ChooseName = "成品出库" });
        //    list.Add(new ChooseModel() { ChooseID = 50, ChooseName = "污物复核" });
        //    list.Add(new ChooseModel() { ChooseID = 51, ChooseName = "污物入厂" });
        //    list.Add(new ChooseModel() { ChooseID = 6, ChooseName = "订单" });
        //    list.Add(new ChooseModel() { ChooseID = 7, ChooseName = "退回" });

        //    string title = "单据查询";
        //    DownModal model = new DownModal();
        //    DownModalViewModel modelModel = new DownModalViewModel(model.Close);
        //    modelModel.Model.ChooseList = list;
        //    modelModel.Model.Title = title;
        //    model.DataContext = modelModel;
        //    model.ShowDialog();
        //    if (modelModel.ChooseItem != null)
        //    {
        //        this.model.InvType = modelModel.ChooseItem.ChooseID;
        //        this.model.InvTypeName = modelModel.ChooseItem.ChooseName;
        //    }

        //}

        private RelayCommand<object> close;
        public ICommand Close
        {
            get
            {
                if (close == null)
                {
                    close = new RelayCommand<object>(CloseAction);
                }
                return close;
            }
        }
        public void CloseAction(object parameter)
        {
            App.Current.Dispatcher.Invoke(() => { _closeAction.Invoke(); });
        }

        public BillInquiry02ViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            model = new BillInquiry02Model()
            {
                IsEnabled = "False",
                CreateTime = DateTime.Now.ToString("yyyy-MM-dd"),
                WaitVisibled = "Hidden",
                HotelVisbility = Visibility.Hidden
            };
            model.ResetTable = new ObservableCollection<resposparameter>();
            BagList = new List<string>();
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();
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
            col.ColumnName = "ProductName";
            col.DefaultValue = string.Empty;
            _printTable.Columns.Add(col);

            col = new DataColumn();
            col.DataType = typeof(string);
            col.ColumnName = "Size";
            col.DefaultValue = string.Empty;
            _printTable.Columns.Add(col);

            col = new DataColumn();
            col.DataType = typeof(int);
            col.ColumnName = "Number";
            col.DefaultValue = 0;
            _printTable.Columns.Add(col);

            return _printTable;
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

            AllRegionList = new List<ResponseRegionModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RegionList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    AllRegionList = JsonConvert.DeserializeObject<List<ResponseRegionModel>>(apiRtn.Result.ToString());
                }
            }

            Prepare = true;
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}
