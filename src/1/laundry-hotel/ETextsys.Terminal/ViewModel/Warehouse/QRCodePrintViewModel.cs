﻿
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Model.Warehouse;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.Utilities.PrintBase;
using ETextsys.Terminal.View.Choose;
using ETextsys.Terminal.ViewModel.Choose;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace ETextsys.Terminal.ViewModel.Warehouse
{
    public class QRCodePrintViewModel : ViewModelBase
    {
        private QRCodePrintModel model;
        public QRCodePrintModel Model
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

        EncodingOptions options = null;
        BarcodeWriter writer = null;

        private ObservableCollection<ChooseModel> ClassList = null;

        public QRCodePrintViewModel(Action closeAction)
        {
            this.model = new Terminal.Model.Warehouse.QRCodePrintModel();
            this._closeAction = closeAction;
            options = new QrCodeEncodingOptions
            {
                CharacterSet = "UTF-8",
                PureBarcode = true,
                Width = 200,
                Height = 200,
                Margin = 0,
                ErrorCorrection = ErrorCorrectionLevel.H
            };
            ClassList = new ObservableCollection<ChooseModel>();
            ClassList.Add(new ChooseModel { ChooseID = 1, ChooseName = "床单" });
            ClassList.Add(new ChooseModel { ChooseID = 2, ChooseName = "被套" });
            ClassList.Add(new ChooseModel { ChooseID = 3, ChooseName = "枕套" });
            ClassList.Add(new ChooseModel { ChooseID = 4, ChooseName = "毛巾" });
            ClassList.Add(new ChooseModel { ChooseID = 5, ChooseName = "浴巾" });
            ClassList.Add(new ChooseModel { ChooseID = 6, ChooseName = "地巾" });
        }

        #region Action

        private void CloseModalAction(string sender)
        {
            if (ConfigController.ReaderConfig != null)
            {
                ReaderController.Instance.ScanUtilities.StopScan();
            }

            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void PrintAction(object sender)
        {
            if (this.model.ClassNo == null || this.model.ClassNo == "")
            {
                EtexsysMessageBox.Show("提示", "请选择品名.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.PrintCount <= 0)
            {
                EtexsysMessageBox.Show("提示", "请输入打印数量.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (this.model.PrintCount > 10000)
            {
                EtexsysMessageBox.Show("提示", "每次打印不超过300个.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            PrintQueue pq = new PrintQueue();
            PrintAttachment att = null;

            string px = this.model.ClassNo;
            string timeStamp = ApiController.Instance.GetTimeStamp();
            int t = 0;
            foreach (var c in timeStamp)
            {
                t += int.Parse(c.ToString());
            }
            string jy = string.Empty;
            for (int i = 1; i <= this.model.PrintCount; i++)
            {
                att = new PrintAttachment();
                att.PrintType = 0;

                jy = (t + i).ToString();
                string no = string.Format("F{0}{1}{2}{3}", px, timeStamp, i.ToString().PadLeft(4, '0'), jy.Remove(0, jy.Length - 1));
                writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.QR_CODE;
                writer.Options = options;
                att.FirstQRImage = writer.Write(no);   

                if (i + 1 <= this.model.PrintCount)
                {
                    i++;
                    jy = (t + i).ToString();
                    no = string.Format("F{0}{1}{2}{3}", px, timeStamp, i.ToString().PadLeft(4, '0'), jy.Remove(0, jy.Length - 1));
                    writer = new BarcodeWriter();
                    writer.Format = BarcodeFormat.QR_CODE;
                    writer.Options = options;
                    att.SecondQRImage = writer.Write(no);
                }

                if (i + 1 <= this.model.PrintCount)
                {
                    i++;
                    jy = (t + i).ToString();
                    no = string.Format("F{0}{1}{2}{3}", px, timeStamp, i.ToString().PadLeft(4, '0'), jy.Remove(0, jy.Length - 1));
                    writer = new BarcodeWriter();
                    writer.Format = BarcodeFormat.QR_CODE;
                    writer.Options = options;
                    att.ThirdQRImage = writer.Write(no);
                }
                pq.Add(att);
            }
            pq.Print();
        }

        private void ChooseAttrAction(string sender)
        {

            ObservableCollection<ChooseModel> list = new ObservableCollection<Terminal.Model.Choose.ChooseModel>();
            string title = "";
            switch (sender)
            {
                case "Class":
                    title = "请选择品名";
                    list = ClassList;
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
                    case "Class":
                        this.model.ClassName = modalModel.ChooseItem.ChooseName;
                        this.model.ClassNo = modalModel.ChooseItem.ChooseID.ToString().PadLeft(2, '0');
                        break;
                }
            }
        }
        #endregion

        #region Command

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

        private RelayCommand<object> _printCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                {
                    _printCommand = new RelayCommand<object>(PrintAction);
                }
                return _printCommand;
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

        #endregion
    }
}
