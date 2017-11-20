﻿using ETexsys.RFIDServer.Model;
using ETexsys.RFIDServer.Reader;
using Etextsys.Terminal.Domain;
using Etextsys.Terminal.Model.Settings;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Model.Settings;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ETextsys.Terminal.Domain
{
    public class RFIDScanUitilities : Window
    {
        private object obj = new object();

        public int OpenSubmitTime { get; set; }

        private bool _isStrat = false;
        public bool IsStrated
        {
            get { return _isStrat; }
            set { _isStrat = value; }
        }

        private readonly DispatcherTimer _timer;

        private ReaderModel readerModel;

        private List<TagModel> _newList;
        public List<TagModel> NewList
        {
            get { return _newList; }
            set { _newList = value; }
        }

        private List<TagModel> _uidList;
        public List<TagModel> UidList
        {
            get { return _uidList; }
            set { _uidList = value; }
        }

        private IRFIDScan _scanAction;

        private DateTime PreAntTurnTime;
        private ComUitilities com;

        public RFIDScanUitilities()
        {
            readerModel = ConfigController.ReaderConfig;
            OpenSubmitTime = 3;

            com = new ComUitilities();
            PreAntTurnTime = DateTime.MinValue;

            _newList = new List<TagModel>();
            _uidList = new List<TagModel>();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            _timer.Tick += _timer_Tick;
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            if (App.TurnSP != null)
            {
                DateTime dt = PreAntTurnTime.AddSeconds(20);
                if (PreAntTurnTime == DateTime.MinValue || DateTime.Now > dt)
                {
                    PreAntTurnTime = DateTime.Now;

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        com.TurnAntenna(App.TurnSP);
                    });
                }
            }

            if (AddTagList())
            {
                if (_scanAction != null)
                {
                    if (_newList.Count > 0)
                    {
                        _scanAction.ScanNew(_newList);
                        _newList.Clear();
                    }
                }
            }
            else
            {
                TimeSpan ts = DateTime.Now - ReaderController.Instance.LastReiceveTagTime;
                if (ts.TotalSeconds > OpenSubmitTime)
                {
                    if (_scanAction != null)
                    {
                        _scanAction.NoScanTag();
                    }
                }
            }

            _timer.Start();
        }

        bool AddTagList()
        {
            bool isAdded = false;

            while (ReaderController.Instance.TagQueue.Count > 0)
            {
                Tag_ISO_18000_6C tag = ReaderController.Instance.TagQueue.Dequeue();

                if (tag == null)
                {
                    continue;
                }

                if (_uidList.Where(v => v.TagNo == tag.EPCCode).Count() == 0)
                {
                    lock (_newList)
                    {
                        //bool result = Regex.IsMatch(tag.EPCCode, @"300E\w{20}");
                        bool result = Regex.IsMatch(tag.EPCCode, @"(0000|300E)\w{20}");
                        if (result)
                        {
                            //纺织品
                            _newList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Textile });
                            _uidList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Textile });
                        }
                        else
                        {
                            result = Regex.IsMatch(tag.EPCCode, @"E200\w{20}");
                            if (result)
                            {
                                //包
                                _newList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Bag });
                                _uidList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Bag });
                            }
                            else
                            {
                                result = Regex.IsMatch(tag.EPCCode, @"3008\w{20}");
                                if (result)
                                {
                                    //架子车
                                    _newList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Truck });
                                    _uidList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Truck });
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        ReaderController.Instance.LastReiceveTagTime = DateTime.Now;
                        isAdded = true;
                    }
                }
            }

            return isAdded;
        }

        public void StartScan(IRFIDScan action, Action callback = null, Action errorcallback = null)
        {
            if (readerModel != null)
            {
                if (!_isStrat || readerModel.ReaderMode == "Big")
                {
                    if (readerModel.IsConnection)
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            lock (obj)
                            {
                                _isStrat = ReaderController.Instance.Reader.StartScan();
                                if (_isStrat)
                                {
                                    _scanAction = action;
                                    _timer.Start();

                                    if (callback != null)
                                    {
                                        callback();
                                    }
                                }
                                else
                                {
                                    Dispatcher.Invoke(new Action(() =>
                                    {
                                        if (errorcallback != null)
                                        {
                                            errorcallback();
                                        }
                                        EtexsysMessageBox.Show("提示", "阅读器打开失败。", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }));
                                }
                            }
                        });
                    }
                    else
                    {
                        if (errorcallback != null)
                        {
                            errorcallback();
                        }
                        EtexsysMessageBox.Show("提示", "请确认阅读器是否开启，或者重新启动系统。", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    if (_isStrat)
                    {
                        _timer.Start();

                        if (callback != null)
                        {
                            callback();
                        }
                    }
                }
            }
            else
            {
                if (errorcallback != null)
                {
                    errorcallback();
                }
                EtexsysMessageBox.Show("提示", "请先配置阅读器。", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void StopScan()
        {
            if (readerModel != null)
            {
                if (_isStrat)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        _isStrat = false;
                        ReaderController.Instance.Reader.StopScan();
                        _timer.Stop();
                    });
                }
            }
        }

        public void ClearScanTags()
        {
            if (readerModel.Type.Equals(ReaderType.EMRK))
            {
                ReaderController.Instance.Reader.StopScan();
                ((EMRK)ReaderController.Instance.Reader).EraseTagList();
                ReaderController.Instance.Reader.StartScan();
            }

            ReaderController.Instance.TagQueue.Clear();
            _newList.Clear();
            _uidList.Clear();
        }
    }
}
