using ETexsys.RFIDServer;
using ETexsys.RFIDServer.Model;
using ETexsys.RFIDServer.Reader;
using ETexsys.WashingCabinet.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ETexsys.WashingCabinet
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static SerialPort sp { get; set; }

        public IReader DirtReader { get; set; }

        public static Queue<string> TagQueue { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (IsAppStart())
            {
                Process current = Process.GetCurrentProcess();
                if (IsAppStart())
                {
                    CloseProcess(current);
                    if (MessageBoxResult.Yes == MessageBox.Show("系统需重启", "系统遇到问题需要重新启动，是否立即重启？", MessageBoxButton.YesNo, MessageBoxImage.Question))
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + current.ProcessName + ".exe";
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                    }

                    //关闭所有已经运行的程序
                    CloseProcess(current);
                    return;
                }
            }
            ConfigController.Init();
            if (ConfigController.ReaderConfig != null)
            {
                ReaderController.Instance.ScanUtilities = new RFIDScanUitilities();
                ReaderController.Instance.InitReader();
            }
            TagQueue = new Queue<string>();

            if (ConfigController.ReaderConfigDirt != null)
            {
                DirtReader = DeviceFactory.CreateDevice(ConfigController.ReaderConfigDirt.Type);
                DirtReader.InitReader(ConfigController.ReaderConfigDirt);
                DirtReader.Scaned += DirtReader_Scaned; ;
                if (DirtReader.Connect())
                {
                    DirtReader.StartScan();
                    ConfigController.ReaderConfigDirt.IsConnection = true;
                }
            }
            base.OnStartup(e);

            InitCOM();
        }

        private void DirtReader_Scaned(object sender, ScanedEventArgs e)
        {
            try
            {
                List<Tag_ISO_18000_6C> list = e.TagList;

                Tag_ISO_18000_6C tag = null;
                for (int i = 0; i < list.Count; i++)
                {
                    tag = list[i];

                    if (tag == null)
                        continue;

                    lock (TagQueue)
                    {
                        if (!TagQueue.Contains(tag.EPCCode))
                            TagQueue.Enqueue(tag.EPCCode);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info("处理了一个错误:" + ex.Message);
            }
        }

        private static bool CloseProcess(Process process)
        {
            try
            {
                Process[] ps = Process.GetProcessesByName(process.ProcessName);

                //最后一个是新启动的进程，不用计算
                Process last = null;
                List<Process> list = ps.ToList().OrderByDescending(c => c.StartTime).ToList();
                if (list.Count > 0)
                {
                    last = list[0];
                }

                foreach (Process p in ps)
                {
                    if (last != null && p == last)
                        continue;
                    p.Kill();
                    p.WaitForExit();
                    p.Close();

                    System.Threading.Thread.Sleep(100);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool IsAppStart()
        {
            bool result = false;

            Process curret = Process.GetCurrentProcess();
            Process[] process = Process.GetProcessesByName(curret.ProcessName);

            //最后一个是新启动的进程，不用计算
            Process last = null;
            List<Process> list = process.ToList().OrderByDescending(c => c.StartTime).ToList();
            if (list.Count > 0)
            {
                last = list[0];
            }
            foreach (var item in process)
            {
                if (last != null && last == item)
                    continue;
                if (item.Id != curret.Id)
                {
                    if (item.MainWindowHandle == null)
                    {
                        item.Kill();
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private void InitCOM()
        {
            try
            {
                sp = new SerialPort();
                sp.PortName = ConfigurationManager.AppSettings["COM"];
                sp.ReadBufferSize = 1024;
                sp.WriteBufferSize = 1024;
                sp.BaudRate = 9600;
                sp.DataBits = 8;
                sp.StopBits = System.IO.Ports.StopBits.One;
                sp.Parity = System.IO.Ports.Parity.None;
                sp.WriteTimeout = -1;
                sp.ReadTimeout = -1;
                sp.Handshake = Handshake.None;
                sp.RtsEnable = false;
                sp.DtrEnable = false;
                sp.Open();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (DirtReader != null)
            {
                DirtReader.StopScan();
                DirtReader.DisConnect();
            }

            if (ReaderController.Instance.Reader != null)
            {
                ReaderController.Instance.Reader.StopScan();
                ReaderController.Instance.Reader.DisConnect();
            }

            Application.Current.Shutdown();
            Process current = System.Diagnostics.Process.GetCurrentProcess();
            CloseProcess(current);
            if (App.sp != null && App.sp.IsOpen)
            {
                App.sp.Close();
            }
        }
    }
}
