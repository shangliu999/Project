﻿using ETexsys.APIRequestModel;
using ETexsys.APIRequestModel.Request;
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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class RegisterViewModel : ViewModelBase, IRFIDScan
    {
        private RegisterModel model;
        public RegisterModel Model
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
        /// 注册纺织品
        /// </summary>
        //private BackgroundWorker _bw;

        /// <summary>
        /// 界面数据是否准备好
        /// </summary>
        private bool Prepare;

        #region 界面待选数据

        private ObservableCollection<ChooseModel> BrandList;
        private ObservableCollection<ChooseModel> ClassList;
        private ObservableCollection<ChooseModel> SizeList;
        private ObservableCollection<ChooseModel> FabricList;
        private ObservableCollection<ChooseModel> ColorList;
        private ObservableCollection<ChooseModel> StoreList;
        private ObservableCollection<ChooseModel> TextileBrandList;
        private List<ResponseClassSizeModel> ClassSize;

        #endregion

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private List<ResponseRFIDTagModel> TextileList;

        private DateTime ResetTime;

        public RegisterViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new RegisterModel();
            this.model.UnRegTable = new ObservableCollection<UnRegTableModel>();
            this.model.RegTable = new ObservableCollection<RegTableModel>();
            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;
            this.model.SizeVisbilied = Visibility.Hidden;
            this.model.WaitVisibled = Visibility.Hidden;

            ResetTime = DateTime.Now;
            _isSubmit = false;
            TextileList = new List<ResponseRFIDTagModel>();

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
            int i = 0;
            Prepare = false;

            BrandList = new ObservableCollection<ChooseModel>();
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.BrandTypeList, null);
            if (apiRtn.ResultCode == 0)
            {
                i++;
                if (apiRtn.Result != null)
                {
                    var brand = JsonConvert.DeserializeObject<List<ResponseBrandTypeModel>>(apiRtn.Result.ToString());
                    if (brand != null)
                    {
                        if (brand.Count == 1)
                        {
                            ResponseBrandTypeModel rbtm = brand.SingleOrDefault();

                            this.model.BrandID = rbtm.ID;
                            this.model.BrandName = rbtm.BrandName;

                            BrandList.Add(new ChooseModel { ChooseID = rbtm.ID, ChooseName = rbtm.BrandName });
                        }
                        else
                        {
                            brand.OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList().ForEach(q =>
                            {
                                BrandList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.BrandName });
                            });
                        }
                    }
                }
            }


            FabricList = new ObservableCollection<ChooseModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.FabricList, null);
            if (apiRtn.ResultCode == 0)
            {
                i++;
                if (apiRtn.Result != null)
                {
                    var fabric = JsonConvert.DeserializeObject<List<ResponseFabricModel>>(apiRtn.Result.ToString());
                    if (fabric != null)
                    {
                        fabric.OrderBy(v => v.FabricName).ToList().ForEach(q =>
                        {
                            FabricList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.FabricName });
                        });
                    }
                }
            }

            ColorList = new ObservableCollection<ChooseModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.ColorList, null);
            if (apiRtn.ResultCode == 0)
            {
                i++;
                if (apiRtn.Result != null)
                {
                    var color = JsonConvert.DeserializeObject<List<ResponseColorModel>>(apiRtn.Result.ToString());
                    if (color != null)
                    {
                        color.OrderBy(v => v.Sort).ThenBy(v => v.ColorName).ToList().ForEach(q =>
                          {
                              ColorList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.ColorName });
                          });
                    }
                }
            }

            StoreList = new ObservableCollection<ChooseModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.StoreList, null);
            if (apiRtn.ResultCode == 0)
            {
                i++;
                if (apiRtn.Result != null)
                {
                    var store = JsonConvert.DeserializeObject<List<ResponseStoreModel>>(apiRtn.Result.ToString());
                    if (store != null)
                    {
                        //登记在成品仓库中
                        List<ResponseStoreModel> temp = store.Where(v => v.StoreType == 3).OrderBy(v => v.Sort).ThenBy(v => v.StoreName).ToList();
                        if (temp.Count == 1)
                        {
                            ResponseStoreModel rsm = temp.SingleOrDefault();
                            this.model.StoreID = rsm.ID;
                            this.model.StoreName = rsm.StoreName;

                            StoreList.Add(new ChooseModel { ChooseID = rsm.ID, ChooseName = rsm.StoreName });
                        }
                        else
                        {
                            temp.ForEach(q =>
                            {
                                StoreList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.StoreName });
                            });
                        }
                    }
                }
            }

            ClassList = new ObservableCollection<ChooseModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.TextileClassList, null);
            if (apiRtn.ResultCode == 0)
            {
                i++;
                if (apiRtn.Result != null)
                {
                    var textileClass = JsonConvert.DeserializeObject<List<ResponseTextileClassModel>>(apiRtn.Result.ToString());
                    if (textileClass != null)
                    {
                        textileClass.Where(v => v.IsRFID == true).OrderBy(v => v.Sort).ThenBy(v => v.ClassName).ToList().ForEach(q =>
                        {
                            ClassList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.ClassName });
                        });
                    }
                }
            }

            ClassSize = new List<ResponseClassSizeModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.SizeList, null);
            if (apiRtn.ResultCode == 0)
            {
                i++;
                if (apiRtn.Result != null)
                {
                    ClassSize = JsonConvert.DeserializeObject<List<ResponseClassSizeModel>>(apiRtn.Result.ToString());
                }
            }

            TextileBrandList = new ObservableCollection<ChooseModel>();
            apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.TextileBrandList, null);
            if (apiRtn.ResultCode == 0)
            {
                i++;
                if (apiRtn.Result != null)
                {
                    var textileClass = JsonConvert.DeserializeObject<List<ResponseTextileBrandModel>>(apiRtn.Result.ToString());
                    if (textileClass != null)
                    {
                        textileClass.ForEach(q =>
                        {
                            TextileBrandList.Add(new ChooseModel { ChooseID = q.ID, ChooseName = q.TextileBrandName });
                        });
                    }
                }
            }
            if (StoreList.Count == 1)
            {
                model.StoreID = StoreList[0].ChooseID;
                model.StoreName = StoreList[0].ChooseName;
            }

            List<string> cacheList = ConfigController.GetCacheRegTextileBrand();
            if (cacheList.Count > 0 && cacheList[0] != "0")
            {
                int id = 0;
                int.TryParse(cacheList[0], out id);
                var mt = TextileBrandList.Where(v => v.ChooseID == id).FirstOrDefault();
                if (mt != null)
                {
                    model.TextileBrandId = mt.ChooseID;
                    model.TextileBrandName = mt.ChooseName;
                }
            }

            if (i == 7)
            {
                Prepare = true;
            }
        }

        #region RFID Interface

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
                List<ResponseRFIDTagModel> temp = null;
                if (apiRtn.Result != null && apiRtn.OtherResult != null)
                {
                    DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                    temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                    if (temp != null && d > ResetTime)
                    {
                        TextileList.AddRange(temp);

                        for (int i = 0; i < requestParam.TagList.Length; i++)
                        {
                            if (temp == null || temp.Where(v => v.TagNo == requestParam.TagList[i]).Count() == 0)
                            {
                                string t = requestParam.TagList[i];
                                if (model.UnRegTable.Where(v => v.RFID == t).FirstOrDefault() == null)
                                {
                                    model.UnRegTable.Add(new UnRegTableModel { RFID = t, RFIDWashingTime = 0 });
                                }
                            }
                        }

                        RegTableModel regModel = null;
                        var query = from t in TextileList
                                    group t by new
                                    {
                                        t1 = t.ClassSort,
                                        t2 = t.ClassName,
                                    } into m
                                    orderby m.Key.t1, m.Key.t2
                                    select new
                                    {
                                        className = m.Key.t2,
                                        count = m.Count()
                                    };

                        this.model.RegTable.Clear();
                        query.ToList().ForEach(q =>
                        {
                            regModel = new RegTableModel();
                            regModel.ClassName = q.className;
                            regModel.TextileCount = q.count;
                            this.model.RegTable.Add(regModel);
                        });

                        this.model.UnRegisterTotal = model.UnRegTable.Count;
                        this.model.TextileCount = TextileList.Count;
                    }
                }
            }


        }

        public void NoScanTag()
        {
            if (model.UnRegTable.Count > 0 && !_isSubmit)
            {
                this.model.SubmitEnabled = true;
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
                                this.model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                                this.model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                            });
                        }
                    }
                    break;
                case 1:
                    ReaderController.Instance.ScanUtilities.StopScan();
                    this.model.ReaderState = 2;
                    this.model.ReaderLight = new Uri("../../Skins/Default/Images/stopscan.gif", UriKind.RelativeOrAbsolute);
                    break;
                case 2:
                    ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                    {
                        this.model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                        this.model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                    });
                    break;
            }
        }

        private void ChooseAttrAction(string sender)
        {
            if (!Prepare)
            {
                EtexsysMessageBox.Show("提示", "数据正在加载，请稍等.或重新进入。", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ObservableCollection<ChooseModel> list = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();
            string title = "";
            switch (sender)
            {
                case "Brand":
                    list = BrandList;
                    title = "请选择流通:";
                    break;
                case "Class":
                    list = ClassList;
                    title = "请选择品名";
                    break;
                case "Size":
                    if (model.ClassID != 0)
                    {
                        list = SizeList;
                    }
                    else
                    {
                        EtexsysMessageBox.Show("提示", "请先选择品名.", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    title = "请选择尺寸:";
                    break;
                case "Fabric":
                    list = FabricList;
                    title = "请选择面料:";
                    break;
                case "Color":
                    list = ColorList;
                    title = "请选择颜色:";
                    break;
                case "Store":
                    list = StoreList;
                    title = "请选择仓库:";
                    break;
                case "TextileBrand":
                    list = TextileBrandList;
                    title = "请选择品牌";
                    break;
                default:
                    return;
            }


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
                    case "Brand":
                        this.model.BrandID = modalModel.ChooseItem.ChooseID;
                        this.model.BrandName = modalModel.ChooseItem.ChooseName;
                        break;
                    case "Class":
                        this.model.ClassID = modalModel.ChooseItem.ChooseID;
                        this.model.ClassName = modalModel.ChooseItem.ChooseName;
                        SizeList = new ObservableCollection<ChooseModel>();

                        var query = from t in ClassSize
                                    where t.ClassID == model.ClassID
                                    orderby t.Sort, t.SizeName
                                    select new
                                    {
                                        t.SizeName,
                                        t.SizeID
                                    };

                        query.ToList().ForEach(q =>
                        {
                            SizeList.Add(new ChooseModel { ChooseID = q.SizeID, ChooseName = q.SizeName });
                        });
                        if (SizeList.Count > 0)
                        {
                            this.model.SizeVisbilied = Visibility.Visible;
                        }
                        else
                        {
                            this.model.SizeVisbilied = Visibility.Hidden;
                        }
                        this.model.SizeID = 0;
                        this.model.SizeName = "";
                        break;
                    case "Size":
                        this.model.SizeID = modalModel.ChooseItem.ChooseID;
                        this.model.SizeName = modalModel.ChooseItem.ChooseName;
                        break;
                    case "Fabric":
                        this.model.FabricID = modalModel.ChooseItem.ChooseID;
                        this.model.FabricName = modalModel.ChooseItem.ChooseName;
                        break;
                    case "Color":
                        this.model.ColorID = modalModel.ChooseItem.ChooseID;
                        this.model.ColorName = modalModel.ChooseItem.ChooseName;
                        break;
                    case "Store":
                        this.model.StoreID = modalModel.ChooseItem.ChooseID;
                        this.model.StoreName = modalModel.ChooseItem.ChooseName;
                        break;
                    case "TextileBrand":
                        this.model.TextileBrandId = modalModel.ChooseItem.ChooseID;
                        this.model.TextileBrandName = modalModel.ChooseItem.ChooseName;
                        ConfigController.CacheRegTextileBrand(modalModel.ChooseItem.ChooseID, modalModel.ChooseItem.ChooseName);
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
            if (this.model.BrandID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择流通.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.ClassID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择品名.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.SizeVisbilied == Visibility.Visible && this.model.SizeID == 0)
            {
                EtexsysMessageBox.Show("提示", "请选择尺寸.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.UnRegTable.Count == 0)
            {
                EtexsysMessageBox.Show("提示", "请扫描纺织品.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //if (!_bw.IsBusy)
            //{
            _isSubmit = true;
            this.model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitContent = "正在玩命提交.";
            //    _bw.RunWorkerAsync();
            //}
            ResetTime = DateTime.Now;
            RegisterParamModel request = new RegisterParamModel();
            request.BrandID = Convert.ToInt32(this.model.BrandID);
            request.ClassID = Convert.ToInt32(this.model.ClassID);
            request.SizeID = Convert.ToInt32(this.model.SizeID);
            request.FabricID = Convert.ToInt32(this.model.FabricID);
            request.ColorID = Convert.ToInt32(this.model.ColorID);
            request.StoreID = Convert.ToInt32(this.model.StoreID);
            request.TextileBrandID = Convert.ToInt32(this.model.TextileBrandId);
            request.Tags = this.model.UnRegTable.Select(v => v.RFID).ToArray();
            request.TimeStamp = ApiController.Instance.GetTimeStamp();
            request.UUID = ConfigController.MacCode;
            request.TerminalType = ConfigController.TerminalType;

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.Register, request);

            this.model.WaitVisibled = Visibility.Hidden;
            if (rtn.ResultCode == 1)
            {
                EtexsysMessageBox.Show("提示", rtn.ResultMsg, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                EtexsysMessageBox.Show("提示", "登记成功", MessageBoxButton.OK, MessageBoxImage.Information);
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
            this.model.UnRegTable.Clear();
            this.model.RegTable.Clear();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitVisibled = Visibility.Hidden;
            this.model.WaitContent = "刷新读写器.";
            Thread.Sleep(50);
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
