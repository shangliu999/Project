using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class RFIDReplaceViewModel : ViewModelBase, IRFIDScan
    {
        private RFIDReplaceModel model;
        public RFIDReplaceModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;

        private DateTime PreScanTime { get; set; }

        private List<string> tagList { get; set; }

        private int TextileID { get; set; }

        private DateTime ResetTime;

        public RFIDReplaceViewModel(Action closeAction)
        {
            this.model = new RFIDReplaceModel();
            ResetTime = DateTime.Now;
            this._closeAction = closeAction;
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

            this.model.NewIconVIsibled = Visibility.Collapsed;
            this.model.OldIconVIsibled = Visibility.Collapsed;

            this.model.OldReaderIconVisibled = Visibility.Visible;
            this.model.NewReaderIconVisibled = Visibility.Visible;

            this.model.ReplaceBtnEnable = false;
            PreScanTime = DateTime.MinValue;
            tagList = new List<string>();
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            if (tags.Count == 0)
            {
                return;
            }
            if (tags.Count > 2)
            {
                this.model.Prompt = "扫描到多个芯片.";
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            else
            {

                RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
                requestParam.TagList = tags.ToArray();
                requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                requestParam.UUID = ConfigController.MacCode;
                requestParam.TerminalType = ConfigController.TerminalType;
                requestParam.RequestTime = DateTime.Now;

                tagList.Clear();
                tagList.AddRange(tags);

                var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RFIDTagAnalysis, requestParam);
                if (apiRtn.ResultCode == 0)
                {
                    if (apiRtn.Result != null && apiRtn.OtherResult != null)
                    {
                        DateTime d = Convert.ToDateTime(apiRtn.OtherResult);
                        var temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                        if (temp != null && d > ResetTime)
                        {
                            if (temp.Count == 1)
                            {
                                var query = from t in temp
                                            select new
                                            {
                                                brandName = t.BrandName,
                                                className = t.ClassName,
                                                sizeName = t.SizeName,
                                                washingTime = t.Washtimes,
                                                left = t.ClassLeft,
                                                tagNo = t.TagNo,
                                                id = t.ID,
                                                costTime = t.RFIDCostTime,
                                                rfidWashingTime = t.RFIDWashtime,
                                            };

                                var q = query.First();
                                this.model.BrandName = q.brandName;
                                this.model.ClassName = q.className;
                                this.model.SizeName = q.sizeName;
                                this.model.WashingTime = q.washingTime.ToString();
                                this.model.Left = q.left == 0 ? "" : q.left.ToString();

                                TimeSpan ts = q.costTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                this.model.CostTime = ApiController.Instance.GetTime(ts.TotalSeconds.ToString()).ToString("yyyy-MM-dd HH:mm:ss");

                                this.model.RFIDWashingTime = q.rfidWashingTime.ToString();

                                this.model.OldRFIDTagNo = q.tagNo;
                                TextileID = q.id;
                                if (tagList.Count == 2)
                                {
                                    this.model.NewRFIDTagNo = tagList.Where(v => v != q.tagNo).FirstOrDefault();
                                    this.model.OldReaderIconVisibled = Visibility.Collapsed;
                                    this.model.NewReaderIconVisibled = Visibility.Collapsed;

                                    this.model.NewIconVIsibled = Visibility.Visible;
                                    this.model.OldIconVIsibled = Visibility.Visible;

                                    this.model.ReplaceBtnEnable = true;
                                }
                                else if (tagList.Count == 1)
                                {
                                    if (model.NewRFIDTagNo != "" && model.NewRFIDTagNo != null)
                                    {
                                        this.model.ReplaceBtnEnable = true;
                                    }
                                }

                                this.model.Prompt = "";

                            }
                            else
                            {
                                if (tagList.Count == 1)
                                {
                                    this.model.NewRFIDTagNo = tagList.FirstOrDefault();

                                    if (this.model.OldRFIDTagNo != "" && this.model.NewRFIDTagNo != null)
                                    {
                                        this.model.OldReaderIconVisibled = Visibility.Collapsed;
                                        this.model.NewReaderIconVisibled = Visibility.Collapsed;

                                        this.model.NewIconVIsibled = Visibility.Visible;
                                        this.model.OldIconVIsibled = Visibility.Visible;

                                        this.model.Prompt = "";
                                        this.model.ReplaceBtnEnable = true;
                                    }
                                    else
                                    {
                                        this.model.OldReaderIconVisibled = Visibility.Visible;
                                        this.model.NewReaderIconVisibled = Visibility.Collapsed;

                                        this.model.NewIconVIsibled = Visibility.Visible;
                                        this.model.OldIconVIsibled = Visibility.Collapsed;

                                        this.model.Prompt = "";
                                        this.model.ReplaceBtnEnable = false;
                                    }
                                }
                                else
                                {
                                    this.model.NewIconVIsibled = Visibility.Collapsed;
                                    this.model.OldIconVIsibled = Visibility.Collapsed;

                                    this.model.OldReaderIconVisibled = Visibility.Visible;
                                    this.model.NewReaderIconVisibled = Visibility.Visible;
                                    this.model.Prompt = "扫描到两个新芯片或者两个旧芯片.";
                                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                                }

                            }
                        }
                    }
                }
            }
        }

        void IRFIDScan.NoScanTag()
        {

        }

        #region Action

        private void CloseModalAction(string sender)
        {
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.StopScan();
            }

            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

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

        private void CancelAction(object sender)
        {
            if (ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.ClearScanTags();
            }
            ResetTime = DateTime.Now;
            this.model.OldRFIDTagNo = "";
            this.model.NewRFIDTagNo = "";
            this.model.OldIconVIsibled = Visibility.Collapsed;
            this.model.OldReaderIconVisibled = Visibility.Visible;
            this.model.NewIconVIsibled = Visibility.Collapsed;
            this.model.NewReaderIconVisibled = Visibility.Visible;
            this.model.BrandName = "";
            this.model.ClassName = "";
            this.model.SizeName = "";
            this.model.Left = "";
            this.model.WashingTime = "";
            this.model.CostTime = "";
            this.model.RFIDWashingTime = "";
            this.model.Prompt = "";
            this.model.ReplaceBtnEnable = false;
            TextileID = 0;
            tagList.Clear();
        }

        private async void ReplaceAction(object sender)
        {
            RFIDReplaceParamModel requestParam = new RFIDReplaceParamModel();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;
            requestParam.NewTagNo = this.model.NewRFIDTagNo;
            requestParam.OldTagNo = this.model.OldRFIDTagNo;
            requestParam.CreateUserId = App.CurrentLoginUser.UserID;
            requestParam.TextileID = TextileID;

            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RDIFReplace, requestParam);
            if (apiRtn.ResultCode == 0)
            {
                this.model.Prompt = "更换成功.";
                this.model.ReplaceBtnEnable = false;
            }
            else
            {
                this.model.Prompt = "更换失败.";
            }
        }


        private async void QRCodeScanAction(object sender)
        {
            if (this.model.OldRFIDTagNo == null) { return; }

            try
            {
                string _qrCode = this.model.OldRFIDTagNo.Trim();
                string strtemp = _qrCode.Substring(3, _qrCode.Length - 8);
                int n = 0;
                foreach (var c in strtemp)
                {
                    n += int.Parse(c.ToString());
                }
                int no = Convert.ToInt32(_qrCode.Substring(_qrCode.Length - 5, 4));
                n += no;
                string jy = n.ToString().Remove(0, n.ToString().Length - 1);
                if (jy == _qrCode.Substring(_qrCode.Length - 1, 1))
                {
                    RFIDRelaceQRCodeParamModel requestParam = new RFIDRelaceQRCodeParamModel();
                    requestParam.QRCode = this.model.OldRFIDTagNo;
                    requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                    requestParam.UUID = ConfigController.MacCode;
                    requestParam.TerminalType = ConfigController.TerminalType;
                    requestParam.RequestTime = DateTime.Now;

                    var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RFIDTagAnalysisByQRCode, requestParam);
                    if (apiRtn.ResultCode == 0)
                    {
                        if (apiRtn.Result != null && apiRtn.OtherResult != null)
                        {
                            var temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                            if (temp.Count == 1)
                            {
                                var query = from t in temp
                                            select new
                                            {
                                                brandName = t.BrandName,
                                                className = t.ClassName,
                                                sizeName = t.SizeName,
                                                washingTime = t.Washtimes,
                                                left = t.ClassLeft,
                                                tagNo = t.TagNo,
                                                id = t.ID,
                                                costTime = t.RFIDCostTime,
                                                rfidWashingTime = t.RFIDWashtime,
                                            };

                                var q = query.First();
                                this.model.BrandName = q.brandName;
                                this.model.ClassName = q.className;
                                this.model.SizeName = q.sizeName;
                                this.model.WashingTime = q.washingTime.ToString();
                                this.model.Left = q.left == 0 ? "" : q.left.ToString();

                                TimeSpan ts = q.costTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                this.model.CostTime = ApiController.Instance.GetTime(ts.TotalSeconds.ToString()).ToString("yyyy-MM-dd HH:mm:ss");

                                this.model.RFIDWashingTime = q.rfidWashingTime.ToString();

                                this.model.OldRFIDTagNo = q.tagNo;
                                TextileID = q.id;
                                if (tagList.Count == 2)
                                {
                                    this.model.NewRFIDTagNo = tagList.Where(v => v != q.tagNo).FirstOrDefault();
                                    this.model.OldReaderIconVisibled = Visibility.Collapsed;
                                    this.model.NewReaderIconVisibled = Visibility.Collapsed;

                                    this.model.NewIconVIsibled = Visibility.Visible;
                                    this.model.OldIconVIsibled = Visibility.Visible;

                                    this.model.ReplaceBtnEnable = true;
                                }
                                else if (tagList.Count == 1)
                                {
                                    if (model.NewRFIDTagNo != "" && model.NewRFIDTagNo != null)
                                    {
                                        this.model.ReplaceBtnEnable = true;
                                    }
                                }

                                this.model.Prompt = "";

                            }
                        }
                    }
                }
                else
                {
                    this.model.Prompt = "请扫描纺织品上的二维码.";
                    this.model.OldRFIDTagNo = "";
                }
            }
            catch
            {
                this.model.Prompt = "请扫描纺织品上的二维码.";
                this.model.OldRFIDTagNo = "";
            }
        }

        #endregion

        #region Command

        private RelayCommand<object> _replaceCommand;
        public ICommand ReplaceCommand
        {
            get
            {
                if (_replaceCommand == null)
                {
                    _replaceCommand = new RelayCommand<object>(ReplaceAction);
                }
                return _replaceCommand;
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

        private RelayCommand<object> _QRCodeScanCommand;
        public RelayCommand<object> QRCodeScanCommand
        {
            get
            {
                if (_QRCodeScanCommand == null)
                {
                    _QRCodeScanCommand = new RelayCommand<object>(QRCodeScanAction);
                }
                return _QRCodeScanCommand;
            }
        }

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



        #endregion
    }
}
