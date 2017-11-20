using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.Utilities.PrintBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Choose
{
    public class DetailsViewModel
    {
        private DetailsModel model;
        public DetailsModel Model
        {
            get { return model; }
            set { value = model; }
        }

        private List<ResponseDetail> TextileList;

        BillInquiryparamModel BillparmModel;
        private List<string> BagList;
        int Toatl;

        string InvTypeName;

        string HotelName;

        Action _closeAction;

        public DetailsViewModel(Action action, BillInquiryparamModel parmMode, string invTypeName, string hotelName, string document)
        {
            _closeAction = action;
            model = new DetailsModel();
            model.DocumentNumberparam = document;
            model.CreateTime = parmMode.Timeparam;
            model.Hotel = parmMode.Hotelparam;
            model.Region = parmMode.Floorparam;
            model.CreateUser = parmMode.OneManparam;
            model.LoadingWait = "Hidden";
            BillparmModel = parmMode;
            HotelName = hotelName;
            InvTypeName = invTypeName;
            TextileList = new List<ResponseDetail>();
            model.ReturnTable = new ObservableCollection<ReturnTable>();
            BagList = new List<string>();
            //   1：污物送洗单
            //2：净物配送单
            //3：入库单
            //4：出库单
            //5：入厂
            //6：订单
            //7：退回
            //8：工厂返洗

        }

        public async void Details_ContentRendered(object sender, EventArgs e)
        {
            DetailsParamModel detai = new DetailsParamModel();
            detai.OrderNumber = BillparmModel.DocumentNumberparam;
            detai.TerminalType = ConfigController.TerminalType;
            detai.TimeStamp = ApiController.Instance.GetTimeStamp();
            detai.UUID = ConfigController.MacCode;
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.Details, detai);
            if (apiRtn.Result != null)
            {
                var temp = JsonConvert.DeserializeObject<List<ResponseDetail>>(apiRtn.Result.ToString());
                if (temp != null)
                {
                    TextileList.AddRange(temp);
                }

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
            ReturnTable respos;
            var jquery = from t in TextileList
                         select t;
            int sum = 0;
            jquery.ToList().ForEach(q =>
            {
                respos = new ReturnTable();
                respos.Number = q.Number;
                respos.Size = q.Size;
                respos.Type = q.ClassName;
                model.ReturnTable.Add(respos);
                sum += respos.Number;
            });
            //respos = new ReturnTable();
            //respos.Number = sum;
            //respos.Size = "";
            //respos.Type = "总和";
            //model.ReturnTable.Add(respos);
        }

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

        private RelayCommand<object> closeModal;
        public ICommand CloseModal
        {
            get
            {
                if (closeModal == null)
                {
                    closeModal = new RelayCommand<object>(CloseModalAction);
                }
                return closeModal;
            }
        }
        public void CloseModalAction(object parameter)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { _closeAction.Invoke(); }));
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
            model.WaitContent = "正在打印...";
            model.LoadingWait = "Visible";
            PrintQueue pq = new PrintQueue();
            PrintAttachment att = null;
            int _printCount = ConfigController.BusinessSettingConfig.OtherPrintCount;
            int _printPaper = ConfigController.SystemSettingConfig.SelectPrintPaper;
            List<TableColumnHeaderModel> tableAttr = GetPrintTableAttr();
            DataTable _printTable = CreatePrintTable();

            var query = from t in model.ReturnTable
                        group t by new
                        {
                            t.Type,
                            t.Size
                        } into m
                        select new
                        {
                            croductName = m.Key.Type,
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
                Toatl = q.number + Toatl;
                _printTable.Rows.Add(row);
            });

            for (int i = 0; i < _printCount; i++)
            {
                att = new PrintAttachment();
                att.DocumentNumber = BillparmModel.DocumentNumberparam;
                att.HandlerName = BillparmModel.OneManparam;
                att.RegionName = BillparmModel.Floorparam;
                att.Total = Toatl;
                att.Title = InvTypeName;
                att.PrintTime = BillparmModel.Timeparam.Split(' ')[0];
                att.CustomerName = HotelName;
                //att.HandlerName = App.CurrentLoginUser.UName;
                att.PrintType = 1;
                att.PaperType = _printPaper;
                att.TableColumns = tableAttr;
                att.BagCodes = BagList;
                att.PrintDataTable = _printTable;
                pq.Add(att);
            }
            pq.Print();
            model.LoadingWait = "Visible";
            App.Current.Dispatcher.Invoke((Action)(() => { _closeAction.Invoke(); }));
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

        private List<TableColumnHeaderModel> GetPrintTableAttr()
        {
            int _printPaper = ConfigController.SystemSettingConfig.SelectPrintPaper;
            List<TableColumnHeaderModel> tableAttr = new List<TableColumnHeaderModel>();
            TableColumnHeaderModel tableModel = null;
            if (_printPaper == 1)
            {
                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "品名";
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
                tableModel.Name = "品名";
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

    }
}
