using ETexsys.WashingCabinet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using ETexsys.APIRequestModel.Request;
using Newtonsoft.Json;
using ETexsys.APIRequestModel;
using ETexsys.WashingCabinet.Model;
using ETexsys.WashingCabinet.Utilities;
using System.Collections.ObjectModel;
using ETexsys.APIRequestModel.Response;
using System.Windows.Input;

namespace ETexsys.WashingCabinet.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IRFIDScan
    {
        private bool DoorState;

        private Dictionary<string, string> UserDic;

        private string CurrentUser;

        private readonly DispatcherTimer _comTimer;

        DispatcherTimer _stop;

        private ComUitilities comUitileties;
        
        MessageTable messagtable;
        
        Label Time;

        public ChooesTable sizeModel;

        public ObservableCollection<ChooesTable> sizeList;

        public ObservableCollection<DetailsTable> detaileList;

        private List<ResponseRFIDTagModel> TextileList;

        ObservableCollection<MessageTable> messageList;

        ChooesTable chooesTable;

        ObservableCollection<DetailsTable> oldMessageDetailsList;

        DetailsTable rowModel;

        public MessageDetails detailetable;
        
        public MainWindowModel model;
        public MainWindowModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private ChooesTable chooesModel;
        public ChooesTable ChooesModel
        {
            get { return chooesModel; }
            set
            {
                chooesModel = value;
                RaisePropertyChanged("ChooesModel");
            }
        }

        public MainWindowViewModel(Label lable)
        {
            messageList = new ObservableCollection<MessageTable>();
            _stop = new DispatcherTimer();
            detailetable = new MessageDetails();
            oldMessageDetailsList = new ObservableCollection<DetailsTable>();
            model = new MainWindowModel { NetVisible= "Visible", DirtVisible= "Hidden" };
            this.model.DetailsList = new ObservableCollection<DetailsTable>();
            this.model.MessageList = new ObservableCollection<MessageTable>();
            TextileList = new List<ResponseRFIDTagModel>();
            sizeList = new ObservableCollection<ChooesTable>();
            detaileList = new ObservableCollection<DetailsTable>();
            Time = lable;
            lable.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(ShowCurrentTime);
            int time1 = DateTime.Now.Second;
            int time2 = 60 - time1;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, time2);
            dispatcherTimer.Start();
            _comTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _comTimer.Tick += _comTimer_Tick; ;

            comUitileties = new ComUitilities();

            UserDic = new Dictionary<string, string>();
            UserDic.Add("06037678", "邓志军");
            UserDic.Add("07844062", "曾超芬");
            UserDic.Add("25364197", "彭寿金");
            UserDic.Add("18535365", "秦志翔");
        }

        private void _comTimer_Tick(object sender, EventArgs e)
        {
            int rtn = comUitileties.GetDoorState();

            if (rtn == 1)
            {
                //门已关上,需要去开启读写器
                this.model.DetailsList = new ObservableCollection<DetailsTable>();
                DoorState = false;
                _comTimer.Stop();
                if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                {
                    if (ConfigController.ReaderConfig.IsConnection)
                    {
                        ReaderController.Instance.ScanUtilities.StartScan(this);
                        ReaderController.Instance.ScanUtilities.ClearScanTags();
                        if (_stop.Tag == null)
                        {
                            _stop.Tick += new EventHandler(_stopScan);
                            _stop.Tag = "stopScan";
                        }
                        _stop.Interval = new TimeSpan(0, 0, 0, 20);
                        _stop.Start();
                    }
                }
            }
        }

        public void ShowCurrentTime(object sender, EventArgs e)
        {
            Time.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public void _stopScan(object sender, EventArgs e)
        {
            MessageTable Delivery = new MessageTable();
            Delivery.MessageDetailsList = new ObservableCollection<MessageDetails>();
            Delivery.OperatorName = CurrentUser;
            Delivery.Time = DateTime.Now.ToShortTimeString().ToString();
            foreach (var item in messageList)
            {
                if (item.DeliveryVisible == "Visible")
                {
                    foreach (var item1 in item.MessageDetailsList)
                    {
                        MessageDetails deyailes = new MessageDetails();
                        deyailes.ClassCount = item1.ClassCount;
                        deyailes.ClassName = item1.ClassName;
                        Delivery.MessageDetailsList.Add(deyailes);
                    }
                    Delivery.DeliveryVisible = "Visible";
                    Delivery.ReceiveVisible = "Hidden";
                }
            }
            #region 将所有类名相同的合并
            var list = Delivery.MessageDetailsList.GroupBy(n => n.ClassName).Select(a => new { ClassName = a.Key, ClassCount = a.Sum(c => c.ClassCount) }).ToList();
            Delivery.MessageDetailsList.Clear();
            foreach (var item in list)
            {
                MessageDetails details = new MessageDetails();
                details.ClassCount = item.ClassCount;
                details.ClassName = item.ClassName;
                Delivery.MessageDetailsList.Add(details);
            }
            if (Delivery.MessageDetailsList.Count > 0)
            {
                this.model.MessageList.Insert(0, Delivery);
            }
            #endregion
            MessageTable Receive = new MessageTable();
            Receive.MessageDetailsList = new ObservableCollection<MessageDetails>();
            Receive.OperatorName = CurrentUser;
            Receive.Time = DateTime.Now.ToShortTimeString().ToString();
            foreach (var item in messageList)
            {
                if (item.ReceiveVisible == "Visible")
                {
                    foreach (var item1 in item.MessageDetailsList)
                    {
                        MessageDetails deyailes = new MessageDetails();
                        deyailes.ClassCount = item1.ClassCount;
                        deyailes.ClassName = item1.ClassName;
                        Receive.MessageDetailsList.Add(deyailes);
                    }
                    Receive.DeliveryVisible = "Hidden";
                    Receive.ReceiveVisible = "Visible";
                }
            }
            #region 将所有类名相同的合并
            var list1 = Receive.MessageDetailsList.GroupBy(n => n.ClassName).Select(a => new { ClassName = a.Key, ClassCount = a.Sum(c => c.ClassCount) }).ToList();
            Receive.MessageDetailsList.Clear();
            foreach (var item in list1)
            {
                MessageDetails details = new MessageDetails();
                details.ClassCount = item.ClassCount;
                details.ClassName = item.ClassName;
                Receive.MessageDetailsList.Add(details);
            }
            if (Receive.MessageDetailsList.Count > 0)
            {
                this.model.MessageList.Insert(0,Receive);
            }
            #endregion
            ReaderController.Instance.ScanUtilities.StopScan();
            ReaderController.Instance.ScanUtilities.ClearScanTags();
            _stop.Stop();
        }

        private RelayCommand<object> goNet;
        public ICommand GoNet
        {
            get
            {
                if (goNet == null)
                {
                    goNet = new RelayCommand<object>(GoNetAction);
                }
                return goNet;
            }
        }
        public void GoNetAction(object Prameter)
        {
            this.model.NetVisible = "Visible";
            this.model.DirtVisible = "Hidden";
        }

        private RelayCommand<object> getDirt;
        public ICommand GetDirt
        {
            get
            {
                if (getDirt == null)
                {
                    getDirt = new RelayCommand<object>(GetDirtAction);
                }
                return getDirt;
            }
        }
        public void GetDirtAction(object Prameter)
        {
            this.model.NetVisible = "Hidden";
            this.model.DirtVisible = "Visible";
        }

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
                                    _stop.Stop();
                                    ReaderController.Instance.ScanUtilities.StopScan();
                                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                                }
                                oldMessageDetailsList = this.model.DetailsList;
                                TextileList.Clear();
                            }
                            _comTimer.Start();
                        }
                    }
                }
                model.ICCode = "";
            }
        }

        private RelayCommand<object> open;
        public ICommand Open
        {
            get
            {
                if (open == null)
                {
                    open = new RelayCommand<object>(OpenAction);
                }
                return open;
            }
        }
        public void OpenAction(object parameter)
        {
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                if (ConfigController.ReaderConfig.IsConnection)
                {
                    _stop.Stop();
                    ReaderController.Instance.ScanUtilities.StopScan();
                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                }
                oldMessageDetailsList = this.model.DetailsList;
                TextileList.Clear();
            }
            return;
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
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                if (ConfigController.ReaderConfig.IsConnection)
                {
                    ReaderController.Instance.ScanUtilities.StartScan(this);
                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                    if (_stop.Tag==null)
                    {
                        _stop.Tick += new EventHandler(_stopScan);
                        _stop.Tag = "stopScan";
                    }
                    _stop.Interval = new TimeSpan(0, 0, 0, 10);
                    _stop.Start();
                }
            }
        }

        #region RFIF Interface
        public void NoScanTag()
        {
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();

            var tag = TextileList.Select(v => v.TagNo).ToList();
            tags = tags.Where(v => !tag.Contains(v)).ToList();
            if (tags.Count > 0)
            {
                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = tags.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RFIDTagAnalysis, requestParam);
                if (apiRtn.ResultCode == 0)
                {
                    if (apiRtn.Result != null)
                    {
                        var temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                        if (temp != null)
                        {
                            TextileList.AddRange(temp);
                        }
                    }
                }
            }
            TextileList = TextileList.OrderBy(v => v.TagNo).ToList();
            TextileList = TextileList.Where((x, y) => TextileList.FindIndex(z => z.TagNo == x.TagNo) == y).ToList();
            var query = from t in TextileList
                        group t by new
                        {
                            t3 = t.ClassSort,
                            t4 = t.ClassName,
                            t5 = t.SizeSort,
                            t6 = t.SizeName
                        } into m
                        orderby m.Key.t3, m.Key.t4, m.Key.t5, m.Key.t6
                        select new
                        {
                            className = m.Key.t4,
                            sizeName = m.Key.t6,
                            count = m.Count()
                        };
            this.model.DetailsList = new ObservableCollection<DetailsTable>();
            messageList = new ObservableCollection<MessageTable>();
            var List = query.ToList();
            List.ForEach(q =>
            {
                int sum = 0;
                rowModel = new DetailsTable();
                rowModel.ChooesList = new ObservableCollection<ChooesTable>();
                rowModel.ClassName = q.className;
                foreach (var item in List)
                {
                    if (item.className == rowModel.ClassName)
                    {
                        chooesTable = new ChooesTable();
                        if (item.sizeName == null)
                        {
                            rowModel.Visible1 = "Hidden";
                            rowModel.Visible2 = "Visible";
                        }
                        else
                        {
                            rowModel.Visible1 = "Visible";
                            rowModel.Visible2 = "Hidden";
                        }
                        chooesTable.SizeCount = item.count;
                        chooesTable.SizeName = item.sizeName;
                        sum += item.count;
                        rowModel.ChooesList.Add(chooesTable);
                    }
                }
                List = query.Where(c => c.className != rowModel.ClassName).ToList();
                rowModel.ClassCount = sum;
                if (rowModel.ClassCount != 0)
                {
                    this.model.DetailsList.Add(rowModel);
                }
            });
            int length = this.model.DetailsList.ToArray().Length;
            int oldlength = oldMessageDetailsList.ToArray().Length;
            bool len = length - oldlength > 0;
            if (len)
            {
                for (int i = 0; i < length; i++)
                {
                    messagtable = new MessageTable();
                    messagtable.MessageDetailsList = new ObservableCollection<MessageDetails>();
                    detailetable = new MessageDetails();
                    int val = 0;
                    bool Contain = false;
                    for (int k = 0; k < oldlength; k++)
                    {
                        if (this.model.DetailsList[i].ClassName == oldMessageDetailsList[k].ClassName)
                        {
                            Contain = true;
                        }
                    }
                    if (Contain)
                    {
                        for (int j = 0; j < oldlength; j++)
                        {
                            val = 0;
                            if (this.model.DetailsList[i].ClassName == oldMessageDetailsList[j].ClassName)
                            {
                                val = this.model.DetailsList[i].ClassCount - oldMessageDetailsList[j].ClassCount;
                                if (val < 0)
                                {
                                    val = -val;
                                    detailetable.ClassName = oldMessageDetailsList[j].ClassName;
                                    detailetable.ClassCount = val;
                                    messagtable.DeliveryVisible = "Hidden";
                                    messagtable.ReceiveVisible = "Visible";
                                }
                                else
                                {
                                    detailetable.ClassName = oldMessageDetailsList[j].ClassName;
                                    detailetable.ClassCount = val;
                                    messagtable.DeliveryVisible = "Visible";
                                    messagtable.ReceiveVisible = "Hidden";
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        messagtable.MessageDetailsList.Add(detailetable);
                        messagtable.Time = DateTime.Now.ToShortTimeString().ToString();
                        messagtable.OperatorName = CurrentUser;
                        if (val != 0)
                        {
                            messageList.Add(messagtable);
                        }
                    }
                    else
                    {
                        MessageDetails details = new MessageDetails();
                        details.ClassCount = this.model.DetailsList[i].ClassCount;
                        details.ClassName = this.model.DetailsList[i].ClassName;
                        messagtable.MessageDetailsList.Add(details);
                        if (details.ClassCount > 0)
                        {
                            messagtable.DeliveryVisible = "Visible";
                            messagtable.ReceiveVisible = "Hidden";
                        }
                        else
                        {
                            messagtable.DeliveryVisible = "Hidden";
                            messagtable.ReceiveVisible = "Visible";
                        }
                        messagtable.Time = DateTime.Now.ToShortTimeString().ToString();
                        messageList.Add(messagtable);
                    }
                }
                #region 后面的
                for (int i = 0; i < oldlength; i++)
                {
                    bool Contain = false;
                    for (int j = 0; j < length; j++)
                    {
                        if (this.model.DetailsList[j].ClassName == oldMessageDetailsList[i].ClassName)
                        {
                            Contain = true;
                        }
                    }
                    if (Contain)
                    {

                    }
                    else
                    {
                        messagtable = new MessageTable();
                        MessageDetails details = new MessageDetails();
                        messagtable.MessageDetailsList = new ObservableCollection<MessageDetails>();
                        details.ClassCount = oldMessageDetailsList[i].ClassCount;
                        details.ClassName = oldMessageDetailsList[i].ClassName;
                        messagtable.MessageDetailsList.Add(details);
                        if (details.ClassCount > 0)
                        {
                            messagtable.DeliveryVisible = "Hidden";
                            messagtable.ReceiveVisible = "Visible";
                        }
                        else
                        {
                            messagtable.DeliveryVisible = "Visible";
                            messagtable.ReceiveVisible = "Hidden";
                        }
                        messagtable.Time = DateTime.Now.ToShortTimeString().ToString();
                        messageList.Add(messagtable);
                    }
                }
                #endregion
            }
            else
            {
                for (int i = 0; i < oldlength; i++)
                {
                    messagtable = new MessageTable();
                    messagtable.MessageDetailsList = new ObservableCollection<MessageDetails>();
                    detailetable = new MessageDetails();
                    int val = 0;
                    bool Contain = false;
                    for (int k = 0; k < length; k++)
                    {
                        if (this.model.DetailsList[k].ClassName == oldMessageDetailsList[i].ClassName)
                        {
                            Contain = true;
                        }
                    }
                    if (Contain)
                    {
                        for (int j = 0; j < length; j++)
                        {
                            if (oldMessageDetailsList[i].ClassName == this.model.DetailsList[j].ClassName)
                            {
                                val = this.model.DetailsList[j].ClassCount - oldMessageDetailsList[i].ClassCount;
                                if (val < 0)
                                {
                                    val = -val;
                                    detailetable.ClassName = oldMessageDetailsList[i].ClassName;
                                    detailetable.ClassCount = val;
                                    messagtable.DeliveryVisible = "Hidden";
                                    messagtable.ReceiveVisible = "Visible";
                                }
                                else
                                {
                                    detailetable.ClassName = oldMessageDetailsList[i].ClassName;
                                    detailetable.ClassCount = val;
                                    messagtable.DeliveryVisible = "Visible";
                                    messagtable.ReceiveVisible = "Hidden";
                                }
                            }
                            else
                            {
                                continue;
                            }
                            messagtable.MessageDetailsList.Add(detailetable);
                            messagtable.Time = DateTime.Now.ToShortTimeString().ToString();
                            messagtable.OperatorName = CurrentUser;
                            if (val != 0)
                            {
                                messageList.Add(messagtable);
                            }
                        }
                    }
                    else
                    {
                        MessageDetails details = new MessageDetails();
                        details.ClassCount = oldMessageDetailsList[i].ClassCount;
                        details.ClassName = oldMessageDetailsList[i].ClassName;
                        messagtable.MessageDetailsList.Add(details);
                        if (details.ClassCount > 0)
                        {
                            messagtable.DeliveryVisible = "Hidden";
                            messagtable.ReceiveVisible = "Visible";
                        }
                        else
                        {
                            messagtable.DeliveryVisible = "Visible";
                            messagtable.ReceiveVisible = "Hidden";
                        }
                        messagtable.Time = DateTime.Now.ToShortTimeString().ToString();
                        messageList.Add(messagtable);
                    }
                }

                #region 后面的
                for (int i = 0; i < length; i++)
                {
                    bool Contain = false;
                    for (int j = 0; j < oldlength; j++)
                    {
                        if (this.model.DetailsList[i].ClassName == oldMessageDetailsList[j].ClassName)
                        {
                            Contain = true;
                        }
                    }
                    if (Contain)
                    {

                    }
                    else
                    {
                        messagtable = new MessageTable();
                        MessageDetails details = new MessageDetails();
                        messagtable.MessageDetailsList = new ObservableCollection<MessageDetails>();
                        details.ClassCount = this.model.DetailsList[i].ClassCount;
                        details.ClassName = this.model.DetailsList[i].ClassName;
                        messagtable.MessageDetailsList.Add(details);
                        if (details.ClassCount > 0)
                        {
                            messagtable.DeliveryVisible = "Visible";
                            messagtable.ReceiveVisible = "Hidden";
                        }
                        else
                        {
                            messagtable.DeliveryVisible = "Hidden";
                            messagtable.ReceiveVisible = "Visible";
                        }
                        messagtable.Time = DateTime.Now.ToShortTimeString().ToString();
                        messageList.Add(messagtable);
                    }
                }
                #endregion
            }
        }
        #endregion
    }
}
