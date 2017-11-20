﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Logistics;
using ETextsys.Terminal.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ETextsys.Terminal.ViewModel.Choose
{
    public class UnDoViewModel : ViewModelBase, IRFIDScan
    {
        Action _closeAction;

        private UnDoModel model;
        public UnDoModel Model
        {
            get { return model; }

            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private bool _isSubmit;
        public bool IsSubmit
        {
            get { return _isSubmit; }
            set
            {
                _isSubmit = value;
            }
        }

        public List<ResponseRFIDTagModel> TextileList { get; set; }

        /// <summary>
        /// 撤销时获取实发数据
        /// </summary>
        public ObservableCollection<SendTableModel> SendTable { get; set; }

        private List<string> ScanList;

        private DateTime ResetTime;


        public UnDoViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            ResetTime = DateTime.Now;
            ScanList = new List<string>();
            TextileList = new List<ResponseRFIDTagModel>();

            model = new UnDoModel();
            model.WaitVisibled = Visibility.Hidden;
            model.UnDoTable = new ObservableCollection<UnDoTableModel>();
            model.ReaderState = 2;
            model.StateColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));

            model.Title = "撤销中";
        }

        #region RFID Interface

        public void NoScanTag()
        {
            if (model.UnDoTable.Count > 0 && !_isSubmit)
            {
                model.SubmitEnabled = true;
            }
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            if (tags.Count == 0)
            {
                return;
            }
            this.model.SubmitEnabled = false;
            this.model.CancelEnabled = true;

            ScanList.AddRange(tags);

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
                    if (temp != null && d > ResetTime)
                    {
                        TextileList.AddRange(temp);
                    }
                }

                UnDoTableModel undoModel = null;
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
                model.UnDoTable.Clear();
                query.ToList().ForEach(q =>
                {
                    undoModel = new UnDoTableModel();
                    undoModel.BrandName = q.brandName;
                    undoModel.ClassName = q.className;
                    undoModel.SizeName = q.sizeName;

                    int actualsend = 0;
                    if (SendTable != null)
                    {
                        string sizename = string.IsNullOrEmpty(q.sizeName) ? "" : q.sizeName;
                        SendTableModel stm = SendTable.FirstOrDefault(c => c.BrandName == q.brandName && c.ClassName == q.className && c.SizeName == sizename);
                        if (stm != null)
                        {
                            actualsend = stm.TextileCount;
                        }
                    }

                    undoModel.TextileCount = actualsend;
                    undoModel.UnDoCount = q.count;
                    total += q.count;
                    model.UnDoTable.Add(undoModel);
                });

                this.model.UnRegisterTotal = ScanList.Count - total;
                this.model.TextileCount = total;
            }
        }

        #endregion

        #region Action

        private void SubmitAction(object sender)
        {
            _isSubmit = true;

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
                    //model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri("../../Skins/Default/Images/scan.gif", UriKind.RelativeOrAbsolute) : new Uri("../../Skins/Default/Images/stopscan.gif", UriKind.RelativeOrAbsolute);
                }
            }

            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void CancelAction(object sender)
        {
            if (ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            ResetTime = DateTime.Now;
            TextileList.Clear();
            model.SubmitEnabled = false;
            model.UnRegisterTotal = 0;
            model.TextileCount = 0;
            model.UnDoTable.Clear();
            ScanList.Clear();

            if (_isSubmit)
            {
                _isSubmit = false;
                if (ReaderController.Instance.ScanUtilities != null)
                {
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

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            model.StateColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                        });
                    }, () =>
                    {
                        this.model.WaitVisibled = Visibility.Hidden;
                        this.model.WaitContent = "正在玩命提交.";
                    });
                }
            }
        }

        private void CloseModalAction(string sender)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void StartAction(string sender)
        {
            if (model.ReaderState == 2)
            {
                if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                {
                    model.ReaderState = ConfigController.ReaderConfig.IsConnection ? 2 : 0;
                    if (ConfigController.ReaderConfig.IsConnection)
                    {
                        this.model.WaitVisibled = Visibility.Visible;
                        this.model.WaitVisibled = Visibility.Hidden;
                        this.model.WaitContent = "刷新读写器.";
                        System.Threading.Thread.Sleep(50);
                        this.model.WaitVisibled = Visibility.Visible;
                        ReaderController.Instance.ScanUtilities.ClearScanTags();
                        ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                        {
                            this.model.WaitVisibled = Visibility.Hidden;
                            this.model.WaitContent = "正在玩命提交.";
                            model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                model.StateColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                            });
                        }, () =>
                        {
                            this.model.WaitVisibled = Visibility.Hidden;
                            this.model.WaitContent = "正在玩命提交.";
                        });
                    }
                }
            }
            else
            {
                if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
                {
                    model.ReaderState = ConfigController.ReaderConfig.IsConnection ? 2 : 0;
                    if (ConfigController.ReaderConfig.IsConnection)
                    {
                        ReaderController.Instance.ScanUtilities.StopScan();
                        ReaderController.Instance.ScanUtilities.ClearScanTags();
                        System.Threading.Thread.Sleep(50);
                        model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 2;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            model.StateColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                        });
                    }
                }
            }
        }

        #endregion

        #region Command

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

        private RelayCommand<string> _StartCommand;
        public ICommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new RelayCommand<string>(StartAction);
                }
                return _StartCommand;
            }
        }

        #endregion
    }
}
