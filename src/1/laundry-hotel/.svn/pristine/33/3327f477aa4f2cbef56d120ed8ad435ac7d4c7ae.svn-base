using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Utilities.PrintBase;
using ETextsys.Terminal.View.Choose;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Choose
{
    public class ChooseTaskViewModel : ViewModelBase
    {
        Action _closeAction;

        private ChooseTaskModel model;
        public ChooseTaskModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        #region 打印任务参数

        /// <summary>
        /// 任务项明细列表
        /// </summary>
        public List<ResponseSendTaskModel> SendTaskItems { get; set; }

        /// <summary>
        /// 酒店
        /// </summary>
        public Region Hotel { get; set; }

        /// <summary>
        /// 楼层
        /// </summary>
        public Region Region { get; set; }

        #endregion

        /// <summary>
        /// 一页显示条数
        /// </summary>
        private int PageCount = 16;

        /// <summary>
        /// 当前页码
        /// </summary>
        private int CurrentPageIndex = 1;

        /// <summary>
        /// 总条数
        /// </summary>
        private int PageTotal = 0;

        /// <summary>
        /// 总页码
        /// </summary>
        private int PageIndexTotal = 0;

        public ChooseTaskViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new ChooseTaskModel();
            this.model.ChooseList = new System.Collections.ObjectModel.ObservableCollection<Terminal.Model.Choose.ChooseModel>();
        }

        public void RefreshPage()
        {
            PageTotal = this.model.AllChooseList.Count;

            PageIndexTotal = PageTotal % PageCount == 0 ? PageTotal / PageCount : (PageTotal / PageCount) + 1;

            var query = this.model.AllChooseList.Skip((CurrentPageIndex - 1) * PageCount).Take(PageCount);

            if (CurrentPageIndex < PageIndexTotal)
            {
                this.model.RightEnabled = true;
            }
            else
            {
                this.model.RightEnabled = false;
            }

            if (CurrentPageIndex > 1)
            {
                this.model.LeftEnabled = true;
            }
            else
            {
                this.model.LeftEnabled = false;
            }

            this.model.ChooseList.Clear();
            ChooseModel chooseModel = null;
            query.ToList().ForEach(q =>
            {
                chooseModel = new ChooseModel();
                chooseModel.ChooseID = q.ChooseID;
                chooseModel.ChooseName = q.ChooseName;
                chooseModel.IsChoose = q.IsChoose;
                chooseModel.BasisOnSend = q.BasisOnSend;
                chooseModel.HotelID = q.HotelID;
                chooseModel.RegionID = q.RegionID;
                chooseModel.HotelName = q.HotelName;
                chooseModel.RegionName = q.RegionName;
                chooseModel.RegionMode = q.RegionMode;
                chooseModel.TaskCount = q.TaskCount;
                this.model.ChooseList.Add(chooseModel);
            });

            int cnt = model.AllChooseList.Where(c => c.IsChoose == 1).Count();
            if (cnt > 0)
            {
                model.SubmitEnabled = true;
                model.PrintEnabled = true;
            }
            else
            {
                model.SubmitEnabled = false;
                model.PrintEnabled = false;
            }
        }

        #region Action

        private void SubmitAction(object sender)
        {
            IEnumerable<ChooseModel> enumer = model.AllChooseList.Where(c => c.IsChoose == 1);
            int cnt = enumer.Count();
            if (cnt > 1)
            {
                int result = enumer.GroupBy(c => c.BasisOnSend).Count();
                if (result > 1)
                {
                    EtexsysMessageBox.Show("提示", "来自订单和收货的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                result = enumer.GroupBy(c => c.RegionID).Count();
                if (result > 1)
                {
                    EtexsysMessageBox.Show("提示", "来自不同酒店(或者楼层)的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }                
            }
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void PrintAction(object sender)
        {
            IEnumerable<ChooseModel> ienumerable = model.AllChooseList.Where(c => c.IsChoose == 1);

            int cnt = ienumerable.Count();
            if (cnt > 1)
            {
                int result = ienumerable.GroupBy(c => c.BasisOnSend).Count();
                if (result > 1)
                {
                    EtexsysMessageBox.Show("提示", "来自订单和收货的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                result = ienumerable.GroupBy(c => c.RegionID).Count();
                if (result > 1)
                {
                    EtexsysMessageBox.Show("提示", "来自不同酒店(或者楼层)的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (ienumerable != null && ienumerable.Count() > 0
                && SendTaskItems != null && SendTaskItems.Count > 0)
            {
                PrintQueue pq = new PrintQueue();
                PrintAttachment att = null;

                int _printPaper = ConfigController.SystemSettingConfig.SelectPrintPaper;

                List<TableColumnHeaderModel> tableAttr = GetPrintTableAttr();
                DataTable _printTable = CreatePrintTable();

                List<ResponseSendTaskModel> allsendtasks = new List<ResponseSendTaskModel>();

                List<string> times = ienumerable.Select(c => c.ChooseName).ToList();
                string time = times.Aggregate((a, b) => a + "、" + b);

                int basisOnSend, regionMode, hotelid, regionid;
                basisOnSend = regionMode = hotelid = regionid = 0;
                var q = from t in ienumerable
                        group t by new
                        {
                            t.BasisOnSend,
                            t.RegionMode,
                            t.HotelID,
                            t.RegionID,
                        } into m
                        select new
                        {
                            m.Key.BasisOnSend,
                            m.Key.RegionMode,
                            m.Key.HotelID,
                            m.Key.RegionID,
                        };

                var list = q.ToList();
                if (list.Count > 0)
                {
                    basisOnSend = list.FirstOrDefault().BasisOnSend;
                    regionMode = Convert.ToInt32(list.FirstOrDefault().RegionMode);
                    hotelid = list.FirstOrDefault().HotelID;
                    regionid = list.FirstOrDefault().RegionID;
                }

                IEnumerable<ResponseSendTaskModel> sendtasks = null;
                ienumerable.ToList().ForEach(v =>
                {
                    sendtasks = SendTaskItems.Where(c => c.TaskTime.Equals(v.ChooseName) && c.TaskType == basisOnSend);

                    if (regionMode == 1)
                    {
                        sendtasks = sendtasks.Where(c => c.HotelID == hotelid);
                    }
                    else
                    {
                        sendtasks = sendtasks.Where(c => c.RegionID == regionid);
                    }

                    allsendtasks.AddRange(sendtasks);
                });

                var query = from t in allsendtasks
                            group t by new
                            {
                                t1 = t.ClassName,
                                t2 = t.SizeName,
                                t3 = t.ClassSort,
                                t4 = t.SizeSort,
                            } into m
                            orderby m.Key.t3, m.Key.t1, m.Key.t4, m.Key.t2
                            select new
                            {
                                m.Key.t1,
                                m.Key.t2,
                                TaskCount = m.Sum(c => c.TaskCount),
                            };

                int total = 0;
                DataRow row = null;
                query.ToList().ForEach(x =>
                {
                    row = _printTable.NewRow();

                    row["ClassName"] = x.t1;
                    row["SizeName"] = x.t2;
                    row["Count"] = x.TaskCount;

                    total += x.TaskCount;

                    _printTable.Rows.Add(row);
                });

                att = new PrintAttachment();
                att.Title = "配送任务单";
                att.PrintTime = time;
                att.HandlerName = App.CurrentLoginUser.UName;
                att.CustomerName = Hotel != null ? Hotel.RegionName : "";
                att.RegionName = Region != null ? Region.RegionName : "";
                att.Total = total;
                att.PrintType = 1;
                att.PaperType = _printPaper;
                att.TableColumns = tableAttr;
                att.PrintDataTable = _printTable;
                pq.Add(att);

                pq.Print();
            }
            else
            {
                EtexsysMessageBox.Show("提示", "本次打印失败，请重新选择后再操作.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseModalAction(string sender)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void SelectedItemAction(int id)
        {
            ChooseModel chooseModel = model.ChooseList.SingleOrDefault(c => c.ChooseID == id);
            if (chooseModel != null)
            {
                chooseModel.IsChoose = chooseModel.IsChoose == 0 ? 1 : 0;
            }

            chooseModel = model.AllChooseList.SingleOrDefault(c => c.ChooseID == id);
            if (chooseModel != null)
            {
                chooseModel.IsChoose = chooseModel.IsChoose == 0 ? 1 : 0;
            }

            IEnumerable<ChooseModel> enumer = model.AllChooseList.Where(c => c.IsChoose == 1);
            int cnt = enumer.Count();
            if (cnt > 0)
            {
                if (cnt > 1)
                {
                    int result = enumer.GroupBy(c => c.BasisOnSend).Count();
                    if (result > 1)
                    {
                        EtexsysMessageBox.Show("提示", "来自订单和收货的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    result = enumer.GroupBy(c => c.RegionID).Count();
                    if (result > 1)
                    {
                        EtexsysMessageBox.Show("提示", "来自不同酒店(或者楼层)的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                var query = enumer.GroupBy(c => new { c.HotelID, c.HotelName, c.RegionID, c.RegionName }).Select(c => new { c.Key.HotelID, c.Key.HotelName, c.Key.RegionID, c.Key.RegionName });
                var list = query.ToList();
                if (list != null && list.Count == 1)
                {
                    var item = list.SingleOrDefault();
                    Hotel = new Region() { ID = item.HotelID, RegionName = item.HotelName };
                    Region = new Region() { ID = item.RegionID, RegionName = item.RegionName };
                }

                model.SubmitEnabled = true;
                model.PrintEnabled = true;
            }
            else
            {
                model.SubmitEnabled = false;
                model.PrintEnabled = false;
            }
        }

        private void LeftAction(object sender)
        {
            CurrentPageIndex--;
            RefreshPage();
        }

        private void RightAction(object sender)
        {
            CurrentPageIndex++;
            RefreshPage();
        }

        #endregion

        #region Command

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

        private RelayCommand<int> _SelectionChangedCmd;
        public ICommand SelectionChangedCmd
        {
            get
            {
                if (_SelectionChangedCmd == null)
                {
                    _SelectionChangedCmd = new RelayCommand<int>(SelectedItemAction);
                }
                return _SelectionChangedCmd;
            }
        }

        private RelayCommand<object> _LeftCommand;
        public ICommand LeftCommand
        {
            get
            {
                if (_LeftCommand == null)
                {
                    _LeftCommand = new RelayCommand<object>(LeftAction);
                }
                return _LeftCommand;
            }
        }

        private RelayCommand<object> _RightCommand;
        public ICommand RightCommand
        {
            get
            {
                if (_RightCommand == null)
                {
                    _RightCommand = new RelayCommand<object>(RightAction);
                }
                return _RightCommand;
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
