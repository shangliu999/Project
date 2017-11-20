using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class QRCodeCheckViewModel : ViewModelBase, IRFIDScan
    {
        private QRCodeCheckOutModel model;
        public QRCodeCheckOutModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        private ResponseRFIDTagModel TextileModel;

        private string QRCode = string.Empty;

        private string PreTagNo = string.Empty;

        private DateTime PreTime = DateTime.MinValue;

        private bool isLock;

        /// <summary>
        /// 窗体关闭Action
        /// </summary>
        Action _closeAction;


        public QRCodeCheckViewModel(Action closeAction)
        {
            this.model = new QRCodeCheckOutModel();
            this._closeAction = closeAction;
            isLock = false;
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                this.model.ReaderState = ConfigController.ReaderConfig.IsConnection ? 2 : 0;
                if (ConfigController.ReaderConfig.IsConnection)
                {
                    ReaderController.Instance.ScanUtilities.OpenSubmitTime = 1;
                    ReaderController.Instance.ScanUtilities.StartScan(this, () =>
                    {
                        model.ReaderState = ReaderController.Instance.ScanUtilities.IsStrated ? 1 : 0;
                        model.ReaderLight = ReaderController.Instance.ScanUtilities.IsStrated ? new Uri(ApiController.Scan, UriKind.RelativeOrAbsolute) : new Uri(ApiController.NotScan, UriKind.RelativeOrAbsolute);
                    });
                    ReaderController.Instance.ScanUtilities.ClearScanTags();
                }
            }

            this.model.RFIDReaderVisibility = Visibility.Visible;
            this.model.QRCodeScanVisibility = Visibility.Visible;
            this.model.QRCodeVisibility = Visibility.Collapsed;
            this.model.BindingState = "";// new Uri("../../Skins/Default/Images/img-bangdingzhong.png", UriKind.RelativeOrAbsolute);
            this.model.RFIDText = "正在扫描纺织品...";
            this.model.QRCodeText = "正在检测二维码...";
            this.model.RFIDColor = "#EAEAEA";
            this.model.QRCodeColor = "#EAEAEA";
            this.model.RFIDTextColor = "#288DFF";
            this.model.QRCodeTextColor = "#288DFF";
            this.model.RFIDTag = "";
        }

        #region RFID Interface

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            if (tags.Count == 0)
            {
                return;
            }
            if (tags.Count > 1)
            {
                this.model.RFIDText = "扫描到多件纺织品.";
                this.model.RFIDColor = "#FC6464";
                this.model.RFIDTextColor = "#FFF";
                this.model.RFIDTag = "";
                Thread.Sleep(300);
                TextileModel = null;
                ReaderController.Instance.ScanUtilities.ClearScanTags();
                return;
            }

            RFIDTagAnalysisParamModel requestParam = new RFIDTagAnalysisParamModel();
            requestParam.TagList = tags.ToArray();
            requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
            requestParam.UUID = ConfigController.MacCode;
            requestParam.TerminalType = ConfigController.TerminalType;

            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.RFIDTagAnalysis, requestParam);
            if (apiRtn.ResultCode == 0)
            {
                List<ResponseRFIDTagModel> temp = null;
                if (apiRtn.Result != null)
                {
                    temp = JsonConvert.DeserializeObject<List<ResponseRFIDTagModel>>(apiRtn.Result.ToString());
                    if (temp == null || temp.Count == 0)
                    {
                        this.model.RFIDReaderVisibility = Visibility.Visible;
                        this.model.RFIDText = "纺织品未登记.";
                        this.model.RFIDColor = "#FC6464";
                        this.model.RFIDTextColor = "#FFF";
                        this.model.RFIDTag = "";
                        this.model.ClassName = "";
                        Thread.Sleep(300);
                        ReaderController.Instance.ScanUtilities.ClearScanTags();
                        TextileModel = null;
                    }
                    else
                    {
                        if (temp.Count == 1)
                        {
                            TextileModel = temp.FirstOrDefault();
                            if (PreTagNo != TextileModel.TagNo)
                            {
                                PreTagNo = TextileModel.TagNo;
                                ReaderController.Instance.ScanUtilities.ClearScanTags();
                            }
                            this.model.RFIDReaderVisibility = Visibility.Collapsed;
                            this.model.RFIDText = "纺织品合格.";
                            this.model.RFIDColor = "#21AF37";
                            this.model.RFIDTextColor = "#FFF";
                            this.model.RFIDTag = TextileModel.TagNo;
                            this.model.ClassName = TextileModel.ClassName;

                        }
                        else
                        {
                            this.model.RFIDReaderVisibility = Visibility.Visible;
                            TextileModel = null;
                            this.model.RFIDText = "扫描到多件纺织品.";
                            this.model.RFIDColor = "#FC6464";
                            this.model.RFIDTextColor = "#FFF";
                            this.model.RFIDTag = "";
                            this.model.ClassName = "";
                            ReaderController.Instance.ScanUtilities.ClearScanTags();
                        }
                    }
                }
            }
        }

        public async void NoScanTag()
        {
            if (TextileModel != null)
            {
                this.model.RFIDText = "纺织品合格.";
                this.model.RFIDColor = "#21AF37";
                this.model.RFIDTextColor = "#FFF";
                this.model.RFIDTag = TextileModel.TagNo;
                this.model.ClassName = TextileModel.ClassName;
                if (!string.IsNullOrEmpty(QRCode) && QRCode.Length > 10)
                {
                    if (!isLock)
                    {
                        isLock = true;
                        QRCodeBindingParamModel requestParam = new QRCodeBindingParamModel();
                        requestParam.QRCode = QRCode;
                        requestParam.RFIDTagNo = TextileModel.TagNo;
                        requestParam.TimeStamp = ApiController.Instance.GetTimeStamp();
                        requestParam.UUID = ConfigController.MacCode;
                        requestParam.TerminalType = ConfigController.TerminalType;

                        var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.QRCodeCheckOut, requestParam);
                        if (apiRtn.ResultCode == 0)
                        {
                            this.model.BindingState = "校验成功.";// new Uri("../../Skins/Default/Images/img-bangdingchenggong.png", UriKind.RelativeOrAbsolute);
                            this.model.BindingStateColor = "#21AF37";
                        }
                        else
                        {
                            this.model.BindingState = "校验失败.";// new Uri("../../Skins/Default/Images/img-bangdingshibai.png", UriKind.RelativeOrAbsolute);
                            this.model.BindingStateColor = "#FC6464";
                            Thread.Sleep(300);
                            TextileModel = null;
                            ReaderController.Instance.ScanUtilities.ClearScanTags();
                        }
                        QRCode = string.Empty;
                        TextileModel = null;
                        PreTime = DateTime.Now;

                        isLock = false;
                    }
                }
                else
                {
                    this.model.BindingState = "";//new Uri("../../Skins/Default/Images/img-bangdingzhong.png", UriKind.RelativeOrAbsolute);
                    QRCode = string.Empty;
                    this.model.QRCodeText = "正在检测二维码...";
                    this.model.QRCodeTextColor = "#288DFF";
                    this.model.QRCodeColor = "#EAEAEA";
                    this.model.QRCodeScanVisibility = Visibility.Visible;
                    this.model.QRCodeVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                TimeSpan ts = DateTime.Now - PreTime;
                if (ts.TotalSeconds > 2)
                {
                    this.model.BindingState = "";// new Uri("../../Skins/Default/Images/img-bangdingzhong.png", UriKind.RelativeOrAbsolute);
                    this.model.RFIDText = "正在扫描纺织品...";
                    this.model.RFIDColor = "#EAEAEA";
                    this.model.RFIDTextColor = "#288DFF";
                    this.model.RFIDReaderVisibility = Visibility.Visible;
                    this.model.RFIDTag = "";
                    this.model.ClassName = "";
                    TextileModel = null;

                    if (string.IsNullOrEmpty(QRCode))
                    {
                        this.model.QRCodeText = "正在检测二维码...";
                        this.model.QRCodeTextColor = "#288DFF";
                        this.model.QRCodeColor = "#EAEAEA";
                        this.model.QRCodeScanVisibility = Visibility.Visible;
                        this.model.QRCodeVisibility = Visibility.Collapsed;
                    }
                }
            }

        }

        #endregion

        #region Action

        private void CloseModalAction(string sender)
        {
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.StopScan();
                ReaderController.Instance.ScanUtilities.OpenSubmitTime = 3;
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
                    this.model.ReaderLight = new Uri(ApiController.StopScan, UriKind.RelativeOrAbsolute);
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

        private void QRCodeScanAction(object sender)
        {
            if (this.model.QRCode == null) { return; }

            try
            {
                string _qrCode = this.model.QRCode.Trim();
                string temp = _qrCode.Substring(3, _qrCode.Length - 8);
                int n = 0;
                foreach (var c in temp)
                {
                    n += int.Parse(c.ToString());
                }
                int no = Convert.ToInt32(_qrCode.Substring(_qrCode.Length - 5, 4));
                n += no;
                string jy = n.ToString().Remove(0, n.ToString().Length - 1);
                if (jy != _qrCode.Substring(_qrCode.Length - 1, 1))
                {
                    this.model.QRCodeText = "二维码不合法.";
                    this.model.QRCodeColor = "#FC6464";
                    this.model.QRCodeTextColor = "#FFF";
                    QRCode = string.Empty;
                    this.model.QRCodeScanVisibility = Visibility.Visible;
                    this.model.QRCodeVisibility = Visibility.Collapsed;
                }
                else
                {
                    this.model.QRCodeText = "二维码合法.";
                    this.model.QRCodeColor = "#21AF37";
                    this.model.QRCodeTextColor = "#FFF";
                    this.model.QRCodeScanVisibility = Visibility.Hidden;
                    this.model.QRCodeVisibility = Visibility.Visible;
                    QRCode = _qrCode;
                }
                this.model.QRCode = "";
            }
            catch
            {
                this.model.QRCodeText = "二维码不合法.";
                this.model.QRCodeColor = "#FC6464";
                this.model.QRCodeTextColor = "#FFF";
                QRCode = string.Empty;
                this.model.QRCodeScanVisibility = Visibility.Visible;
                this.model.QRCodeVisibility = Visibility.Collapsed;
                this.model.QRCode = "";
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
