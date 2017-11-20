﻿using ETexsys.APIRequestModel;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace ETextsys.Terminal.ViewModel.Logistics
{
    public class SendViewModel : ViewModelBase, IRFIDScan
    {
        private SendModel model;
        public SendModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        EncodingOptions options = null;
        BarcodeWriter writer = null;

        /// <summary>
        /// 获取选择数据线程
        /// </summary>
        private BackgroundWorker _worker;

        private string GUID;

        /// <summary>
        /// 界面数据是否准备好
        /// </summary>
        private bool Prepare;

        #region 界面待选数据

        private ObservableCollection<ChooseModel> HotelList;
        private ObservableCollection<ChooseModel> RegionList;
        private ObservableCollection<ChooseModel> SendTaskList;

        private List<ResponseHotelModel> AllHotelList;
        private List<ResponseRegionModel> AllRegionList;
        private List<ResponseBrandTypeModel> AllBrandTypeList;
        /// <summary>
        /// 任务明细
        /// </summary>
        private List<ResponseSendTaskModel> AllSendTaskItems;

        /// <summary>
        /// 选择客户的流通类型
        /// </summary>
        private BrandType brandType;

        #endregion

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private List<ResponseRFIDTagModel> TextileList;

        private List<string> ScanList;

        private string InvID;

        /// <summary>
        /// 已配送芯片码
        /// </summary>
        private List<ResponseRFIDTagModel> SendedTagList;

        private string InvNo { get; set; }

        private string CreateTime { get; set; }

        private DateTime ResetTime;

        public SendViewModel(Action closeAction)
        {
            _closeAction = closeAction;
            model = new SendModel();
            model.SendTable = new ObservableCollection<SendTableModel>();
            model.SubmitEnabled = false;
            model.UnDoEnabled = false;
            model.CancelEnabled = true;
            model.BtnReceiveEnabled = false;
            model.WaitVisibled = Visibility.Hidden;
            model.Bags = new ObservableCollection<string>();
            model.ReceivedTotal = 0;

            options = new QrCodeEncodingOptions
            {
                CharacterSet = "UTF-8",
                PureBarcode = true,
                Width = 200,
                Height = 200,
                Margin = 0,
                ErrorCorrection = ErrorCorrectionLevel.H
            };

            SendTaskList = new ObservableCollection<ChooseModel>();
            TextileList = new List<ResponseRFIDTagModel>();
            ScanList = new List<string>();
            SendedTagList = new List<ResponseRFIDTagModel>();
            _isSubmit = false;
            GUID = Guid.NewGuid().ToString();
            ResetTime = DateTime.Now;

            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                model.ReaderState = ConfigController.ReaderConfig.IsConnection ? 2 : 0;
                if (ConfigController.ReaderConfig.IsConnection)
                {
                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                    ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                    {
                        model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                        model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                    });
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

            //HotelList = new ObservableCollection<ChooseModel>();

            AllHotelList = new List<ResponseHotelModel>();
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.HotelList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    AllHotelList = JsonConvert.DeserializeObject<List<ResponseHotelModel>>(apiRtn.Result.ToString());
                    //if (AllHotelList != null)
                    //{
                    //    AllHotelList.OrderBy(v => v.DeliveryTime).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.HotelName).ToList().ForEach(q =>
                    //    {
                    //        HotelList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.HotelName });
                    //    });
                    //}
                }
            }

            //SetHotelList();

            //AllRegionList = new List<ResponseRegionModel>();
            //apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RegionList, null);
            //if (apiRtn.ResultCode == 0)
            //{
            //    if (apiRtn.Result != null)
            //    {
            //        AllRegionList = JsonConvert.DeserializeObject<List<ResponseRegionModel>>(apiRtn.Result.ToString());
            //    }
            //}

            AllBrandTypeList = new List<ResponseBrandTypeModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.BrandTypeList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    AllBrandTypeList = JsonConvert.DeserializeObject<List<ResponseBrandTypeModel>>(apiRtn.Result.ToString());
                }
            }

            AllSendTaskItems = new List<ResponseSendTaskModel>();
            GetSendTask(() =>
            {

            });

            Prepare = true;
        }

        #region RFID Interface

        public void NoScanTag()
        {
            if (model.SendTable.Count > 0 && !_isSubmit)
            {
                this.model.SubmitEnabled = true;
            }
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            var bags = rfidTags.Where(v => v.Type == TagType.Bag).Select(v => v.TagNo).ToList();

            if (bags.Count > 0)
            {
                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = bags.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.BagRFIDTagAnalysis, requestParam);
                if (apiRtn.ResultCode == 0 && apiRtn.Result != null && apiRtn.OtherResult != null)
                {
                    DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                    List<ResponseBagRFIDTagModel> temp = JsonConvert.DeserializeObject<List<ResponseBagRFIDTagModel>>(apiRtn.Result.ToString());
                    if (temp != null && d > ResetTime)
                    {
                        foreach (var item in temp)
                        {
                            if (!model.Bags.Contains(item.BagNo))
                            {
                                model.Bags.Add(item.BagNo);
                            }
                        }
                        if (model.Bags.Count > 0)
                        {
                            model.BagsStr = model.Bags.Aggregate((a, b) => a + "、" + b);
                        }
                    }
                }
            }

            var tag = TextileList.Select(v => v.TagNo).ToList();
            tags = tags.Where(v => !tag.Contains(v)).ToList();

            if (tags.Count > 0)
            {
                model.SubmitEnabled = false;
                model.CancelEnabled = true;
                model.UnDoEnabled = true;
                
                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = tags.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.ComplexRFIDTagAnalysis, requestParam);
                if (apiRtn.ResultCode == 0 && apiRtn.Result != null && apiRtn.OtherResult != null)
                {
                    DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                    List<ResponseRFIDTagModel> temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                    if (temp != null && d > ResetTime)
                    {
                        ScanList.AddRange(tags);

                        List<string> tagList = TextileList.Select(v => v.TagNo).ToList();
                        temp = temp.Where(v => !tagList.Contains(v.TagNo)).ToList();
                        TextileList.AddRange(temp);

                        tag = TextileList.Where(v => !ScanList.Contains(v.TagNo)).Select(v => v.TagNo).ToList();
                        ScanList.AddRange(tag);

                        TextileList = TextileList.Where((x, y) => TextileList.FindIndex(z => z.TagNo == x.TagNo) == y).ToList();

                        //只加载最近一次扫描到的芯片
                        SendTableModel sendModel = null;
                        var query = from t in temp
                                    where t.TextileState != 2
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

                        query.ToList().ForEach(q =>
                        {
                            string sizeName = q.sizeName == null ? "" : q.sizeName;

                            SendTableModel stm = null;
                            if (!string.IsNullOrEmpty(q.sizeName))
                            {
                                stm = model.SendTable.SingleOrDefault(c => c.ClassName == q.className && c.SizeName == sizeName);
                            }
                            else
                            {
                                stm = model.SendTable.SingleOrDefault(c => c.ClassName == q.className);
                            }

                            if (stm != null)
                            {
                                stm.TextileCount = stm.TextileCount + q.count;
                                stm.DiffCount = stm.DiffCount + q.count;
                            }
                            else
                            {
                                sendModel = new SendTableModel();

                                sendModel.BrandName = q.brandName;
                                sendModel.ClassName = q.className;
                                sendModel.SizeName = q.sizeName;
                                sendModel.TaskCount = 0;
                                sendModel.TextileCount = q.count;
                                sendModel.DiffCount = q.count;

                                model.SendTable.Add(sendModel);
                            }
                        });
                    }

                    SendedTagList = TextileList.Where(v => v.TextileState == 2).ToList();

                    int total = 0;
                    if (model.SendTable != null && model.SendTable.Count > 0)
                    {
                        total = model.SendTable.Sum(c => c.TextileCount);
                    }
                    model.UnRegisterTotal = ScanList.Count - total - SendedTagList.Count;
                    model.TextileCount = total;
                    model.ReceivedTotal = SendedTagList.Count;
                    model.BtnReceiveEnabled = SendedTagList.Count > 0 ? true : false;
                }
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

            ObservableCollection<ChooseModel> list = null;
            string title = "";
            switch (sender)
            {
                case "Hotel":
                    HotelList = new ObservableCollection<ChooseModel>();
                    SetHotelList();

                    list = HotelList;
                    title = "酒店";
                    break;
                case "Region":
                    if (model.Hotel != null && model.Hotel.ID != 0)
                    {
                        list = RegionList;
                    }
                    else
                    {
                        EtexsysMessageBox.Show("提示", "请先选择楼层.", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    title = "楼层";
                    break;
                case "Task":
                    list = SendTaskList;
                    title = "配送依据";
                    break;
                case "Bag":
                    title = "请选择包:";
                    break;
                default:
                    return;
            }

            if (sender == "Hotel")
            {
                #region 酒店选择

                ChooseHotel hotel = new ChooseHotel();
                ChooseHotelViewModel hotelViewModal = new Choose.ChooseHotelViewModel(hotel.Close);
                hotelViewModal.Model.AllChooseList = list;
                hotelViewModal.Model.Title = title;
                hotelViewModal.RefreshPage();

                hotel.DataContext = hotelViewModal;
                hotel.ShowDialog();
                if (hotelViewModal.ChooseItem != null)
                {
                    model.Hotel = new Region() { ID = hotelViewModal.ChooseItem.ChooseID, RegionName = hotelViewModal.ChooseItem.ChooseName };
                    model.Region = new Region();

                    int regionmode = -1;
                    var rquery = from r in AllHotelList
                                 join b in AllBrandTypeList on r.BrandID equals b.ID
                                 where r.ID == model.Hotel.ID
                                 select new
                                 {
                                     r.RegionMode,
                                     b.ID,
                                     b.BrandName,
                                 };
                    var region = rquery.SingleOrDefault();
                    if (region != null)
                    {
                        regionmode = region.RegionMode;
                        brandType = new BrandType() { ID = region.ID, Name = region.BrandName };
                    }
                    model.Hotel.RegionMode = regionmode;

                    int id = 0;
                    if (regionmode == 1)
                    {
                        model.RegionVisibility = Visibility.Hidden;
                        id = model.Hotel.ID;
                    }
                    else
                    {
                        model.RegionVisibility = Visibility.Visible;

                        RegionList = new ObservableCollection<ChooseModel>();
                        var query = from t in AllRegionList where t.HotelID == model.Hotel.ID select new { regionName = t.RegionName, sort = t.Sort, id = t.ID };

                        query.OrderBy(v => v.sort).ThenBy(v => v.regionName).ToList().ForEach(q =>
                        {
                            RegionList.Add(new ChooseModel { ChooseID = q.id, ChooseName = q.regionName });
                        });

                        if (RegionList.Count == 1)
                        {
                            model.Region = new Region() { ID = RegionList[0].ChooseID, RegionName = RegionList[0].ChooseName };

                            id = model.Region.ID;
                        }
                    }
                    SendTaskList.Clear();
                    model.SendTaskStr = "";
                    //清除界面
                    ClearForm();
                    if (id != 0)
                    {
                        //获取任务列表
                        GetSendTask(id, regionmode);
                    }
                }

                #endregion
            }
            else if (sender == "Region")
            {
                DownModal modal = new DownModal();
                DownModalViewModel modalModel = new DownModalViewModel(modal.Close);
                modalModel.Model.ChooseList = list;
                modalModel.Model.Title = title;
                modal.DataContext = modalModel;
                modal.ShowDialog();

                if (modalModel.ChooseItem != null)
                {
                    model.Region = new Region() { ID = modalModel.ChooseItem.ChooseID, RegionName = modalModel.ChooseItem.ChooseName };

                    GetSendTask(modalModel.ChooseItem.ChooseID, 2);

                    model.SendTaskStr = "";

                    //清除界面
                    ClearForm();
                }
            }
            else if (sender == "Task")
            {
                ChooseTask chooseTask = new ChooseTask();
                ChooseTaskViewModel chooseTaskViewModel = new ChooseTaskViewModel(chooseTask.Close);
                chooseTaskViewModel.Model.AllChooseList = list;
                chooseTaskViewModel.Model.Title = title;

                //任务打印
                chooseTaskViewModel.SendTaskItems = AllSendTaskItems;
                chooseTaskViewModel.Hotel = model.Hotel;
                chooseTaskViewModel.Region = model.Region;

                chooseTaskViewModel.RefreshPage();

                chooseTask.DataContext = chooseTaskViewModel;
                chooseTask.ShowDialog();

                IEnumerable<ChooseModel> list1 = chooseTaskViewModel.Model.AllChooseList.Where(c => c.IsChoose == 1);
                if (list1 != null && list1.Count() > 0)
                {
                    if (list1.GroupBy(c => c.BasisOnSend).Count() > 1)
                    {
                        model.SendTaskStr = "";
                        chooseTaskViewModel.Model.AllChooseList.ToList().ForEach(q => { q.IsChoose = 0; });
                        EtexsysMessageBox.Show("提示", "来自订单和收货的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (list1.GroupBy(c => c.RegionID).Count() > 1)
                    {
                        model.SendTaskStr = "";
                        chooseTaskViewModel.Model.AllChooseList.ToList().ForEach(q => { q.IsChoose = 0; });
                        EtexsysMessageBox.Show("提示", "来自不同酒店(或者楼层)的任务，请分开选择.", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    IEnumerable<string> ienumerable = list1.Select(c => c.ChooseName);
                    model.SendTaskStr = ienumerable.Aggregate((a, b) => a + "、" + b);

                    ClearForm();

                    InitDatagrid(list1);
                }
            }
            else if (sender == "Bag")
            {
                ChooseBag bag = new ChooseBag();
                ChooseBagViewModel bagViewModal = new ChooseBagViewModel(bag.Close);
                bagViewModal.Model.Title = title;
                for (int i = 0; i < model.Bags.Count; i++)
                {
                    bagViewModal.Model.AllChooseList.Add(new ChooseModel() { ChooseName = model.Bags[i] });
                }
                bag.ContentRendered += bagViewModal.Bag_ContentRendered;
                bag.DataContext = bagViewModal;
                bag.ShowDialog();

                model.Bags.Clear();
                model.BagsStr = "";
                if (bagViewModal.Model.AllChooseList != null && bagViewModal.Model.AllChooseList.Count > 0)
                {
                    bagViewModal.Model.AllChooseList.ToList().ForEach(c =>
                    {
                        model.Bags.Add(c.ChooseName);
                        model.BagsStr = model.Bags.Aggregate((a, b) => a + "、" + b);
                    });
                }
            }
            else
            {
                return;
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
            if (model.Hotel == null || model.Hotel.ID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择酒店.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (model.Hotel.RegionMode == 2 && model.Region.ID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择楼层.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SendTaskList.Where(c => c.IsChoose == 1).Count() == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择配送依据.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int total = model.SendTable.Sum(c => c.TextileCount);
            if (total == 0)
            {
                EtexsysMessageBox.Show("提示", "请扫描纺织品.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IEnumerable<ResponseRFIDTagModel> list = TextileList.Where(c => c.TextileState != 2 && c.BrandID != brandType.ID);
            if (list.Count() > 0)
            {
                EtexsysMessageBox.Show("提示", "净物配货不允许窜流通.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            #region 判断超欠

            bool isMore = false, isLess = false;
            SendTableModel stm = null;
            for (int i = 0; i < model.SendTable.Count; i++)
            {
                stm = model.SendTable[i];
                if (stm.DiffCount > 0)
                {
                    isMore = true;
                    break;
                }
                else if (stm.DiffCount < 0)
                {
                    isLess = true;
                }
            }
            if (isMore)
            {
                EtexsysMessageBox.Show("提示", "净物配送不允许超发.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isLess)
            {
                bool? isOK = EtexsysMessageBox.Show("提示", "存在欠发的纺织品，确定提交吗？", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (isOK == false) return;
            }

            #endregion

            _isSubmit = true;
            model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            model.WaitVisibled = Visibility.Visible;
            model.WaitContent = "正在玩命提交...";
            ResetTime = DateTime.Now;

            ScanList.RemoveAll(v => SendedTagList.Select(c => c.TagNo).Contains(v));

            RFIDInvoiceParamModel requestParam = new RFIDInvoiceParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.HotelID = this.model.Hotel.ID;
            requestParam.RegionID = this.model.Region.ID;
            requestParam.Quantity = this.model.TextileCount;
            requestParam.InvType = (short)EnumInvType.Send;
            requestParam.InvSubType = 0;
            requestParam.Tags = ScanList.ToArray();
            requestParam.CreateUserID = App.CurrentLoginUser.UserID;
            requestParam.CreateUserName = App.CurrentLoginUser.UName;
            requestParam.GUID = GUID;

            List<ChooseModel> selecttask = SendTaskList.Where(c => c.IsChoose == 1).ToList();
            requestParam.TaskType = selecttask.FirstOrDefault().BasisOnSend;

            List<AttchModel> attchModels = new List<AttchModel>();
            for (int i = 0; i < selecttask.Count; i++)
            {
                attchModels.Add(new AttchModel() { Type = "TaskDate", Value = selecttask[i].ChooseName });
            }
            for (int i = 0; i < model.Bags.Count; i++)
            {
                attchModels.Add(new AttchModel() { Type = "Bag", Value = model.Bags[i] });
            }
            requestParam.Attach = attchModels;

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.InsertRFIDInvoice, requestParam);

            this.model.WaitVisibled = Visibility.Hidden;
            if (rtn.ResultCode == 1)
            {
                model.SubmitEnabled = true;
                EtexsysMessageBox.Show("提示", rtn.ResultMsg, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (rtn.Result == null)
                {
                    this.model.SubmitEnabled = true;
                    EtexsysMessageBox.Show("提示", "污物送洗失败", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    //单据打印时，会用到
                    string[] r = rtn.Result.ToString().Split('|');
                    InvNo = r[0];
                    CreateTime = r[1];
                    InvID = r[2];

                    model.PrintEnabled = true;
                    model.UnDoEnabled = false;

                    Int64 time = Convert.ToInt64(ApiController.Instance.GetTimeStamp());
                    using (LaundryContext laundryContext = new LaundryContext())
                    {
                        laundryContext.SendLogs.Add(new SendLog() { RegionID = model.Hotel.ID, RegionName = model.Hotel.RegionName, CreateTime = time });
                        laundryContext.SaveChanges();
                    }

                    EtexsysMessageBox.Show("提示", "净物配送成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            //提交成功后，重新加载任务
            Prepare = false;
            GetSendTask(() =>
            {
                Prepare = true;
            });
        }

        private void CancelAction(object sender)
        {
            if (ReaderController.Instance != null && ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            ResetTime = DateTime.Now;
            TextileList.Clear();
            SendedTagList.Clear();
            model.SubmitEnabled = false;
            model.BtnReceiveEnabled = false;
            model.UnDoEnabled = false;
            model.UnRegisterTotal = 0;
            model.TextileCount = 0;
            model.ReceivedTotal = 0;
            model.SendTable.Clear();
            ScanList.Clear();
            model.BagsStr = "";
            model.Bags.Clear();

            InvNo = string.Empty;
            CreateTime = string.Empty;

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

            var stl = SendTaskList.Where(c => c.IsChoose == 1);
            InitDatagrid(stl);

            if (_isSubmit)
            {
                _isSubmit = false;

                model.PrintEnabled = false;
                model.Hotel = new Region();
                model.Region = new Region();
                SendTaskList.Clear();
                model.SendTaskStr = "";
                //清除界面
                ClearForm();

                Prepare = false;
                GetSendTask(() =>
                {
                    Prepare = true;
                });

                GUID = Guid.NewGuid().ToString(); 
            }
        }

        private void PrintAction(object sender)
        {
            if (!string.IsNullOrEmpty(InvNo) && InvNo.Length > 5)
            {
                PrintQueue pq = new PrintQueue();
                PrintAttachment att = null;

                int _printCount = ConfigController.BusinessSettingConfig.SendPrintCount;
                int _printPaper = ConfigController.SystemSettingConfig.SelectPrintPaper;
                List<TableColumnHeaderModel> tableAttr = GetPrintTableAttr();
                DataTable _printTable = CreatePrintTable();

                SendTableModel stm = null;
                DataRow row = null;
                for (int i = 0; i < model.SendTable.Count; i++)
                {
                    stm = model.SendTable[i];

                    row = _printTable.NewRow();

                    row["ClassName"] = stm.ClassName;
                    row["SizeName"] = stm.SizeName;
                    row["TaskCount"] = stm.TaskCount;
                    row["Count"] = stm.TextileCount;

                    _printTable.Rows.Add(row);
                }
                List<string> bags = null;
                if (model.Bags != null && model.Bags.Count > 0)
                {
                    bags = model.Bags.ToList();
                }
                writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.QR_CODE;
                writer.Options = options;
                string wxurl = System.Configuration.ConfigurationManager.AppSettings["InvoiceUrl"] + "?id=" + InvID + "&scode=";
                for (int i = 0; i < _printCount; i++)
                {
                    att = new PrintAttachment();
                    att.FirstQRImage = writer.Write(wxurl);
                    att.Title = "净物配送单";
                    att.PrintTime = CreateTime;
                    att.DocumentNumber = InvNo;
                    att.HandlerName = App.CurrentLoginUser.UName;
                    att.CustomerName = model.Hotel.RegionName;
                    att.RegionName = model.Region.RegionName;
                    att.Total = model.TextileCount;
                    att.PrintType = 1;
                    att.PaperType = _printPaper;
                    att.TableColumns = tableAttr;
                    att.PrintDataTable = _printTable;
                    att.BagCodes = bags;
                    pq.Add(att);
                }
                pq.Print();
            }
            else
            {
                EtexsysMessageBox.Show("提示", "本次配送失败，请重新操作.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UndoAction1(object sender)
        {
            UnDoModal modal = new UnDoModal();
            UnDoViewModel viewmodel = new UnDoViewModel(modal.Close);
            viewmodel.SendTable = model.SendTable;
            modal.DataContext = viewmodel;

            //停止读写器
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                //model.ReaderState = ConfigController.ReaderConfig.IsConnection ? 2 : 0;
                if (ConfigController.ReaderConfig.IsConnection)
                {
                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                    ReaderController.Instance.ScanUtilities.StopScan();
                    System.Threading.Thread.Sleep(50);
                    model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 2;
                    model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri("../../Skins/Default/Images/scan.gif", UriKind.RelativeOrAbsolute) : new Uri("../../Skins/Default/Images/stopscan.gif", UriKind.RelativeOrAbsolute);
                }
            }

            modal.ShowDialog();

            if (viewmodel.IsSubmit == true)
            {
                //返回回来之后可以继续扫描已撤销的芯片
                ReaderController.Instance.ScanUtilities.NewList.Clear();
                ReaderController.Instance.ScanUtilities.UidList.Clear();

                List<TagModel> newList = new List<TagModel>();
                //返回被删除的字数
                int cnt = ScanList.RemoveAll(c => viewmodel.TextileList.Select(v => v.TagNo).Contains(c));

                if (cnt > 0)
                {
                    //newList.AddRange(ScanList);
                    foreach (var t in ScanList)
                    {
                        newList.Add(new TagModel { TagNo = t, Type = TagType.Textile });
                    }

                    ScanList.Clear();
                    TextileList.Clear();

                    var stl = SendTaskList.Where(c => c.IsChoose == 1);
                    InitDatagrid(stl);

                    ScanNew(newList);
                }
                else
                {
                    EtexsysMessageBox.Show("提示", "没有扫描到已配送纺织品.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SendAction(object sender)
        {
            ExecutedModal modal = new ExecutedModal();
            ExecutedViewModel viewModel = new ExecutedViewModel(modal.Close);
            viewModel.Model.Title = "重复配货";
            viewModel.Model.TagList = this.SendedTagList;
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

        private RelayCommand<object> _UndoCommand;
        public ICommand UndoCommand
        {
            get
            {
                if (_UndoCommand == null)
                {
                    _UndoCommand = new RelayCommand<object>(UndoAction1);
                }
                return _UndoCommand;
            }
        }

        private RelayCommand<object> _SendCommand;
        public ICommand SendCommand
        {
            get
            {
                if (_SendCommand == null)
                {
                    _SendCommand = new RelayCommand<object>(SendAction);
                }
                return _SendCommand;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取任务明细列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="regionmode"></param>
        private async void GetSendTask(int id, int regionmode)
        {
            SendTaskParamModel requestParam = new SendTaskParamModel();
            requestParam.ID = id;
            requestParam.RegionMode = regionmode;
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;

            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.SendTask, requestParam);
            if (apiRtn.ResultCode == 0 && apiRtn.Result != null)
            {
                SendTaskList.Clear();

                AllSendTaskItems = JsonConvert.DeserializeObject<List<ResponseSendTaskModel>>(apiRtn.Result.ToString());
                if (AllSendTaskItems != null)
                {
                    var query = from t in AllSendTaskItems
                                group t by new { t1 = t.TaskTime, t2 = t.TaskType } into m
                                orderby m.Key.t1, m.Key.t2
                                select new
                                {
                                    m.Key.t1,
                                    m.Key.t2,
                                    TextileCount = m.Sum(p => p.TaskCount - p.CheckCount),
                                };

                    for (int i = 0; i < query.Count(); i++)
                    {
                        SendTaskList.Add(new ChooseModel()
                        {
                            ChooseID = i,
                            ChooseName = query.ElementAt(i).t1,
                            IsChoose = 0,
                            BasisOnSend = query.ElementAt(i).t2
                        });
                    }
                }
            }
        }

        private async void GetSendTask(Action action)
        {
            SendTaskParamModel requestParam = new SendTaskParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;

            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.SendTask1, requestParam);
            if (apiRtn.ResultCode == 0 && apiRtn.Result != null)
            {
                AllSendTaskItems = JsonConvert.DeserializeObject<List<ResponseSendTaskModel>>(apiRtn.Result.ToString());

                SendTaskList.Clear();
                if (AllSendTaskItems != null)
                {
                    var query = from t in AllSendTaskItems
                                group t by new
                                {
                                    t1 = t.TaskTime,
                                    t2 = t.TaskType,
                                    t3 = t.HotelID,
                                    t4 = t.RegionMode == 1 ? 0 : t.RegionID,
                                    t5 = t.HotelName,
                                    t6 = t.RegionMode == 1 ? "" : t.RegionName,
                                    t7 = t.RegionMode,
                                } into m
                                orderby m.Key.t1 descending, m.Key.t2, m.Key.t4 descending
                                select new
                                {
                                    m.Key.t1,
                                    m.Key.t2,
                                    m.Key.t3,
                                    m.Key.t4,
                                    m.Key.t5,
                                    m.Key.t6,
                                    m.Key.t7,
                                    TextileCount = m.Sum(p => p.TaskCount - p.CheckCount),
                                };

                    int i = 0;
                    query.ToList().ForEach(c =>
                    {
                        ChooseModel cm = new ChooseModel();

                        cm.ChooseID = i;
                        cm.ChooseName = c.t1;
                        cm.BasisOnSend = c.t2;
                        cm.HotelID = c.t3;
                        cm.RegionID = c.t4;
                        cm.HotelName = c.t5;
                        cm.RegionName = c.t6;
                        cm.RegionMode = c.t7;
                        cm.TaskCount = c.TextileCount;

                        SendTaskList.Add(cm);

                        i++;
                    });

                    action();
                }
            }
        }

        /// <summary>
        /// 选择任务加载表格
        /// </summary>
        /// <param name="ienumerable"></param>
        private void InitDatagrid(IEnumerable<ChooseModel> ienumerable)
        {
            model.SendTable.Clear();

            List<ResponseSendTaskModel> allsendtasks = new List<ResponseSendTaskModel>();

            int basisOnSend, regionMode, hotelid, regionid;
            basisOnSend = regionMode = hotelid = regionid = 0;
            var q = from t in ienumerable
                    group t by new
                    {
                        t.BasisOnSend,
                        t.RegionMode,
                        t.HotelID,
                        t.HotelName,
                        t.RegionID,
                        t.RegionName,
                    } into m
                    select new
                    {
                        m.Key.BasisOnSend,
                        m.Key.RegionMode,
                        m.Key.HotelID,
                        m.Key.HotelName,
                        m.Key.RegionID,
                        m.Key.RegionName,
                    };

            var list = q.ToList();
            if (list.Count > 0)
            {
                basisOnSend = list.FirstOrDefault().BasisOnSend;
                regionMode = Convert.ToInt32(list.FirstOrDefault().RegionMode);
                hotelid = list.FirstOrDefault().HotelID;
                regionid = list.FirstOrDefault().RegionID;

                //根据选择的任务加载对应的酒店、流通、楼层信息
                model.Hotel = new Region() { ID = hotelid, RegionName = list.FirstOrDefault().HotelName };

                ResponseHotelModel rhm = AllHotelList.FirstOrDefault(c => c.ID == hotelid);
                ResponseBrandTypeModel rbtm = AllBrandTypeList.FirstOrDefault(c => c.ID == rhm.BrandID);
                brandType = new BrandType() { ID = rbtm.ID, Name = rbtm.BrandName, Sort = rbtm.Sort };

                model.Region = new Region() { ID = regionid, RegionName = list.FirstOrDefault().RegionName };
            }

            IEnumerable<ResponseSendTaskModel> sendtasks = null;
            ienumerable.ToList().ForEach(v =>
            {
                sendtasks = AllSendTaskItems.Where(c => c.TaskTime.Equals(v.ChooseName) && c.TaskType == basisOnSend);

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

            query.ToList().ForEach(x =>
            {
                model.SendTable.Add(new SendTableModel() { BrandName = brandType.Name, ClassName = x.t1, SizeName = x.t2, TaskCount = x.TaskCount, DiffCount = -x.TaskCount });
            });
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
                tableModel.Width = 40;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "任务";
                tableModel.Alignment = ColumnHAlignment.Right;
                tableModel.Width = 35;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "数量";
                tableModel.Alignment = ColumnHAlignment.Right;
                tableModel.Width = 35;
                tableAttr.Add(tableModel);
            }
            else
            {
                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "名称";
                tableModel.Alignment = ColumnHAlignment.Left;
                tableModel.Width = 100;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "尺寸";
                tableModel.Alignment = ColumnHAlignment.Left;
                tableModel.Width = 80;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "任务";
                tableModel.Alignment = ColumnHAlignment.Right;
                tableModel.Width = 50;
                tableAttr.Add(tableModel);

                tableModel = new TableColumnHeaderModel();
                tableModel.Name = "数量";
                tableModel.Alignment = ColumnHAlignment.Right;
                tableModel.Width = 50;
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
            col.ColumnName = "TaskCount";
            col.DefaultValue = 0;
            _printTable.Columns.Add(col);

            col = new DataColumn();
            col.DataType = typeof(int);
            col.ColumnName = "Count";
            col.DefaultValue = 0;
            _printTable.Columns.Add(col);

            return _printTable;
        }

        private void SetHotelList()
        {
            //所有的酒店信息
            List<ChooseModel> regions = new List<ChooseModel>();
            if (AllHotelList != null)
            {
                AllHotelList.OrderBy(v => v.DeliveryTime).ThenBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.HotelName).ToList().ForEach(q =>
                {
                    regions.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.HotelName });
                });
            }
            //服务器上所有酒店ID集合
            List<int> hotelids = AllHotelList.Select(c => c.ID).ToList();

            //已配送的酒店信息
            List<SendLog> list = new List<SendLog>();
            var laundryContext = new LaundryContext();
            var db = laundryContext.SendLogs.Where(c => hotelids.Contains((int)c.ID)).GroupBy(v => new { regionid = v.RegionID, regionname = v.RegionName })
                .Select(v => new
                {
                    regionID = v.Key.regionid,
                    regionname = v.Key.regionname,
                    minCreatetime = v.Min(c => c.CreateTime)
                })
                .OrderByDescending(c => c.minCreatetime);

            SendLog sl = null;
            db.ToList().ForEach(c =>
            {
                sl = new SendLog();
                sl.RegionID = c.regionID;
                sl.RegionName = c.regionname;
                sl.CreateTime = c.minCreatetime;
                list.Add(sl);

                ChooseModel reg = regions.SingleOrDefault(v => v.ChooseID == c.regionID);
                if (reg != null)
                {
                    regions.Remove(reg);
                }
            });

            //去掉最后一次配送的酒店
            List<SendLog> list1 = list.Skip(1).Take(list.Count - 1).Reverse().ToList();
            //第一个
            if (regions != null && regions.Count > 0)
            {
                ChooseModel first = regions.FirstOrDefault();
                HotelList.Add(first);
            }
            //第二个
            if (list.Count > 0)
            {
                HotelList.Add(new ChooseModel() { ChooseID = list[0].RegionID, ChooseName = list[0].RegionName });
            }
            //按配送时间排序的
            for (int i = 1; i < regions.Count; i++)
            {
                HotelList.Add(regions[i]);
            }
            //按已配送的排在后面
            for (int i = 0; i < list1.Count; i++)
            {
                HotelList.Add(new ChooseModel() { ChooseID = list1[i].RegionID, ChooseName = list1[i].RegionName });
            }
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        private void ClearForm()
        {
            if (ReaderController.Instance != null && ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            ResetTime = DateTime.Now;
            TextileList.Clear();
            SendedTagList.Clear();
            model.SubmitEnabled = false;
            model.BtnReceiveEnabled = false;
            model.UnDoEnabled = false;
            model.UnRegisterTotal = 0;
            model.TextileCount = 0;
            model.ReceivedTotal = 0;
            model.SendTable.Clear();
            ScanList.Clear();
            model.Bags.Clear();
            model.BagsStr = "";
        }

        #endregion
    }
}
