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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Logistics
{
    public class TextileMergeViewModel : ViewModelBase, IRFIDScan
    {
        private TextileMergeModel model;
        public TextileMergeModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

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

        private List<ResponseTruckRFIDTagModel> TruckList;

        private List<ResponseRFIDTagModel> TextileList;

        private List<string> ScanList;

        private List<string> ScanTruckList;

        /// <summary>
        /// 已收货芯片码
        /// </summary>
        private List<ResponseRFIDTagModel> StoragedTagList;

        public TextileMergeViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new TextileMergeModel();
            this.model.MergeTable = new ObservableCollection<TextileMergeTableModel>();
            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;
            this.model.WaitVisibled = Visibility.Hidden;
            this.model.TruckTotal = 0;

            TextileList = new List<ResponseRFIDTagModel>();
            TruckList = new List<ResponseTruckRFIDTagModel>();
            ScanList = new List<string>();
            ScanTruckList = new List<string>();
            StoragedTagList = new List<ResponseRFIDTagModel>();
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
            if (model.MergeTable.Count > 0 && !_isSubmit)
            {
                this.model.SubmitEnabled = true;
            }
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            var fcartags = rfidTags.Where(v => v.Type == TagType.Truck).Select(v => v.TagNo).ToList();

            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;

            var tag = TextileList.Select(v => v.TagNo).ToList();
            tags = tags.Where(v => !tag.Contains(v)).ToList();

            if (tags.Count > 0)
            {
                #region 纺织品

                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = tags.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.ComplexRFIDTagAnalysis, requestParam);
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

                            tag = TextileList.Where(v => !ScanList.Contains(v.TagNo)).Select(v => v.TagNo).ToList();
                            ScanList.AddRange(tag);
                        }
                    }

                    TextileList = TextileList.Where((x, y) => TextileList.FindIndex(z => z.TagNo == x.TagNo) == y).ToList();

                    TextileMergeTableModel receiveModel = null;
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
                    this.model.MergeTable.Clear();
                    query.ToList().ForEach(q =>
                    {
                        receiveModel = new TextileMergeTableModel();
                        receiveModel.BrandName = q.brandName;
                        receiveModel.ClassName = q.className;
                        receiveModel.SizeName = q.sizeName;
                        receiveModel.TextileCount = q.count;
                        total += q.count;
                        this.model.MergeTable.Add(receiveModel);
                    });

                    this.model.UnRegisterTotal = ScanList.Count - total;
                    this.model.TextileCount = total;
                }
                #endregion
            }
            if (fcartags.Count > 0)
            {

                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = fcartags.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.TruckRFIDTagAnalysis, requestParam);
                if (apiRtn.ResultCode == 0)
                {
                    if (apiRtn.Result != null && apiRtn.OtherResult != null)
                    {
                        DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                        var temp = JsonConvert.DeserializeObject<List<ResponseTruckRFIDTagModel>>(apiRtn.Result.ToString());
                        if (temp != null && d > ResetTime)
                        {
                            TruckList.AddRange(temp);
                            ScanTruckList.AddRange(fcartags);
                        }
                    }

                    if (TruckList.Count == 1)
                    {
                        this.model.TruckNo = TruckList.FirstOrDefault().TrunckRFIDTagNo;
                    }
                    else
                    {
                        this.model.TruckNo = "";
                    }
                    this.model.TruckTotal = TruckList.Count;
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
            if (this.model.MergeTable.Count == 0)
            {
                EtexsysMessageBox.Show("提示", "请扫描纺织品.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ConfigController.SystemSettingConfig.TruckIsRequired)
            {
                if (TruckList.Count == 0)
                {
                    EtexsysMessageBox.Show("提示", "请扫描架子车.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (TruckList.Count > 1 || this.model.TruckNo == "")
                {
                    EtexsysMessageBox.Show("提示", "请勿扫描多个架子车.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _isSubmit = true;
            this.model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitContent = "正在玩命提交.";
            ResetTime = DateTime.Now;

            var list = StoragedTagList.Select(v => v.TagNo).ToList();

            ScanList.RemoveAll(v => list.Contains(v));

            TextileMergeParamModel requestParam = new TextileMergeParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.TruckNo = TruckList.First().TrunckRFIDTagNo;
            requestParam.Tags = ScanList.ToArray();

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.TextileMergeTruck, requestParam);

            this.model.WaitVisibled = Visibility.Hidden;
            if (rtn.ResultCode == 1)
            {
                this.model.SubmitEnabled = true;
                EtexsysMessageBox.Show("提示", rtn.ResultMsg, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                EtexsysMessageBox.Show("提示", "拼车成功", MessageBoxButton.OK, MessageBoxImage.Information);
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
            StoragedTagList.Clear();
            this.model.SubmitEnabled = false;

            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.TruckTotal = 0;
            this.model.TruckNo = "";
            this.model.MergeTable.Clear();
            InvNo = string.Empty;
            CreateTime = string.Empty;
            ScanList.Clear();
            ScanTruckList.Clear();
            TruckList.Clear();
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

        #endregion

    }
}
