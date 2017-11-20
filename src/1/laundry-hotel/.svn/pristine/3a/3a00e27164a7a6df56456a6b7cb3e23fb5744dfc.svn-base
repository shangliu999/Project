using ETexsys.RFIDServer.Model;
using Etextsys.Terminal.Domain;
using Etextsys.Terminal.Model.Settings;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.View.Choose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Settings
{
    public class ReaderSettingViewModel : ViewModelBase
    {
        private ReaderSettingModel model;
        public ReaderSettingModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        public ReaderSettingViewModel()
        {
            Init();
        }

        #region Action

        private void Save(object op)
        {
            if (model.ConnType == (int)ConnectionType.COM)
            {
                if (string.IsNullOrEmpty(model.COM))
                {
                    EtexsysMessageBox.Show("提示", "请选择COM口", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                bool isEMRK = model.ReaderType.Equals((int)ReaderType.EMRK);

                string warn = isEMRK ? "请填写蓝牙名称." : "请填写IP地址.";

                if (string.IsNullOrEmpty(model.IPAddress))
                {
                    EtexsysMessageBox.Show("提示", warn, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (isEMRK)
                {
                    if (!model.IPAddress.StartsWith("EMRK"))
                    {
                        EtexsysMessageBox.Show("提示", "蓝牙名称输入不正确.", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (string.IsNullOrEmpty(model.HostIPAddress))
                    {
                        EtexsysMessageBox.Show("提示", "请输入蓝牙mac地址.", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    //if (!IsIP(model.IPAddress))
                    //{
                    //    EtexsysMessageBox.Show("提示", "IP地址输入格式不正确", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    return;
                    //}
                }
            }

            if (!model.Antennas.Contains(true))
            {
                EtexsysMessageBox.Show("提示", "请设置天线", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(model.AntennaPower.ToString()))
            {
                EtexsysMessageBox.Show("提示", "请设置功率", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                if (!IsNum(model.AntennaPower.ToString()))
                {
                    EtexsysMessageBox.Show("提示", "功率格式不正确", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            if (model.ReaderType == (int)ReaderType.Alien && model.ConnType == (int)ConnectionType.TCPIP)
            {
                if (string.IsNullOrEmpty(model.IPAddress) || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                {
                    EtexsysMessageBox.Show("提示", "本机IP、用户名、密码不能为空", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IsIP(model.HostIPAddress))
                {
                    EtexsysMessageBox.Show("提示", "本机IP格式不正确", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(model.Frepuency.ToString()))
            {
                double frepuency;
                if (!double.TryParse(model.Frepuency.ToString(), out frepuency))
                {
                    EtexsysMessageBox.Show("提示", "频段设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }


            ReaderModel rmodel = new ReaderModel();
            rmodel.Type = (ReaderType)model.ReaderType;
            rmodel.ConnType = (ConnectionType)model.ConnType;
            rmodel.ReaderMode = model.ReaderMode < 2 ? "Small" : "Big";
            rmodel.IPAddress = model.IPAddress;
            rmodel.COM = model.COM;

            //bool isXIPING = model.ReaderType == (int)ReaderType.XIPING || model.ReaderType == (int)ReaderType.Alien;
            List<int> ant = new List<int>();
            for (int i = 0; i < model.Antennas.Count; i++)
            {
                if (model.Antennas[i])
                {
                    //ant.Add(isXIPING ? i : (i + 1));
                    ant.Add(i);
                }
            }

            rmodel.Antenna = ant;
            rmodel.AntennaPower = Convert.ToDouble(model.AntennaPower);
            rmodel.Frepuency = Convert.ToDouble(model.Frepuency);
            rmodel.HostIPAddress = model.HostIPAddress;
            rmodel.LoginName = model.UserName;
            rmodel.LoginPwd = model.Password;

            ConfigController.SaveReaderConfig(rmodel);

            ConfigController.Init();
            if (ConfigController.ReaderConfig != null)
            {
                ReaderController.Instance.ScanUtilities = new RFIDScanUitilities();
                ReaderController.Instance.InitReader();
            }

            EtexsysMessageBox.Show("提示", "读写器设置保存成功", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void SelectionChanged(ComboBox cmb)
        {
            string val = cmb.SelectedValue.ToString();
            if (val.Equals("COM"))
            {
                model.Visibles[0] = Visibility.Hidden;
                model.Visibles[1] = Visibility.Visible;
                model.IPTitle = "COM端口：";

                model.HostIPTitle = "本机IP地址：";
            }
            else if (val.Equals("TCPIP"))
            {
                model.Visibles[0] = Visibility.Visible;
                model.Visibles[1] = Visibility.Hidden;

                model.IPTitle = "IP地址：";

                model.HostIPTitle = "本机IP地址：";
            }
            else
            {
                model.Visibles[0] = Visibility.Visible;
                model.Visibles[1] = Visibility.Hidden;
                model.IPTitle = "蓝牙名称：";

                model.HostIPTitle = "蓝牙Mac地址：";
            }
        }

        #endregion

        #region commond


        private RelayCommand<object> saveCommond;
        public ICommand SaveCommond
        {
            get
            {
                if (saveCommond == null)
                {
                    saveCommond = new RelayCommand<object>(Save);
                }
                return saveCommond;
            }
        }

        private RelayCommand<ComboBox> selectionChangedCmd;
        public RelayCommand<ComboBox> SelectionChangedCmd
        {
            get
            {
                if (selectionChangedCmd == null)
                {
                    selectionChangedCmd = new RelayCommand<ComboBox>(SelectionChanged);
                }
                return selectionChangedCmd;
            }
        }


        #endregion

        #region 私有方法

        private void Init()
        {
            ReaderModel rmodel = ConfigController.GetReaderConfig();

            if (rmodel != null)
            {
                model = new ReaderSettingModel()
                {
                    ReaderType = (int)rmodel.Type,
                    ConnType = (int)rmodel.ConnType,
                    IPAddress = rmodel.IPAddress,
                    AntennaPower = rmodel.AntennaPower,
                    Frepuency = (int)rmodel.Frepuency,
                    UserName = rmodel.LoginName,
                    Password = rmodel.LoginPwd,
                    HostIPAddress = rmodel.HostIPAddress,
                    COM = rmodel.COM,
                    ReaderMode = (rmodel.ReaderMode == "Small" ? 0 : 2),
                };

                if (rmodel.ConnType.Equals(ConnectionType.COM))
                {
                    model.Visibles[0] = Visibility.Hidden;
                    model.Visibles[1] = Visibility.Visible;
                    model.IPTitle = "COM端口：";

                    model.HostIPTitle = "本地IP地址：";
                }
                else if (rmodel.ConnType.Equals(ConnectionType.TCPIP))
                {
                    model.Visibles[0] = Visibility.Visible;
                    model.Visibles[1] = Visibility.Hidden;
                    model.IPTitle = "IP地址：";

                    model.HostIPTitle = "本地IP地址：";
                }
                else
                {
                    model.Visibles[0] = Visibility.Visible;
                    model.Visibles[1] = Visibility.Hidden;
                    model.IPTitle = "蓝牙名称：";

                    model.HostIPTitle = "蓝牙mac地址：";
                }

                for (int i = 0; i < rmodel.Antenna.Count; i++)
                {
                    model.Antennas[i] = true;
                }
            }
            else
            {
                model = new ReaderSettingModel();

                model.Visibles[0] = Visibility.Hidden;
                model.Visibles[1] = Visibility.Visible;
                model.IPTitle = "COM端口：";
            }
        }

        private bool IsNum(string num)
        {
            bool flag = true;
            try
            {
                if (double.Parse(num) < 0)
                    flag = false;
            }
            catch
            {
                flag = false;
            }

            return flag;
        }

        private bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        #endregion
    }
}
