using ETexsys.RFIDServer.Model;
using Etextsys.Terminal.Domain;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Etextsys.Terminal.Model.Settings
{
    public class ReaderSettingModel : ViewModelBase
    {
        private int readerType;
        public int ReaderType
        {
            get { return readerType; }
            set
            {
                readerType = value;
                this.RaisePropertyChanged("ReaderType");
            }
        }

        private int connType;
        public int ConnType
        {
            get { return connType; }
            set
            {
                connType = value;
                this.RaisePropertyChanged("ConnType");
            }
        }

        private string iPAddress;
        public string IPAddress
        {
            get { return iPAddress; }
            set
            {
                iPAddress = value;
                this.RaisePropertyChanged("IPAddress");
            }
        }

        private string cOM;
        public string COM
        {
            get { return cOM; }
            set
            {
                cOM = value;
                this.RaisePropertyChanged("cOM");
            }
        }

        private ObservableCollection<bool> antennas;
        public ObservableCollection<bool> Antennas
        {
            get { return antennas; }
            set
            {
                antennas = value;
                this.RaisePropertyChanged("Antennas");
            }
        }

        private double antennaPower;
        public double AntennaPower
        {
            get { return antennaPower; }
            set
            {
                antennaPower = value;
                this.RaisePropertyChanged("AntennaPower");
            }
        }

        private int frepuency;
        public int Frepuency
        {
            get { return frepuency; }
            set
            {
                frepuency = value;
                this.RaisePropertyChanged("Frepuency");
            }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                this.RaisePropertyChanged("UserName");
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                this.RaisePropertyChanged("Password");
            }
        }

        private string hostIPAddress;
        public string HostIPAddress
        {
            get { return hostIPAddress; }
            set
            {
                hostIPAddress = value;
                this.RaisePropertyChanged("HostIPAddress");
            }
        }

        private int readerMode;
        public int ReaderMode
        {
            get { return readerMode; }
            set
            {
                readerMode = value;
                this.RaisePropertyChanged("ReaderMode");
            }
        }

        #region 控件隐藏显示

        private ObservableCollection<Visibility> visibles;
        public ObservableCollection<Visibility> Visibles
        {
            get { return visibles; }
            set
            {
                visibles = value;
                this.RaisePropertyChanged("Visibles");
            }
        }

        private string iPTitle;
        public string IPTitle
        {
            get { return iPTitle; }
            set
            {
                iPTitle = value;
                this.RaisePropertyChanged("IPTitle");
            }
        }

        private string hostIPTitle;
        public string HostIPTitle
        {
            get { return hostIPTitle; }
            set
            {
                hostIPTitle = value;
                this.RaisePropertyChanged("HostIPTitle");
            }
        }

        #endregion

        #region Combox 数据源

        private ObservableCollection<string> readerTypeData;
        public ObservableCollection<string> ReaderTypeData
        {
            get { return readerTypeData; }
            set
            {
                readerTypeData = value;
                this.RaisePropertyChanged("ReaderTypeData");
            }
        }

        private ObservableCollection<string> connTypeData;
        public ObservableCollection<string> ConnTypeData
        {
            get { return connTypeData; }
            set
            {
                connTypeData = value;
                this.RaisePropertyChanged("ConnTypeData");
            }
        }
        
        private ObservableCollection<string> coms;
        public ObservableCollection<string> Coms
        {
            get { return coms; }
            set
            {
                coms = value;
                this.RaisePropertyChanged("Coms");
            }
        }

        private ObservableCollection<ScanMode> scanModes;
        public ObservableCollection<ScanMode> ScanModes
        {
            get { return scanModes; }
            set
            {
                scanModes = value;
                this.RaisePropertyChanged("ScanModes");
            }
        }

        #endregion

        public ReaderSettingModel()
        {
            //获取Enum数据
            readerTypeData = new ObservableCollection<string>();
            connTypeData = new ObservableCollection<string>();

            foreach (var item in Enum.GetValues(typeof(ReaderType)))
            {
                readerTypeData.Add(item.ToString());
            }
            foreach (var item in Enum.GetValues(typeof(ConnectionType)))
            {
                connTypeData.Add(item.ToString());
            }

            visibles = new ObservableCollection<Visibility>() { Visibility.Visible, Visibility.Hidden };

            antennas = new ObservableCollection<bool>() { true, false, false, false };
            
            SetComs();

            scanModes = new ObservableCollection<ScanMode>();
            scanModes.Add(new ScanMode() { Name= "平板一体机", Val="Small" });
            scanModes.Add(new ScanMode() { Name = "隧道清点一体机", Val = "Small" });
            scanModes.Add(new ScanMode() { Name = "封闭式扫描通道", Val = "Big" });
        }

        #region 私有方法

        private void SetComs()
        {
            coms = new ObservableCollection<string>();

            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey(@"Hardware\DeviceMap\SerialComm");
            if (keyCom != null)
            {
                string[] sSubKeys = keyCom.GetValueNames();
                foreach (string sName in sSubKeys)
                {
                    string sValue = (string)keyCom.GetValue(sName);
                    coms.Add(sValue);
                }
            }
        }

        #endregion
    }

    public class ScanMode : ViewModelBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                this.RaisePropertyChanged("Name");
            }
        }

        private string val;
        public string Val
        {
            get { return val; }
            set
            {
                val = value;
                this.RaisePropertyChanged("Val");
            }
        }
    }
}
