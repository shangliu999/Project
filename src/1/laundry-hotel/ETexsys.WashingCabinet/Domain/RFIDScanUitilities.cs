using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ETexsys.RFIDServer.Model;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using ETexsys.RFIDServer.Reader;

namespace ETexsys.WashingCabinet.Domain
{
    public class RFIDScanUitilities: Window
    {
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

        public RFIDScanUitilities()
        {
            readerModel = ConfigController.ReaderConfig;

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
                if (ts.TotalSeconds > 3)
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

                        //bool result = Regex.IsMatch(tag.EPCCode, @"300ED\w{19}");
                        bool result = Regex.IsMatch(tag.EPCCode, @"(00000|300ED)\w{19}");
                        if (result)
                        {
                            //纺织品
                            _newList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Textile });
                            _uidList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Textile });
                        }
                        else
                        {
                            result = Regex.IsMatch(tag.EPCCode, @"E2000\w{19}");
                            if (result)
                            {
                                //包
                                _newList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Bag });
                                _uidList.Add(new TagModel { TagNo = tag.EPCCode, Type = TagType.Bag });
                            }
                            else
                            {
                                result = Regex.IsMatch(tag.EPCCode, @"30083\w{19}");
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

        public void StartScan(IRFIDScan action, Action callback = null)
        {
            if (readerModel != null)
            {
                if (!_isStrat || readerModel.ReaderMode == "Big")
                {
                    if (readerModel.IsConnection)
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
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
                                    //EtexsysMessageBox.Show("提示", "阅读器打开失败。", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }));
                            }
                        });
                    }
                    else
                    {
                        //EtexsysMessageBox.Show("提示", "请确认阅读器是否开启，或者重新启动系统。", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    if (_isStrat)
                    {
                        _timer.Start();
                    }
                }
            }
            else
            {
                //EtexsysMessageBox.Show("提示", "请先配置阅读器。", MessageBoxButton.OK, MessageBoxImage.Warning);
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
