using ETexsys.APIRequestModel.Request;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Utilities
{
    public class SyncController
    {
        private static readonly string applicationpath = Directory.GetCurrentDirectory();
        private static readonly string uuid = Computer.Instance().GetMacAddress();

        #region 单例模式初始化

        private static readonly object obj = new object();

        private static volatile SyncController instance;
        public static SyncController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (obj)
                    {
                        if (null == instance)
                        {
                            instance = new SyncController();
                        }
                    }
                }
                return instance;
            }
        }

        #endregion

        public SyncController()
        {

        }

        /// <summary>
        /// 数据同步
        /// </summary>
        /// <param name="sm"></param>
        /// <returns>true:存在数据更新 false:无数据更新</returns>
        public bool Sync(Action<long, long> action)
        {
            SyncParamModel model = null;

            string path = string.Format("{0}\\DBData\\db.sqlite", applicationpath);
            if (File.Exists(path))
            {
                SyncParamModel local = GetVersion(path);
                if (local != null)
                {
                    model = new SyncParamModel() { UUID = uuid, Version = local.Version, CreateTime = local.CreateTime };
                    bool ishave = Check(model);
                    if (ishave)
                    {
                        Download(model, action, true);
                    }
                    else
                    {
                        action(0, 0);
                        return false;
                    }
                }
            }
            else
            {
                model = new SyncParamModel() { UUID = uuid, Version = 0 };
                Download(model, action);
            }
            return true;
        }

        #region 私有方法

        private bool Check(SyncParamModel model)
        {
            var tmm = ApiController.Instance.DoPost(ApiController.Instance.CheckSyn, model);
            var mm = tmm.Result;
            if (mm.ResultCode == 0 && mm.Result.ToString() == "1")
            {
                return true;
            }
            return false;
        }

        private void Download(SyncParamModel model, Action<long, long> process, bool isWriteDB = false)
        {
            string rpath = model.Version == 0 ? ApiController.Instance.FirSyn : ApiController.Instance.SecSync;

            var tmm = ApiController.Instance.DoPost(rpath, model);
            var mm = tmm.Result;
            if (mm.ResultCode == 0 && mm.Result != null)
            {
                SyncParamModel spm = Newtonsoft.Json.JsonConvert.DeserializeObject<SyncParamModel>(mm.Result.ToString());

                string zippath = string.Format(@"{0}\DBData\db{1}.zip", applicationpath, spm.Version);

                WebRequest request = WebRequest.Create(ApiController.Instance.BaseUrl + "/" + spm.FilePath);
                WebResponse respone = request.GetResponse();

                process(respone.ContentLength, 0);

                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    Stream netStream = respone.GetResponseStream();
                    Stream fileStream = new FileStream(zippath, FileMode.OpenOrCreate);
                    byte[] read = new byte[1024];
                    long progressBarValue = 0;
                    int realReadLen = netStream.Read(read, 0, read.Length);
                    while (realReadLen > 0)
                    {
                        fileStream.Write(read, 0, realReadLen);
                        progressBarValue += realReadLen;
                        process(respone.ContentLength, progressBarValue);

                        realReadLen = netStream.Read(read, 0, read.Length);
                    }
                    netStream.Close();
                    fileStream.Close();

                    WinRarHelper.UnCompressRar(zippath, applicationpath + @"\DBData");

                    File.Delete(zippath);

                    if (isWriteDB)
                    {
                        WriteDB(spm);
                    }
                }, null);
            }
            else
            {
                log4net.ILog log = log4net.LogManager.GetLogger("abc");
                log.Error(mm.ResultMsg);
            }
        }

        private void WriteDB(SyncParamModel model)
        {
            string rpath = string.Format("{0}\\DBData\\et.sqlite", applicationpath);
            string rconnstr = string.Format("Data Source={0};Version=3;Pooling=False;BinaryGUID=False;Max Pool Size=100;", rpath);

            string wpath = string.Format("{0}\\DBData\\db.sqlite", applicationpath);
            string wconnstr = string.Format("Data Source={0};Version=3;Pooling=False;BinaryGUID=False;Max Pool Size=100;", wpath);

            DbConnection rconn = null;
            DbCommand rcmd;

            DbConnection wconn = null;
            DbCommand wcmd;
            DbTransaction trans = null;

            try
            {
                wconn = new SQLiteConnection(wconnstr);
                wconn.Open();
                trans = wconn.BeginTransaction();

                wcmd = wconn.CreateCommand();
                wcmd.Transaction = trans;

                rconn = new SQLiteConnection(rconnstr);
                rconn.Open();

                rcmd = rconn.CreateCommand();
                rcmd.CommandText = "SELECT c1 FROM t0 ";

                int m = 0;
                DbDataReader reader = rcmd.ExecuteReader();
                while (reader.Read())
                {
                    wcmd.CommandText = reader["c1"].ToString();
                    wcmd.ExecuteNonQuery();
                    m++;
                    if (m % 5000 == 0)
                    {
                        trans.Commit();
                        trans = wconn.BeginTransaction();
                    }
                }

                long ticks = ToUnixTime(model.CreateTime);
                wcmd.CommandText = string.Format("update t0 set c1={0},c2='{1}' where exists (select * from t0 where c0=1);", model.Version, ticks);
                wcmd.ExecuteNonQuery();

                trans.Commit();
            }
            catch (Exception e1)
            {
                if (trans != null)
                {
                    trans.Rollback();
                }
                throw e1;
            }
            finally
            {
                if (rconn != null)
                {
                    rconn.Close();
                }
                if (wconn != null)
                {
                    wconn.Close();
                }
            }
        }

        private SyncParamModel GetVersion(string path)
        {
            string connstr = string.Format("Data Source={0};Version=3;Pooling=False;BinaryGUID=False;Max Pool Size=100;", path);

            SyncParamModel spm = null;

            DbConnection conn = null;
            DbCommand cmd;

            try
            {
                conn = new SQLiteConnection(connstr);
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.CommandText = "select c0,c1,c2 from t0;";
                DbDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    spm = new SyncParamModel();
                    spm.Version = Convert.ToInt64(reader["c1"]);

                    DateTime time = FromUnixTime(Convert.ToInt64(reader["c2"]));
                    spm.CreateTime = time;
                }
            }
            catch (Exception e1)
            {
                throw e1;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            conn.Close();

            return spm;
        }

        #endregion

        #region 时间转换

        /// <summary>
        /// 毫秒数获取时间
        /// </summary>
        /// <param name="unixTime">毫秒数</param>
        /// <returns></returns>
        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }

        /// <summary>
        /// 获取毫秒数
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        private long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalMilliseconds);
        }

        #endregion
    }
}
