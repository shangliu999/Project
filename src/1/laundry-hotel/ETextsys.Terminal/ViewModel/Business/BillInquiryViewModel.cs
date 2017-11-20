﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
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
    public class BillInquiryViewModel
    {
        private BackgroundWorker _worker;
        Action _closeAction;
        private ObservableCollection<ChooseModel> HotelList;
        private ObservableCollection<ChooseModel> FloorList;
        private List<ResponseRegionModel> AllRegionList;
        private bool Prepare;
        private BillInquiryModel model;
        public BillInquiryModel Model
        {
            get { return model; }
            set { model = value; }
        }

        private RelayCommand<object> showDetails;
        public ICommand ShowDetails
        {
            get
            {
                if (showDetails == null)
                {
                    showDetails = new RelayCommand<object>(ShowDetailsAction);
                }
                return showDetails;
            }
        }
        public void ShowDetailsAction(object obj)
        {
            if (obj != null && obj is BillInquiryparamModel)
            {
                BillInquiryparamModel requestModel = obj as BillInquiryparamModel;
                var paramModel = model.ResetTable.Where(p => p.DocumentNumberparam == requestModel.DocumentNumberparam).ToList();
                BillInquiryparamModel parmodel = new BillInquiryparamModel()
                {
                    DocumentNumberparam = paramModel[0].DocumentNumberparam,
                    OneManparam = paramModel[0].OneManparam,
                    Floorparam = paramModel[0].Floorparam,
                    Timeparam = paramModel[0].Timeparam,
                    Hotelparam = paramModel[0].Hotelparam
                };

                BillInquirydetail details = new BillInquirydetail();
                DetailsViewModel detailsViewModel = new DetailsViewModel(details.Close, parmodel, model.InvTypeName, model.HotelName, requestModel.DocumentNumberparam);
                details.DataContext = detailsViewModel;
                details.ContentRendered += detailsViewModel.Details_ContentRendered;
                details.ShowDialog();
            }
        }


        private RelayCommand<string> chooseAttrChanged;
        public ICommand ChooseAttrChanged
        {
            get
            {
                if (chooseAttrChanged == null)
                {
                    chooseAttrChanged = new RelayCommand<string>(chooseAttrAction);
                }
                return chooseAttrChanged;
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
                    title = "单据查询";
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
        public void CloseAction(object obj)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private RelayCommand<DataGrid> selectInvoice { get; set; }
        public ICommand SelectInvoice
        {
            get
            {
                if (selectInvoice == null)
                {
                    selectInvoice = new RelayCommand<DataGrid>(SelectInvoiceAction);
                }
                return selectInvoice;
            }
        }
        public async void SelectInvoiceAction(DataGrid parameter)
        {
            if (model.CreateTime == null)
            {
                EtexsysMessageBox.Show("提示", "请选择日期时间.", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (model.InvType == 0)
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
            if (parameter.Items.Count != 0)
            {
                model.ResetTable.Clear();
            }
            model.WaitVisibled = "Visible";
            model.WaitContent = "玩命加载中";
            BillInquiryParamModel billmodel = new BillInquiryParamModel();
            billmodel.CreateTime = Convert.ToDateTime(model.CreateTime);
            billmodel.HotelId = model.HotelId;
            billmodel.InvType = model.InvType;
            billmodel.RegionID = model.RegionID;
            billmodel.UUID = ConfigController.MacCode;
            billmodel.TimeStamp = ApiController.Instance.GetTimeStamp();
            billmodel.TerminalType = ConfigController.TerminalType;
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.SelectInvoice, billmodel);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var temp = JsonConvert.DeserializeObject<List<ResponseBillInquiry>>(apiRtn.Result.ToString());
                    if (temp != null)
                    {
                        BillInquiryparamModel paramModel = new BillInquiryparamModel();

                        temp.ToList().ForEach(q =>
                        {
                            paramModel = new BillInquiryparamModel();
                            paramModel.DocumentNumberparam = q.InvNo;
                            paramModel.OneManparam = q.CreateUserName;
                            paramModel.OrderQuantityparam = q.Quantity;
                            paramModel.Timeparam = q.CreateTime.ToString("");
                            paramModel.Hotelparam = q.HotelName;
                            paramModel.Floorparam = q.RegionName;
                            this.model.ResetTable.Add(paramModel);
                        });
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
                }
            }
        }

        public BillInquiryViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new BillInquiryModel()
            {
                WaitVisibled = "Hidden",
                IsEnabled = "false",
                CreateTime = DateTime.Now.ToString("yyyy-MM-dd")

            };

            this.model.ResetTable = new ObservableCollection<BillInquiryparamModel>();
            this.model.HotelVisbility = Visibility.Hidden;
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();
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
