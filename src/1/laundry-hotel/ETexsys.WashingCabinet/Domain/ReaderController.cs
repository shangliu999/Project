﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETexsys.RFIDServer.Model;
using ETexsys.RFIDServer.Reader;
using ETexsys.RFIDServer;
using System.Reflection;

namespace ETexsys.WashingCabinet.Domain
{
    public class ReaderController
    {
        private ReaderModel readerModel;
        public IReader Reader { get; set; }
        public Queue<Tag_ISO_18000_6C> TagQueue { get; set; }
        public DateTime LastReiceveTagTime { get; set; }

        private static ReaderController instance;
        public static ReaderController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ReaderController();
                }
                return instance;
            }
        }
        public RFIDScanUitilities ScanUtilities { get; set; }

        public void InitReader()
        {
            readerModel = ConfigController.ReaderConfig;

            TagQueue = new Queue<Tag_ISO_18000_6C>();
            LastReiceveTagTime = DateTime.Now;

            if (readerModel != null)
            {
                Reader = DeviceFactory.CreateDevice(readerModel.Type);
                Reader.InitReader(readerModel);
                Reader.Scaned += Reader_Scaned;
                Reader.Trigger += Reader_Trigger;

                //System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                //{
                    readerModel.IsConnection = Reader.Connect();
                //});
            }
        }
        public void Reader_Trigger(object sender, TriggerEventArgs e)
        {
            TriggerInfo model = e.TInfo;

            //lock (TriggerQueue)
            //{
            //    TriggerQueue.Enqueue(model);
            //}
        }

        public void Reader_Scaned(object sender, ScanedEventArgs e)
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
                        if (!TagQueue.Contains(tag))
                            TagQueue.Enqueue(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info("处理了一个错误:" + ex.Message);
            }
        }
    }
}
