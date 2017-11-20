﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using ETextsys.Terminal.ViewModel.Choose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class TextileGroupingViewModel : ViewModelBase, IRFIDScan
    {
        private TextileGroupingModel model;
        public TextileGroupingModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private string GUID;


        private string InvNo { get; set; }

        private string CreateTime { get; set; }

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private bool _isSubmit;

        private List<ResponseRFIDTagModel> TextileList;

        private List<string> ScanList;
        /// <summary>
        /// 已收货芯片码
        /// </summary>
        private List<ResponseRFIDTagModel> ReceivedTagList;

        private DateTime ResetTime;

        public TextileGroupingViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new TextileGroupingModel();
            this.model.TextileGroupingTable = new ObservableCollection<TextileGroupingTableModel>();
            this.model.SubmitEnabled = false;
            this.model.PrintEnabled = false;
            this.model.CancelEnabled = true;
            this.model.WaitVisibled = Visibility.Hidden;

            ResetTime = DateTime.Now;
            TextileList = new List<ResponseRFIDTagModel>();
            ScanList = new List<string>();
            ReceivedTagList = new List<ResponseRFIDTagModel>();
            _isSubmit = false;
            GUID = Guid.NewGuid().ToString();

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
            if (model.TextileGroupingTable.Count > 0 && !_isSubmit)
            {
                this.model.SubmitEnabled = true;
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
                        ScanList.AddRange(tags);
                        TextileList.AddRange(temp);
                    }
                }

                TextileGroupingTableModel receiveModel = null;
                var query = from t in TextileList
                            where t.TextileState != 11
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
                this.model.TextileGroupingTable.Clear();
                query.ToList().ForEach(q =>
                {
                    receiveModel = new TextileGroupingTableModel();
                    receiveModel.BrandName = q.brandName;
                    receiveModel.ClassName = q.className;
                    receiveModel.SizeName = q.sizeName;
                    receiveModel.TextileCount = q.count;
                    total += q.count;
                    this.model.TextileGroupingTable.Add(receiveModel);
                });

                ReceivedTagList = TextileList.Where(v => v.TextileState == 11).ToList();
                this.model.ReceivedTotal = ReceivedTagList.Count;
                this.model.BtnReceiveEnabled = ReceivedTagList.Count > 0 ? true : false;

                this.model.UnRegisterTotal = ScanList.Count - total - this.model.ReceivedTotal;
                this.model.TextileCount = total;
                if (this.model.TextileGroupingTable.Count == 1)
                {
                    this.model.PackCount = TextileList.FirstOrDefault().PackCount > 0 ? TextileList.FirstOrDefault().PackCount.ToString() : "";
                }
                else
                {
                    this.model.PackCount = "";
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

            if (this.model.TextileGroupingTable.Count == 0)
            {
                EtexsysMessageBox.Show("提示", "请扫描纺织品.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var classCount = TextileList.GroupBy(v => new { v.ClassID, v.SizeID }).Count();
            if (classCount > 1)
            {
                EtexsysMessageBox.Show("提示", "多类型不能一起进行打扎.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.model.PackCount == "")
            {
                EtexsysMessageBox.Show("提示", "当前类型未设定打扎数，请先设定.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var packcount = TextileList.FirstOrDefault().PackCount;
            if (packcount != TextileList.Count)
            {
                EtexsysMessageBox.Show("提示", "当前类型打扎数量不对.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isSubmit = true;
            this.model.SubmitEnabled = false;
            ReaderController.Instance.ScanUtilities.StopScan();
            this.model.WaitVisibled = Visibility.Visible;
            this.model.WaitContent = "正在玩命提交.";
            ResetTime = DateTime.Now;

            var list = ReceivedTagList.Select(v => v.TagNo).ToList();

            ScanList.RemoveAll(v => list.Contains(v));

            RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.TagList = ScanList.ToArray();

            var rtn = await ApiController.Instance.DoPost(ApiController.Instance.TextileGrouping, requestParam);

            this.model.WaitVisibled = Visibility.Hidden;
            if (rtn.ResultCode == 1)
            {
                this.model.SubmitEnabled = true;
                EtexsysMessageBox.Show("提示", rtn.ResultMsg, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                EtexsysMessageBox.Show("提示", "打扎成功", MessageBoxButton.OK, MessageBoxImage.Information);
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
            ReceivedTagList.Clear();
            this.model.SubmitEnabled = false;
            this.model.PrintEnabled = false;
            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.TextileGroupingTable.Clear();
            InvNo = string.Empty;
            CreateTime = string.Empty;
            ScanList.Clear();
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
                GUID = Guid.NewGuid().ToString();
                _isSubmit = false; 
            }
        }

        private void ReceiveAction(object sender)
        {
            ExecutedModal modal = new View.Choose.ExecutedModal();
            ExecutedViewModel viewModel = new Choose.ExecutedViewModel(modal.Close);
            viewModel.Model.Title = "重复打扎";
            viewModel.Model.TagList = this.ReceivedTagList;
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

        private RelayCommand<object> _ReceiveCommand;
        public ICommand ReceiveCommand
        {
            get
            {
                if (_ReceiveCommand == null)
                {
                    _ReceiveCommand = new RelayCommand<object>(ReceiveAction);
                }
                return _ReceiveCommand;
            }
        }

        #endregion

    }
}
