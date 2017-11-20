﻿using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static ResponseSysUserModel CurrentLoginUser { get; set; }

        /// <summary>
        /// 主界面背景资源
        /// </summary>
        public static string HomePageDictionary { get; set; }

        /// <summary>
        /// 内页背景资源
        /// </summary>
        public static string InPageDictionary { get; set; }

        /// <summary>
        /// 天线转动串口
        /// </summary>
        public static SerialPort TurnSP { get; set; }

        public static bool ReaderInited { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            //判断当前程序是否已经运行
            if (IsAppStart())
            {
                Process current = Process.GetCurrentProcess();
                if (IsAppStart())
                {
                    CloseProcess(current);
                    if (true == EtexsysMessageBox.Show("系统需重启", "系统遇到问题需要重新启动，是否立即重启？", MessageBoxButton.YesNo, MessageBoxImage.Question))
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

            log4net.ILog LOG = log4net.LogManager.GetLogger("OnStartup");
            LOG.Info("终端正在启动中...");

            ConfigController.Init();

            if (ConfigController.SystemSettingConfig != null)
            {
                if (ConfigController.SystemSettingConfig.OpenMoveAntenna && ConfigController.SystemSettingConfig.MoveAntennaCom != null
                    && ConfigController.SystemSettingConfig.MoveAntennaCom != "")
                {
                    ComUitilities com = new ComUitilities();
                    TurnSP = com.Init(ConfigController.SystemSettingConfig.MoveAntennaCom);
                }
            }
            App.Current.Resources["MainWinBg"] = App.Current.Resources[App.HomePageDictionary];
            App.Current.Resources["FuncWinBg"] = App.Current.Resources[App.InPageDictionary];

            ReaderInited = false;

            if (ConfigController.ReaderConfig != null)
            {
                ReaderController.Instance.ScanUtilities = new RFIDScanUitilities();
                ReaderController.Instance.InitReader();
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                //阻止程序继续处理异常
                e.Handled = true;

                //写入日志的文字
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendFormat("应用程序出现了未捕获的异常，Message：{0};\n StackTrace:{1}。\n",
                    e.Exception.Message, e.Exception.StackTrace);

                if (e.Exception.InnerException != null)
                {
                    stringBuilder.AppendFormat("\n Message：{0};\n StackTrace:{1}。",
                        e.Exception.InnerException.Message, e.Exception.StackTrace);
                }

                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info(stringBuilder.ToString());

                EtexsysMessageBox.Show("提示", "应用程序出现了未捕获的异常", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch
            {
                if (ConfigController.ReaderConfig != null)
                {
                    if (ReaderController.Instance.Reader != null && ConfigController.ReaderConfig.IsConnection && ReaderController.Instance.ScanUtilities != null)
                    {
                        ReaderController.Instance.ScanUtilities.StopScan();
                        ReaderController.Instance.Reader.DisConnect();
                    }
                }

                Process current = Process.GetCurrentProcess();
                CloseProcess(current);
                if (EtexsysMessageBox.Show("系统需重启", "系统遇到问题需要重新启动，是否立即重启？", MessageBoxButton.YesNo, MessageBoxImage.Question) == true)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + current.ProcessName + ".exe";
                    p.StartInfo.UseShellExecute = false;
                    p.Start();

                    CloseProcess(current);
                }
                else
                {
                    //关闭所有已经运行的程序
                    CloseProcess(current);
                }
                return;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                //写入日志的文字
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendFormat("应用程序出现了未捕获的异常，Message：{0};\n StackTrace:{1}。\n",
                    ex.Message, ex.StackTrace);

                if (ex.InnerException != null)
                {
                    stringBuilder.AppendFormat("\n Message：{0};\n StackTrace:{1}。",
                        ex.InnerException.Message, ex.StackTrace);
                }

                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info(stringBuilder.ToString());

                if (e.IsTerminating)
                {
                    EtexsysMessageBox.Show("错误", "应用程序出现了未捕获的异常,程序即将终止", MessageBoxButton.OK, MessageBoxImage.Warning);

                    Environment.Exit(0);
                }
            }
            catch
            {
                if (ConfigController.ReaderConfig != null)
                {
                    if (ReaderController.Instance.Reader != null && ConfigController.ReaderConfig.IsConnection && ReaderController.Instance.ScanUtilities != null)
                    {
                        ReaderController.Instance.ScanUtilities.StopScan();
                        ReaderController.Instance.Reader.DisConnect();
                    }
                }

                Process current = System.Diagnostics.Process.GetCurrentProcess();
                CloseProcess(current);
                if (EtexsysMessageBox.Show("系统需重启", "系统遇到问题需要重新启动，是否立即重启？", MessageBoxButton.YesNo, MessageBoxImage.Question) == true)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + current.ProcessName + ".exe";
                    p.StartInfo.UseShellExecute = false;
                    p.Start();

                    CloseProcess(current);
                }
                else
                {
                    //关闭所有已经运行的程序
                    CloseProcess(current);
                }
                return;
            }
        }

        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <returns>关闭成功(true)或失败(false)</returns>
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

        /// <summary>
        /// 应用程序是否启用
        /// </summary>
        /// <returns></returns>
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
    }
}
