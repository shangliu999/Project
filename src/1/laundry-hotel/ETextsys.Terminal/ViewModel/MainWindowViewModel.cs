﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Business;
using ETextsys.Terminal.View.Choose;
using ETextsys.Terminal.View.Handover;
using ETextsys.Terminal.View.Logistics;
using ETextsys.Terminal.View.QuerySummary;
using ETextsys.Terminal.View.Settings;
using ETextsys.Terminal.View.Warehouse;
using ETextsys.Terminal.ViewModel.Business;
using ETextsys.Terminal.ViewModel.Handover;
using ETextsys.Terminal.ViewModel.Logistics;
using ETextsys.Terminal.ViewModel.QuerySummary;
using ETextsys.Terminal.ViewModel.Settings;
using ETextsys.Terminal.ViewModel.Warehouse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ETextsys.Terminal.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private MainWindowModel model;
        public MainWindowModel Model
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
        /// 检测读写器是否链接
        /// </summary>
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// 页面索引
        /// </summary>
        private int pageIndex;

        private bool Prepare;

        private List<ResponseSysRightModel> parentList;

        public MainWindowViewModel()
        {
            model = new MainWindowModel();

            ObservableCollection<FuncModel> list = new ObservableCollection<FuncModel>();
            list.Add(new FuncModel() { ID = "Exit", Name = "退出", ImageUrl = "../../Skins/Default/Images/icon-tuichu.png" });
            list.Add(new FuncModel() { ID = "FBack", Name = "更多", ImageUrl = "../../Skins/Default/Images/icon-gengduo.png" });
            model.FuncM = list;

            parentList = MainWindowController.SysRights.Where(c => c.RightParentID == null).ToList();
            if (parentList != null && parentList.Count == 1)
            {
                model.LVisibility = Visibility.Hidden;
                model.RVisibility = Visibility.Hidden;
            }

            RefreshPage(pageIndex);

            model.WaitVisibled = Visibility.Hidden;
            model.WaitContent = "正在连接读写器";

            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };

            _timer.Tick += _timer_Tick;
            if (ConfigController.ReaderConfig != null)
            {
                model.WaitVisibled = Visibility.Visible;
                _timer.Start();
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            if (!App.ReaderInited)
            {
                _timer.Start();
            }
            else
            {
                if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                {
                    if (!ConfigController.ReaderConfig.IsConnection)
                    {
                        EtexsysMessageBox.Show("提示", "读写器连接失败.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                model.WaitVisibled = Visibility.Hidden;
            }

        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Prepare = true;

            //model.WaitVisibled = Visibility.Hidden;
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Prepare = false;

            //SyncController.Instance.Sync((a, b) =>
            //{
            //    if (a == 0)
            //    {
            //        model.WaitVisibled = Visibility.Hidden;
            //    }
            //    else
            //    {
            //        model.WaitContent = "正在下载数据...";
            //        model.Process = b;
            //        System.Diagnostics.Debug.WriteLine("{0},{1}", a, b);
            //        if (b >= a)
            //        {
            //            model.WaitVisibled = Visibility.Hidden;
            //        }
            //    }
            //});
        }

        #region Action

        public void RequestClose(object sender, CancelEventArgs e)
        {
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                if (ReaderController.Instance.ScanUtilities != null && ReaderController.Instance.ScanUtilities.IsStrated)
                {
                    ReaderController.Instance.Reader.StopScan();
                }
                if (ConfigController.ReaderConfig.IsConnection)
                    ReaderController.Instance.Reader.DisConnect();
            }
            Environment.Exit(0);
        }

        private void SelectedItemAction(string op)
        {
            if (!Prepare)
            {
                EtexsysMessageBox.Show("提示", "数据正在加载，请稍等...", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            System.Diagnostics.Debug.WriteLine(op);

            switch (op)
            {
                case "Reg":
                    Register reg = new Register();
                    reg.DataContext = new RegisterViewModel(() =>
                    {
                        reg.Close();
                        MainWindowController.Instance.Show();
                    });
                    reg.Show();
                    break;
                case "Scrap":
                    Scrap scrap = new Scrap();
                    scrap.DataContext = new ScrapViewModel(() =>
                    {
                        scrap.Close();
                        MainWindowController.Instance.Show();
                    });
                    scrap.Show();
                    break;
                case "Clear":
                    TextileReset tr = new TextileReset();
                    tr.DataContext = new TextileResetViewModel(() =>
                    {
                        tr.Close();
                        MainWindowController.Instance.Show();
                    });
                    tr.Show();
                    break;
                case "AssetsReg":
                    AssetsReg areg = new AssetsReg();
                    areg.DataContext = new AssetsRegViewModel(() =>
                    {
                        areg.Close();
                        MainWindowController.Instance.Show();
                    });
                    areg.Show();
                    break;
                case "Textile":
                    TextileQuery tq = new TextileQuery();
                    tq.DataContext = new TextileQueryViewModel(() =>
                    {
                        tq.Close();
                        MainWindowController.Instance.Show();
                    });
                    tq.Show();
                    break;
                case "Rec":
                    Receive receive = new Receive();
                    receive.DataContext = new ReceiveViewModel(() =>
                    {
                        receive.Close();
                        MainWindowController.Instance.Show();
                    });
                    receive.Show();
                    break;
                case "Send":
                    Send send = new Send();
                    SendViewModel svm = new SendViewModel(() =>
                    {
                        send.Close();
                        MainWindowController.Instance.Show();
                    });
                    send.DataContext = svm;
                    send.Show();
                    break;
                case "BackWash":
                    InsideBackWashing ibw = new InsideBackWashing();
                    InsideBackWashingViewModel ibwvm = new InsideBackWashingViewModel(() =>
                    {
                        ibw.Close();
                        MainWindowController.Instance.Show();
                    });
                    ibw.DataContext = ibwvm;
                    ibw.Show();
                    break;
                case "QRCodeP":
                    QRCodePrint qrcp = new QRCodePrint();
                    qrcp.DataContext = new QRCodePrintViewModel(() =>
                    {
                        qrcp.Close();
                        MainWindowController.Instance.Show();
                    });
                    qrcp.Show();
                    break;
                case "QRCodeCheck":
                    QRCodeCheckOut qrco = new QRCodeCheckOut();
                    qrco.DataContext = new QRCodeCheckViewModel(() =>
                    {
                        qrco.Close();
                        MainWindowController.Instance.Show();
                    });
                    qrco.Show();
                    break;
                case "QRCode":
                    QRCodeBinding qrcb = new QRCodeBinding();
                    qrcb.DataContext = new QRCodeBindingViewModel(() =>
                    {
                        qrcb.Close();
                        MainWindowController.Instance.Show();
                    });
                    qrcb.Show();
                    break;
                case "TagReplace":
                    RFIDReplace rr = new RFIDReplace();
                    rr.DataContext = new RFIDReplaceViewModel(() =>
                    {
                        rr.Close();
                        MainWindowController.Instance.Show();
                    });
                    rr.Show();
                    break;
                case "OutPut":
                    Delivery d = new Delivery();
                    d.DataContext = new DeliveryViewModel(() =>
                    {
                        d.Close();
                        MainWindowController.Instance.Show();
                    });
                    d.Show();
                    break;
                case "OutPutDirty":
                    DeliveryDirty dd = new DeliveryDirty();
                    dd.DataContext = new DeliveryDirtyViewModel(() =>
                    {
                        dd.Close();
                        MainWindowController.Instance.Show();
                    });
                    dd.Show();
                    break;
                case "OutPutClean":
                    DeliveryClean dc = new DeliveryClean();
                    dc.DataContext = new DeliveryCleanViewModel(() =>
                    {
                        dc.Close();
                        MainWindowController.Instance.Show();
                    });
                    dc.Show();
                    break;
                case "InPut":
                    InStorage ins = new InStorage();
                    ins.DataContext = new InStorageViewModel(() =>
                    {
                        ins.Close();
                        MainWindowController.Instance.Show();
                    });
                    ins.Show();
                    break;
                case "InPutClean":
                    InStorageClean isc = new InStorageClean();
                    isc.DataContext = new InStorageCleanViewModel(() =>
                    {
                        isc.Close();
                        MainWindowController.Instance.Show();
                    });
                    isc.Show();
                    break;
                case "InPutDirty":
                    InStorageDirty isd = new InStorageDirty();
                    isd.DataContext = new InStorageDirtyViewModel(() =>
                    {
                        isd.Close();
                        MainWindowController.Instance.Show();
                    });
                    isd.Show();
                    break;
                case "InFac":
                    InFactory inf = new InFactory();
                    inf.DataContext = new InFactoryViewModel(() =>
                    {
                        inf.Close();
                        MainWindowController.Instance.Show();
                    });
                    inf.Show();
                    break;
                case "FBack":
                    if (model.FuncM != null && model.FuncM.Count >= 5)
                    {
                        model.FuncM.Clear();
                        model.FuncM.Add(new FuncModel() { ID = "Exit", Name = "退出", ImageUrl = "../../Skins/Default/Images/icon-tuichu.png" });
                        model.FuncM.Add(new FuncModel() { ID = "FBack", Name = "更多", ImageUrl = "../../Skins/Default/Images/icon-gengduo.png" });
                    }
                    else
                    {
                        model.FuncM.Clear();
                        model.FuncM.Add(new FuncModel() { ID = "Exit", Name = "退出", ImageUrl = "../../Skins/Default/Images/icon-tuichu.png" });
                        model.FuncM.Add(new FuncModel() { ID = "FBack", Name = "返回", ImageUrl = "../../Skins/Default/Images/icon_fanhui.png" });
                        model.FuncM.Add(new FuncModel() { ID = "Logout", Name = "注销", ImageUrl = "../../Skins/Default/Images/icon-zhuxiao.png" });
                        model.FuncM.Add(new FuncModel() { ID = "About", Name = "关于", ImageUrl = "../../Skins/Default/Images/icon_guanyu.png" });
                        model.FuncM.Add(new FuncModel() { ID = "Setting", Name = "设置", ImageUrl = "../../Skins/Default/Images/icon_shezhi.png" });
                    }
                    break;
                case "Setting":
                    SettingMain busSetting = new SettingMain();
                    busSetting.DataContext = new SettingMainViewModel(() =>
                    {
                        busSetting.Close();
                        MainWindowController.Instance.Show();
                    });
                    BusinessSetting bs = new BusinessSetting();
                    BusinessSettingViewModel bsvm = new ViewModel.Settings.BusinessSettingViewModel();
                    bs.DataContext = bsvm;
                    busSetting.contentPanel.Children.Add(bs);
                    busSetting.contentPanel.Tag = "Business";
                    busSetting.Show();
                    break;
                case "Exit":
                    if (ReaderController.Instance.ScanUtilities != null)
                    {
                        ReaderController.Instance.ScanUtilities.StopScan();
                    }
                    App.Current.Shutdown();
                    break;
                case "Logout":
                    Login login = new Login();
                    LoginViewModel lvm = new LoginViewModel(login.password, () =>
                    {
                        login.Close();
                        MainWindowController.Instance.Show();
                    });
                    login.Show();
                    break;
                case "Invoice":
                    BillInquiry billInquiry = new BillInquiry();
                    billInquiry.DataContext = new BillInquiryViewModel(() =>
                    {
                        billInquiry.Close();
                        MainWindowController.Instance.Show();
                    });
                    billInquiry.Show();
                    break;
                case "Summary":
                    BillInquiry02 billInquiry02 = new BillInquiry02();
                    billInquiry02.DataContext = new BillInquiry02ViewModel(() =>
                    {
                        billInquiry02.Close();
                        MainWindowController.Instance.Show();
                    });
                    billInquiry02.Show();
                    break;
                case "About":
                    About about = new About();
                    about.DataContext = new AboutViewModel(() =>
                    {
                        about.Close();
                        MainWindowController.Instance.Show();
                    });
                    about.Show();
                    break;
                case "TextileGrouping":
                    TextileGrouping tg = new TextileGrouping();
                    tg.DataContext = new TextileGroupingViewModel(() =>
                    {
                        tg.Close();
                        MainWindowController.Instance.Show();
                    });
                    tg.Show();
                    break;
                case "RemoveTextileGrouping":
                    RemoveTextileGrouping rtg = new RemoveTextileGrouping();
                    rtg.DataContext = new RemoveTextileGroupingViewModel(() =>
                    {
                        rtg.Close();
                        MainWindowController.Instance.Show();
                    });
                    rtg.Show();
                    break;
                case "TextileMerge":
                    TextileMerge tm = new TextileMerge();
                    tm.DataContext = new TextileMergeViewModel(() =>
                    {
                        tm.Close();
                        MainWindowController.Instance.Show();
                    });
                    tm.Show();
                    break;
                case "Back":
                    Return rtn = new Return();
                    rtn.DataContext = new ReturnViewModel(() =>
                    {
                        rtn.Close();
                        MainWindowController.Instance.Show();
                    });
                    rtn.Show();
                    break;
                case "FactoryBackWashing":
                    FactoryBackWashing fbw = new FactoryBackWashing();
                    fbw.DataContext = new FactoryBackWashingViewModel(() =>
                    {
                        fbw.Close();
                        MainWindowController.Instance.Show();
                    });
                    fbw.Show();
                    break;
            }

            if (!(op == "FBack" || op == "Exit" || op == "About"))
            {
                MainWindowController.Instance.Hide();
            }
        }

        private void ChangePageAction(string op)
        {
            int max = parentList.Count - 1;
            if (op == "L")
            {
                pageIndex--;
                if (pageIndex < 0)
                {
                    pageIndex = max;
                }
            }
            else
            {
                pageIndex++;
                if (pageIndex > max)
                {
                    pageIndex = 0;
                }
            }
            RefreshPage(pageIndex);
        }

        #endregion

        #region Command

        private RelayCommand<string> _SelectionChangedCmd;
        public ICommand SelectionChangedCmd
        {
            get
            {
                if (_SelectionChangedCmd == null)
                {
                    _SelectionChangedCmd = new RelayCommand<string>(SelectedItemAction);
                }
                return _SelectionChangedCmd;
            }
        }

        //ChangePageCommond
        private RelayCommand<string> _ChangePageCommond;
        public ICommand ChangePageCommond
        {
            get
            {
                if (_ChangePageCommond == null)
                {
                    _ChangePageCommond = new RelayCommand<string>(ChangePageAction);
                }
                return _ChangePageCommond;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="i">1：第一页 0：第二页</param>
        private void RefreshPage(int pageIndex)
        {
            ResponseSysRightModel parent = parentList[pageIndex];
            List<ResponseSysRightModel> current = MainWindowController.SysRights.Where(c => c.RightParentID == parent.RightID).ToList();

            model.Title = parent.RightName;

            List<ResponseSysRightModel> qrcode = current.Where(c => c.RightUrl.Contains("QRCode")).ToList();
            ObservableCollection<FuncModel> list = null;
            ResponseSysRightModel item = null;
            if (qrcode != null && qrcode.Count > 0)
            {
                list = new ObservableCollection<FuncModel>();
                for (int i = 0; i < qrcode.Count; i++)
                {
                    item = qrcode[i];
                    list.Add(new FuncModel() { ID = item.RightUrl, Name = item.RightName, ImageUrl = string.Format("../../Skins/Default/Images/{0}", item.RightIcon), IsEnabled = true });
                }
                model.FuncInFactory = list;
                model.QTop = 150;
            }
            else
            {
                if (model.FuncInFactory != null && model.FuncInFactory.Count > 0)
                {
                    model.FuncInFactory.Clear();
                }
                model.QTop = 20;
            }

            List<ResponseSysRightModel> other = current.Where(c => !c.RightUrl.Contains("QRCode")).OrderBy(c => c.RightSort).ToList();
            if (other != null && other.Count > 0)
            {
                list = new ObservableCollection<FuncModel>();
                for (int i = 0; i < other.Count; i++)
                {
                    item = other[i];
                    list.Add(new FuncModel() { ID = item.RightUrl, Name = item.RightName, ImageUrl = string.Format("../../Skins/Default/Images/{0}", item.RightIcon), IsEnabled = true });
                }
                model.FuncInC = list;
            }

            list = new ObservableCollection<FuncModel>();
            list.Add(new FuncModel() { ID = "Textile", Name = "纺织品查询", ImageUrl = "../../Skins/Default/Images/icon_home_fangzhipinzhuizong.png" });
            list.Add(new FuncModel() { ID = "Invoice", Name = "单据查询", ImageUrl = "../../Skins/Default/Images/icon_home_danjuchaxun.png" });
            list.Add(new FuncModel() { ID = "Summary", Name = "汇总", ImageUrl = "../../Skins/Default/Images/icon_home_huizong.png" });
            model.FuncQ = list;
        }

        private bool GetEnable(string rightname)
        {
            return MainWindowController.SysRights.Select(c => c.RightName).Contains(rightname);
        }

        #endregion
    }
}
