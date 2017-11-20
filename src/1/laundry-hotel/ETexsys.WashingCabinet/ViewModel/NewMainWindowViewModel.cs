using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using ETexsys.RFIDServer;
using ETexsys.RFIDServer.Model;
using ETexsys.RFIDServer.Reader;
using ETexsys.WashingCabinet.Domain;
using ETexsys.WashingCabinet.Model;
using ETexsys.WashingCabinet.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ETexsys.WashingCabinet.ViewModel
{
    public class NewMainWindowViewModel : ViewModelBase, IRFIDScan
    {
        #region 对象

        private NewMainWindowModel model;
        public NewMainWindowModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private string CurrentUser;

        /// <summary>
        /// 门状态
        /// </summary>
        private bool DoorState;

        /// <summary>
        /// 读写器状态
        /// </summary>
        private bool ReaderState;

        /// <summary>
        /// 读写器开始时间
        /// </summary>
        private DateTime OpenReaderTime;

        /// <summary>
        /// 用户集合
        /// </summary>
        private Dictionary<string, string> UserDic;

        /// <summary>
        /// 门锁线程
        /// </summary>
        private readonly DispatcherTimer _comTimer;

        private readonly DispatcherTimer _readerTimer;

        private readonly DispatcherTimer _dirtReaderTimer;

        private List<ResponseRFIDTagModel> TextileList;

        private List<ResponseRFIDTagModel> DirtTextileList;

        private ComUitilities comUitileties;

        private List<ResponseRFIDTagModel> OldTextileList;

        private List<CostReceiveRecordsModel> RecordsList;

        private List<string> DirtScanList;

        private List<string> DirtNewScanList;

        /// <summary>
        /// 显示试图 1 净物 2污物
        /// </summary>
        private int ShowView;
        #endregion

        #region 构造函数

        public NewMainWindowViewModel()
        {
            this.model = new NewMainWindowModel();
            this.model.ChangeBtnText = "切换到污物仓";
            this.model.SubTitle = "当前净物仓-存储量";
            this.model.SubTitleColor = "#21AF37";
            this.model.CostReceiveRecordsList = new List<CostReceiveRecordsModel>();
            this.model.DirtTextileList = new List<TextileStoreModel>();
            this.model.TextileStoreList = new List<TextileStoreModel>();
            this.model.DirtStoreVisibility = Visibility.Hidden;
            this.model.CleanStoreVisibility = Visibility.Visible;
            ShowView = 1;
            DoorState = false;
            ReaderState = false;
            OldTextileList = new List<ResponseRFIDTagModel>();
            TextileList = new List<ResponseRFIDTagModel>();
            DirtTextileList = new List<ResponseRFIDTagModel>();
            RecordsList = new List<CostReceiveRecordsModel>();

            DirtScanList = new List<string>();
            DirtNewScanList = new List<string>();

            comUitileties = new ComUitilities();

            _comTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _comTimer.Tick += _comTimer_Tick;

            int rtn = comUitileties.GetDoorState();

            if (rtn == 1)
            {
                if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                {
                    if (ConfigController.ReaderConfig.IsConnection)
                    {
                        ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                        {
                            ReaderState = true;
                            OpenReaderTime = DateTime.Now;
                        });
                        ReaderController.Instance.ScanUtilities.ClearScanTags();
                    }
                }
            }
            else
            {
                DoorState = true;
                _comTimer.Start();
            }

           

            _readerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _readerTimer.Tick += _readerTimer_Tick;
            _readerTimer.Start();

            _dirtReaderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _dirtReaderTimer.Tick += _dirtReaderTimer_Tick;
            _dirtReaderTimer.Start();


            UserDic = new Dictionary<string, string>();
            UserDic.Add("06037678", "邓志军");
            UserDic.Add("07844062", "曾超芬");
            UserDic.Add("25364197", "彭寿金");
            UserDic.Add("18535365", "秦志翔");
        }

        #endregion

        #region Timer

        private async void _dirtReaderTimer_Tick(object sender, EventArgs e)
        {
            _dirtReaderTimer.Stop();

            if (AddTagList())
            {
                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = DirtNewScanList.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RFIDTagAnalysis, requestParam);
                if (apiRtn.ResultCode == 0)
                {
                    DirtNewScanList.Clear();
                    if (apiRtn.Result != null && apiRtn.OtherResult != null)
                    {
                        DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                        var temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                        if (temp != null)
                        {
                            DirtTextileList.AddRange(temp);
                        }

                        var query = from t in DirtTextileList group t by new { t1 = t.ClassName } into m select new { m.Key.t1, count = m.Count() };

                        List<TextileStoreModel> list = new List<TextileStoreModel>();
                        TextileStoreModel textileModel = null;
                        query.ToList().ForEach(q =>
                        {
                            textileModel = new TextileStoreModel();
                            textileModel.ClassName = q.t1;
                            textileModel.TextileCount = q.count;
                            textileModel.TextileDetail = new List<TextileModel>();

                            var qy = from c in DirtTextileList where c.ClassName == q.t1 group c by new { c1 = c.SizeName } into n select new { n.Key.c1, count = n.Count() };

                            TextileModel m = null;
                            qy.ToList().ForEach(_ =>
                            {
                                if (!string.IsNullOrWhiteSpace(_.c1))
                                {
                                    m = new TextileModel();
                                    m.SizeName = _.c1;
                                    m.TextileCount = _.count;
                                    textileModel.TextileDetail.Add(m);
                                }
                            });

                            if (textileModel.TextileDetail.Count > 0)
                            {
                                textileModel.SizeVisibility = System.Windows.Visibility.Visible;
                                textileModel.NoSizeVisibility = System.Windows.Visibility.Collapsed;
                            }
                            else
                            {
                                textileModel.NoSizeVisibility = System.Windows.Visibility.Visible;
                                textileModel.SizeVisibility = System.Windows.Visibility.Collapsed;
                            }

                            list.Add(textileModel);
                        });
                        this.model.DirtTextileList = list;
                    }
                }
            }

            _dirtReaderTimer.Start();
        }

        private void _readerTimer_Tick(object sender, EventArgs e)
        {
            DateTime dt = OpenReaderTime.AddSeconds(10);
            if (DateTime.Now >= dt)
            {
                if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                {
                    if (ReaderState)
                    {
                        ReaderState = false;
                        ReaderController.Instance.ScanUtilities.StopScan();
                        ReaderController.Instance.ScanUtilities.ClearScanTags();
                        Thread.Sleep(1000);
                        CostReceive();
                    }
                }
            }
        }

        private void _comTimer_Tick(object sender, EventArgs e)
        {
            int rtn = comUitileties.GetDoorState();

            if (rtn == 1 && DoorState)
            {
                //门已关上,需要去开启读写器 
                DoorState = false;
                _comTimer.Stop();
                if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                {
                    if (ConfigController.ReaderConfig.IsConnection)
                    {
                        TextileList = new List<ResponseRFIDTagModel>();

                        ReaderController.Instance.ScanUtilities.ClearScanTags();
                        ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                        {
                            ReaderState = true;
                            OpenReaderTime = DateTime.Now;
                        });
                    }
                }
            }
        }

        #endregion

        #region 私有方法

        private bool AddTagList()
        {
            bool isAdded = false;

            lock (DirtNewScanList)
            {
                while (App.TagQueue.Count > 0)
                {
                    var tag = App.TagQueue.Dequeue();

                    if (tag == null)
                    {
                        continue;
                    }

                    if (!DirtScanList.Contains(tag))
                    {
                        DirtScanList.Add(tag);
                        DirtNewScanList.Add(tag);
                        isAdded = true;
                    }
                }
            }
            return isAdded;
        }

        private void CostReceive()
        {
            List<string> tagList = TextileList.Select(v => v.TagNo).ToList();
            var query = from t in OldTextileList
                        where !tagList.Contains(t.TagNo)
                        group t by new
                        {
                            t.ClassName
                        } into m
                        select new { m.Key.ClassName, count = m.Count() };

            CostReceiveRecordsModel recordsModel = new CostReceiveRecordsModel();
            recordsModel.RecordsDetail = new List<TextileModel>();
            recordsModel.OperatorName = CurrentUser;
            recordsModel.StateName = "领取";
            recordsModel.StateColor = "#21AF37";
            recordsModel.Time = DateTime.Now.ToString("HH:mm");
            TextileModel tModel = null;
            query.ToList().ForEach(q =>
            {
                tModel = new TextileModel();
                tModel.ClassName = q.ClassName;
                tModel.TextileCount = q.count;
                recordsModel.RecordsDetail.Add(tModel);
            });
            if (recordsModel.RecordsDetail.Count > 0)
            {
                RecordsList.Add(recordsModel);
            }

            tagList = OldTextileList.Select(v => v.TagNo).ToList();
            var query1 = from t in TextileList where !tagList.Contains(t.TagNo) group t by new { t.ClassName } into m select new { m.Key.ClassName, count = m.Count() };
            recordsModel = new CostReceiveRecordsModel();
            recordsModel.RecordsDetail = new List<TextileModel>();
            recordsModel.OperatorName = CurrentUser == null ? "李玉峰" : CurrentUser;
            recordsModel.StateName = "投放";
            recordsModel.StateColor = "#FA8305";
            recordsModel.Time = DateTime.Now.ToString("HH:mm");
            query1.ToList().ForEach(q =>
            {
                tModel = new TextileModel();
                tModel.ClassName = q.ClassName;
                tModel.TextileCount = q.count;
                recordsModel.RecordsDetail.Add(tModel);
            });
            if (recordsModel.RecordsDetail.Count > 0)
            {
                RecordsList.Add(recordsModel);
            }
            List<CostReceiveRecordsModel> list = new List<CostReceiveRecordsModel>();
            int index = RecordsList.Count > 3 ? RecordsList.Count - 3 : 0;
            for (int i = RecordsList.Count - 1; i >= index; i--)
            {
                list.Add(RecordsList[i]);
            }
            this.model.CostReceiveRecordsList = list;
            OldTextileList.Clear();
            foreach (var item in TextileList)
            {
                OldTextileList.Add(item);
            }
            TextileList.Clear();
        }

        #endregion

        #region 事件

        private RelayCommand<object> _CardCommand;
        public ICommand CardCommand
        {
            get
            {
                if (_CardCommand == null)
                {
                    _CardCommand = new RelayCommand<object>(CardAction);
                }
                return _CardCommand;
            }
        }

        private RelayCommand<object> _ChangeViewCmd;
        public ICommand ChangeViewCmd
        {
            get
            {
                if (_ChangeViewCmd == null)
                {
                    _ChangeViewCmd = new RelayCommand<object>(ChangeViewAction);
                }
                return _ChangeViewCmd;
            }
        }

        public void CardAction(object obj)
        {
            if (!string.IsNullOrWhiteSpace(model.ICCode))
            {
                var user = UserDic.Where(v => v.Key == model.ICCode).FirstOrDefault();
                if (user.Key != null)
                {
                    CurrentUser = user.Value;
                    model.ICCode = "";

                    if (!DoorState)
                    {
                        bool rtn = comUitileties.OpenDoor();
                        DoorState = rtn;
                        if (rtn)
                        {
                            //开门成功.需要关闭读写器
                            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                            {
                                if (ConfigController.ReaderConfig.IsConnection)
                                {
                                    ReaderState = false;
                                    ReaderController.Instance.ScanUtilities.StopScan();
                                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                                }
                            }
                            _comTimer.Start();
                        }
                    }
                }
                model.ICCode = "";
            }
        }

        public void ChangeViewAction(object obj)
        {
            if (ShowView == 1)
            {
                ShowView = 2;
                this.model.CleanStoreVisibility = Visibility.Hidden;
                this.model.DirtStoreVisibility = Visibility.Visible;
                this.model.SubTitleColor = "#2167AF";
                this.model.SubTitle = "当前污物仓-投污量";
                this.model.ChangeBtnText = "切换到净物仓";
            }
            else
            {
                ShowView = 1;
                this.model.DirtStoreVisibility = Visibility.Hidden;
                this.model.CleanStoreVisibility = Visibility.Visible;
                this.model.SubTitleColor = "#21AF37";
                this.model.SubTitle = "当前净物仓-存储量";
                this.model.ChangeBtnText = "切换到污物仓";
            }
        }

        #endregion

        #region IRFIDScan Interface

        public void NoScanTag()
        {

        }

        public async void ScanNew(List<TagModel> rfidTags)
        {

            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
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
                    if (temp != null)
                    {
                        TextileList.AddRange(temp);
                    }

                    var query = from t in TextileList group t by new { t1 = t.ClassName } into m select new { m.Key.t1, count = m.Count() };

                    List<TextileStoreModel> list = new List<TextileStoreModel>();
                    TextileStoreModel textileModel = null;
                    query.ToList().ForEach(q =>
                    {
                        textileModel = new TextileStoreModel();
                        textileModel.ClassName = q.t1;
                        textileModel.TextileCount = q.count;
                        textileModel.TextileDetail = new List<TextileModel>();

                        var qy = from c in TextileList where c.ClassName == q.t1 group c by new { c1 = c.SizeName } into n select new { n.Key.c1, count = n.Count() };

                        TextileModel m = null;
                        qy.ToList().ForEach(_ =>
                        {
                            if (!string.IsNullOrWhiteSpace(_.c1))
                            {
                                m = new TextileModel();
                                m.SizeName = _.c1;
                                m.TextileCount = _.count;
                                textileModel.TextileDetail.Add(m);
                            }
                        });

                        if (textileModel.TextileDetail.Count > 0)
                        {
                            textileModel.SizeVisibility = System.Windows.Visibility.Visible;
                            textileModel.NoSizeVisibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            textileModel.NoSizeVisibility = System.Windows.Visibility.Visible;
                            textileModel.SizeVisibility = System.Windows.Visibility.Collapsed;
                        }

                        list.Add(textileModel);
                    });
                    this.model.TextileStoreList = list;
                }
            }

        }

        #endregion
    }
}
