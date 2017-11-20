﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.QuerySummary;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using ETextsys.Terminal.View.QuerySummary;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.QuerySummary
{
    public class TextileQueryViewModel : ViewModelBase, IRFIDScan
    {
        private TextileQueryModel model;
        public TextileQueryModel Model
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

        private bool _isSubmit;

        private List<string> ScanList;

        private List<ResponseRFIDTagModel> TextileList;

        private DateTime ResetTime;

        public TextileQueryViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new TextileQueryModel();
            this.model.TextileSingleTable = new ObservableCollection<TextileSingleTableModel>();
            this.model.TextileAreaTable = new ObservableCollection<TextileAreaTableModel>();
            this.model.ExportEnabled = false;
            this.model.SingleBtnChoosed = true;
            this.model.AreaBtnChoosed = false;
            this.model.TextileSingleVisibiled = Visibility.Visible;
            this.model.TextileAreaVisibiled = Visibility.Collapsed;

            this.model.WaitVisibled = Visibility.Hidden;

            ScanList = new List<string>();
            TextileList = new List<ResponseRFIDTagModel>();
            _isSubmit = false;
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
            if (model.TextileSingleTable.Count > 0 && !_isSubmit)
            {
                this.model.ExportEnabled = true;
            }
        }

        public async void ScanNew(List<TagModel> rfidTags)
        {
            var tags = rfidTags.Where(v => v.Type == TagType.Textile).Select(v => v.TagNo).ToList();
            if (tags.Count == 0)
            {
                return;
            }
            this.model.ExportEnabled = false;

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

                TextileSingleTableModel receiveModel = null;
                var query = from t in TextileList
                            select new
                            {
                                id = t.ID,
                                brandName = t.BrandName,
                                className = t.ClassName,
                                sizeName = t.SizeName,
                                hotelName = t.HotelName,
                                washtimes = t.Washtimes,
                                RFIDTagNo = t.TagNo,
                                costTime = t.CostTime,
                                RFIDWashtime = t.RFIDWashtime
                            };

                int total = 0;
                this.model.TextileSingleTable.Clear();
                query.ToList().ForEach(q =>
                {
                    receiveModel = new TextileSingleTableModel();
                    receiveModel.TextileId = q.id;
                    receiveModel.BrandName = q.brandName;
                    receiveModel.ClassName = q.className;
                    receiveModel.SizeName = q.sizeName;
                    receiveModel.TextileWashtime = q.washtimes;
                    receiveModel.RFIDTagNo = q.RFIDTagNo;
                    receiveModel.CostTime = q.costTime.ToString("yyyy/MM/dd");
                    receiveModel.HotelName = q.hotelName;
                    receiveModel.RFIDWashtime = q.RFIDWashtime;
                    total += 1;
                    this.model.TextileSingleTable.Add(receiveModel);
                });

                var areaQuery = from t in TextileList
                                group t by new
                                {
                                    t1 = t.ClassSort,
                                    t2 = t.ClassName,
                                    t3 = t.SizeSort,
                                    t4 = t.SizeName
                                } into m
                                orderby m.Key.t1, m.Key.t2, m.Key.t3, m.Key.t4
                                select new
                                {
                                    className = m.Key.t2,
                                    sizeName = m.Key.t4,
                                    count = m.Count()
                                };

                TextileAreaTableModel areaModel = null;
                this.model.TextileAreaTable.Clear();
                areaQuery.ToList().ForEach(q =>
                {
                    areaModel = new TextileAreaTableModel();
                    areaModel.ClassName = q.className;
                    areaModel.SizeName = q.sizeName;
                    areaModel.TextileCount = q.count;
                    this.model.TextileAreaTable.Add(areaModel);
                });

                this.model.UnRegisterTotal = ScanList.Count - total;
                this.model.TextileCount = total;
            }
        }

        #endregion

        #region Action

        private void SelectDetailAction(object obj)
        {
            if (obj != null && obj is TextileSingleTableModel)
            {
                TextileDetail win = new TextileDetail();
                win.DataContext = new TextileDetailViewModel(() =>
                {
                    win.Close();
                }, obj as TextileSingleTableModel);
                win.ShowDialog();
            }
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

        private void CloseModalAction(string sender)
        {
            if (ConfigController.ReaderConfig != null && ReaderController.Instance.ScanUtilities != null)
            {
                ReaderController.Instance.ScanUtilities.StopScan();
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
            this.model.ExportEnabled = false;
            this.model.UnRegisterTotal = 0;
            this.model.TextileCount = 0;
            this.model.TextileSingleTable.Clear();
            this.model.TextileAreaTable.Clear();
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
                _isSubmit = false; 
            }
        }

        private void SingleAction(object sender)
        {
            this.model.SingleBtnChoosed = true;
            this.model.AreaBtnChoosed = false;
            this.model.TextileSingleVisibiled = Visibility.Visible;
            this.model.TextileAreaVisibiled = Visibility.Collapsed;
        }

        private void AreaAction(object sender)
        {
            this.model.SingleBtnChoosed = false;
            this.model.AreaBtnChoosed = true;
            this.model.TextileSingleVisibiled = Visibility.Collapsed;
            this.model.TextileAreaVisibiled = Visibility.Visible;
        }

        private void ExportAction(object sender)
        {
            DataTable dt = CreateExportTable();
            DataRow row = null;

            if (this.model.TextileSingleTable.Count == 0)
                return;

            foreach (var item in this.model.TextileSingleTable)
            {
                row = dt.NewRow();
                row["芯片码"] = item.RFIDTagNo;
                row["流通"] = item.BrandName;
                row["纺织品品名"] = item.ClassName;
                row["尺寸"] = item.SizeName;
                row["投入时间"] = item.CostTime;
                row["洗涤次数"] = item.TextileWashtime;
                row["最后使用酒店"] = item.HotelName;
                dt.Rows.Add(row);
            }


            string desktopUrl = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = desktopUrl;
            //默然文件后缀
            sfd.DefaultExt = "xls";
            //文件后缀列表
            sfd.Filter = "EXCEL文件(*.XLS)|*.xls";

            if (sfd.ShowDialog() == true)
            {
                ExcelRender.RenderToExcel(dt, sfd.FileName.ToString());

                EtexsysMessageBox.Show("提示", "导出成功.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private DataTable CreateExportTable()
        {
            DataTable dt = new DataTable();

            DataColumn col = new DataColumn();
            col.ColumnName = "芯片码";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "流通";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "纺织品品名";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "尺寸";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "投入时间";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "洗涤次数";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "最后使用酒店";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            return dt;
        }

        #endregion

        #region Command

        private RelayCommand<object> _SelectDetailCommand;
        public RelayCommand<object> SelectDetailCommand
        {
            get
            {
                if (_SelectDetailCommand == null)
                {
                    _SelectDetailCommand = new RelayCommand<object>(SelectDetailAction);
                }
                return _SelectDetailCommand;
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

        private RelayCommand<object> _ExportCommand;
        public ICommand ExportCommand
        {
            get
            {
                if (_ExportCommand == null)
                {
                    _ExportCommand = new RelayCommand<object>(ExportAction);
                }
                return _ExportCommand;
            }
        }

        private RelayCommand<object> _AreaCommand;
        public ICommand AreaCommand
        {
            get
            {
                if (_AreaCommand == null)
                {
                    _AreaCommand = new RelayCommand<object>(AreaAction);
                }
                return _AreaCommand;
            }
        }

        private RelayCommand<object> _SingleCommand;
        public ICommand SingleCommand
        {
            get
            {
                if (_SingleCommand == null)
                {
                    _SingleCommand = new RelayCommand<object>(SingleAction);
                }
                return _SingleCommand;
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
