﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using CM = System.Configuration.ConfigurationManager;
using System.IO;
using ETexsys.APIRequestModel.Request;
using System.Transactions;
using System.ComponentModel;
using ETexsys.Common.Log;

namespace ETexsys.Common.Sync
{
    public class SqliteHelper
    {
        private static readonly string mysqlconnstr = CM.ConnectionStrings["Laundry_hotelEntities"].ConnectionString;

        public bool Check(SyncParamModel model)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT MAX(ut) AS ut FROM (");
            sb.Append("SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM Region ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM Bag ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM Size ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM Scrap ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM BrandType ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM sys_user_dataview ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(RegisterTime)>MAX(LogoutTime) THEN MAX(RegisterTime) ELSE MAX(LogoutTime) END AS ut FROM Textile ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM Category ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM TextileClass ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM Fabric ");
            sb.Append("UNION ALL SELECT CASE WHEN MAX(UpdateTime)>MAX(CreateTime) THEN MAX(UpdateTime) ELSE MAX(CreateTime) END AS ut FROM Color ");
            sb.Append("UNION ALL SELECT MAX(UpdateTime) AS ut FROM ClassSize UNION ALL SELECT MAX(CostTime) AS ut FROM rfidtag ");
            sb.Append("UNION ALL SELECT MAX(CreateTime) AS ut FROM rfidreplace) as v; ");

            MySqlConnection conn = null;
            MySqlCommand comm = null;

            DateTime updatetime, time;
            updatetime = time = DateTime.MinValue;
            try
            {
                string connstr = mysqlconnstr.Substring(mysqlconnstr.IndexOf("\"") + 1);
                connstr = connstr.Substring(0, connstr.Length - 1);

                conn = new MySqlConnection(connstr);
                conn.Open();

                comm = conn.CreateCommand();
                comm.CommandText = sb.ToString();
                comm.CommandTimeout = 0;

                object obj = comm.ExecuteScalar();
                updatetime = Convert.ToDateTime(obj);

                comm.CommandText = string.Format("SELECT CreateTime FROM DataVersion WHERE Version={0}", model.Version);
                MySqlDataReader reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    time = Convert.ToDateTime(reader["CreateTime"]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return updatetime > time;
        }

        /// <summary>
        /// 第一次数据同步，下载数据库文件 只能根据本地的版本号来运算
        /// </summary>
        /// <param name="path"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public SyncParamModel FirSync(string path, ref string zipfile, SyncParamModel model)
        {
            string floder, dbname;
            floder = "Data";
            dbname = "db.sqlite";
            string dbpath = string.Format("{0}DBData\\{1}\\{2}", path, floder, dbname);

            //数据文件是否存在，存在就需要先删除再插入；不存在则查询所有信息
            //将不同客户端的同步文件放在一起，节省写入时间
            bool isExist = CreateDBFile(path, floder, dbname);

            SyncParamModel newmodel = new SyncParamModel();

            DataSet ds = null;
            DateTime time = DateTime.Now;
            if (isExist)
            {
                newmodel = GetVersion(dbpath);

                bool isHave = Check(newmodel);
                if (isHave)
                {
                    ds = GetData(newmodel);
                }
                else
                {
                    newmodel.UUID = model.UUID;

                    string zipname1 = string.Format("db{0}.zip", newmodel.Version);
                    zipfile = string.Format("{0}/{1}", floder, zipname1);

                    //如果已是最新版本数据时，判断压缩文件是否存在
                    bool rst = false;
                    if (!File.Exists(string.Format("{0}DBData\\{1}", path, zipfile)))
                    {
                        rst = CreateZipfile(path, floder, dbname, ref zipname1, newmodel);
                        if (!rst)
                        {
                            zipfile = "";

                            Log4NetFile log = new Log4NetFile();
                            log.Log("zip:" + zipname1);
                        }
                    }

                    return newmodel;
                }
            }
            else
            {
                ds = GetData();
            }
            newmodel.UUID = model.UUID;
            newmodel.CreateTime = time;

            using (var scope = new TransactionScope())
            {
                UpdateVersion(newmodel);

                WriteDB(dbpath, ds, newmodel);

                scope.Complete();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            string zipname = string.Empty;
            bool result = CreateZipfile(path, floder, dbname, ref zipname, newmodel);

            if (result)
            {
                zipfile = string.Format("{0}/{1}", floder, zipname);
            }
            else
            {
                zipfile = string.Empty;
            }

            return newmodel;
        }

        /// <summary>
        /// 中间过程同步，生成txt文档
        /// </summary>
        /// <param name="path"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public SyncParamModel SecSync(string path, ref string zipfile, SyncParamModel model)
        {
            string floder, dbname;
            floder = model.UUID;
            dbname = "et.sqlite";
            string dbpath = string.Format("{0}DBData\\{1}\\{2}", path, floder, dbname);

            //各个客户端差异备份的文件,必须覆盖上一次的文件
            CreateDBFile(path, floder, dbname, true);

            DateTime time = DateTime.Now;
            DataSet ds = GetData(model);

            //修改版本更新的时间
            model.CreateTime = time;

            using (var scope = new TransactionScope())
            {
                UpdateVersion(model);

                WriteDB(dbpath, ds, model, true);

                scope.Complete();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            string zipname = string.Empty;
            bool result = CreateZipfile(path, floder, dbname, ref zipname, model);

            if (result)
            {
                zipfile = string.Format("{0}/{1}", floder, zipname);
            }
            else
            {
                zipfile = string.Empty;
            }

            SyncParamModel rst = new SyncParamModel() { Version = model.Version, CreateTime = model.CreateTime };

            return rst;
        }

        /// <summary>
        /// 生成压缩文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="floder">文件名</param>
        /// <param name="dbname">压缩的数据库名</param>
        /// <param name="zipname"></param>
        /// <param name="model">同步参数</param>
        /// <returns></returns>
        private bool CreateZipfile(string path, string floder, string dbname, ref string zipname, SyncParamModel model)
        {
            bool result = false;
            zipname = dbname.Equals("et.sqlite") ? "db.zip" : string.Format("db{0}.zip", model.Version);

            string filetozip = string.Format("{0}DBData\\{1}\\{2}", path, floder, dbname);

            List<string> list = new List<string>() { filetozip };

            string zipdfilepath = string.Format("{0}DBData\\{1}", path, floder);

            Log4NetFile log = new Log4NetFile();

            if (WinRarHelper.ExistSetupWinRar)
            {
                try
                {
                    result = WinRarHelper.CompressFilesToRar(list, zipdfilepath, zipname);
                }
                catch (Win32Exception e1)
                {
                    //throw e1;
                    log.Log(e1);
                    return false;
                }
                catch (Exception e1)
                {
                    //throw e1;
                    log.Log(e1);
                    return false;
                }
            }

            return result;
        }

        /// <summary>
        /// 写入数据库
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ds"></param>
        /// <param name="model"></param>
        /// <param name="isDiff">是否写入差异sql</param>
        /// <returns></returns>
        private bool WriteDB(string path, DataSet ds, SyncParamModel model, bool isDiff = false)
        {
            DbConnection conn = null;
            DbTransaction trans = null;
            DbCommand comm = null;

            try
            {
                string connstr = string.Format("Data Source={0};Version=3;Pooling=False;BinaryGUID=False;Max Pool Size=100;", path);

                conn = new SQLiteConnection(connstr);
                conn.Open();

                trans = conn.BeginTransaction();
                comm = conn.CreateCommand();
                comm.CommandTimeout = 0;
                comm.Transaction = trans;

                DataTable dt = ds.Tables[ds.Tables.Count - 1];
                string code = dt.Rows[0][0].ToString();

                if (isDiff)
                {
                    CreateCommondSQL(ds, conn, trans, comm, 11);

                    trans = conn.BeginTransaction();

                    string sql = "update t0 set c3='" + code + "' where c0=1;";
                    comm.CommandText = "insert into t0 (c1) values (@p1);";
                    DbParameter param = comm.CreateParameter();
                    param.ParameterName = "@p1";
                    param.DbType = DbType.String;
                    param.Value = sql;
                    comm.Parameters.Add(param);
                    comm.ExecuteNonQuery();

                    trans.Commit();
                }
                else
                {
                    //根据表格的数量判断是否为第一次创建数据库（首次创建数据库文件）
                    if (ds != null && ds.Tables != null && ds.Tables.Count <= 13)
                    {
                        CreateCommond(ds, conn, trans, comm);
                    }
                    else
                    {
                        //创建数据库文件之后（包括删除数据部分）
                        CreateCommond(ds, conn, trans, comm, 11);
                    }

                    trans = conn.BeginTransaction();

                    //修改版本信息
                    StringBuilder sql = new StringBuilder();
                    long ticks = ToUnixTime(model.CreateTime);
                    sql.AppendFormat("insert into t0 (c0,c1,c2,c3) select 1,{0},'{1}','{2}' where not exists (select c0 from t0 where c0=1);", model.Version, ticks, code);
                    sql.AppendFormat("update t0 set c1={0},c2='{1}' where exists (select * from t0 where c0=1);", model.Version, ticks);
                    comm.CommandText = sql.ToString();
                    comm.ExecuteNonQuery();

                    trans.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                if (trans != null)
                {
                    trans.Rollback();
                }
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            return false;
        }

        /// <summary>
        /// 从服务器获取更新数据，包括删除的数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private DataSet GetData(SyncParamModel model)
        {
            StringBuilder sb = new StringBuilder();

            string sqlwhere, sqlwhere1, sqlwhere2;

            MySqlConnection conn = null;
            MySqlCommand comm = null;

            DataSet ds = new DataSet();

            try
            {
                string connstr = mysqlconnstr.Substring(mysqlconnstr.IndexOf("\"") + 1);
                connstr = connstr.Substring(0, connstr.Length - 1);

                conn = new MySqlConnection(connstr);
                conn.Open();
                comm = conn.CreateCommand();

                comm.CommandText = string.Format("SELECT CreateTime FROM DataVersion WHERE Version={0}", model.Version);
                MySqlDataReader reader = comm.ExecuteReader();
                DateTime time = DateTime.MinValue;
                while (reader.Read())
                {
                    time = Convert.ToDateTime(reader["CreateTime"]);
                }

                if (conn != null)
                {
                    conn.Close();
                }

                sqlwhere = string.Format(" (CreateTime>='{0}' or UpdateTime>='{0}') ", time.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sqlwhere1 = string.Format(" RegisterTime>='{0}' ", time.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sqlwhere2 = string.Format(" LogoutType<>0 AND LogoutTime>='{0}' ", time.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                sb.AppendFormat("SELECT ID FROM Region WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM Bag WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM Size WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM Scrap WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM BrandType WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM sys_user_dataview WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM Textile WHERE {0};", sqlwhere2);
                sb.AppendFormat("SELECT ID FROM Category WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM TextileClass WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM Fabric WHERE {0};", sqlwhere);
                sb.AppendFormat("SELECT ID FROM Color WHERE {0};", sqlwhere);

                sb.AppendFormat("SELECT ID,ParentID,FullName,RegionName,RegionType,RegionMode,BrandID,LogoUrl,Sort,DeliveryTime,HandConfirm FROM Region WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,BagNo,BagRFIDNo,RegionID FROM Bag WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,SizeName,Sort FROM Size WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,ScrapName,ScrapType,ScrapDesc FROM Scrap WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,BrandName,BrandCode,BrandDesc,Sort FROM BrandType WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,UserID,RegionID FROM sys_user_dataview WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,ClassID,SizeID FROM ClassSize;");
                sb.AppendFormat("SELECT ID,ClassID,SizeID,FabricID,IsFlag,RegionID,BrandID,WashTimes,unix_timestamp(RegisterTime)*1000 AS RegisterTime,TagNo,TextileState,0 AS RecieveTime,0 AS SendTime,0 AS InFactoryTime FROM Textile WHERE LogoutType=0 AND {0};", sqlwhere1);
                sb.AppendFormat("SELECT ID,CateName FROM Category WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,ClassName,ClassCode,Sort,ClassLeft,PackCount,CateID,IsRFID FROM TextileClass WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,FabricName FROM Fabric WHERE IsDelete=0 AND {0};", sqlwhere);
                sb.AppendFormat("SELECT ID,ColorName,Sort FROM Color WHERE IsDelete=0 AND {0};", sqlwhere);
                //sb.AppendFormat("SELECT ID,RFIDTagNo,RFIDWashtime,unix_timestamp(CostTime)*1000 AS CostTime,RFIDState FROM rfidtag;");
                sb.AppendFormat("SELECT TextileID,OldTagNo,NewTagNo FROM rfidreplace WHERE CreateTime>='{0}' ORDER BY CreateTime;", time.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append("SELECT `Code` FROM sys_customer");//客户的系统编码

                comm.CommandText = sb.ToString();
                comm.CommandTimeout = 0;

                MySqlDataAdapter adapter = new MySqlDataAdapter(comm);
                adapter.Fill(ds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return ds;
        }

        /// <summary>
        /// 创建服务器版本号
        /// </summary>
        /// <param name="model"></param>
        private void UpdateVersion(SyncParamModel model)
        {
            string sql = string.Format("insert into DataVersion (UUID,CreateTime) values ('{0}','{1}');", model.UUID, model.CreateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            MySqlConnection conn = null;
            MySqlCommand comm = null;
            try
            {
                string connstr = mysqlconnstr.Substring(mysqlconnstr.IndexOf("\"") + 1);
                connstr = connstr.Substring(0, connstr.Length - 1);

                conn = new MySqlConnection(connstr);
                conn.Open();
                comm = conn.CreateCommand();
                comm.CommandText = sql.ToString();

                comm.ExecuteNonQuery();

                model.Version = comm.LastInsertedId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 组装commond指令，分页事务提交，sql语句插入数据库
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="comm"></param>
        /// <param name="delcount">装有删除数据的表格个数</param>
        private void CreateCommondSQL(DataSet ds, DbConnection conn, DbTransaction trans, DbCommand comm, int delcount)
        {
            string sql = string.Empty;
            DbParameter param = null;

            string[] array = new string[] { "delete from t12;" };
            for (int i = 0; i < array.Length; i++)
            {
                comm.CommandText = "insert into t0 (c1) values (@p1);";
                param = comm.CreateParameter();
                param.ParameterName = "@p1";
                param.DbType = DbType.String;
                param.Value = array[i];
                comm.Parameters.Add(param);
                comm.ExecuteNonQuery();
            }

            string val = string.Empty;
            int m = 0;
            //最后一张表为系统编码，消息推送使用
            for (int k = 0; k < ds.Tables.Count - 1; k++)
            {
                DataTable table = ds.Tables[k];

                for (int j = 0; j < table.Rows.Count; j++)
                {
                    DataRow row = table.Rows[j];

                    if (k >= 0 && k < delcount)
                    {
                        sql = string.Format("DELETE FROM t{0} WHERE c0={1}", k + 1, row[0]);
                    }
                    else
                    {
                        #region 组装数值

                        val = string.Empty;

                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            if (row[i] is DBNull)
                            {
                                val += "null,";
                                continue;
                            }

                            if (table.Columns[i].DataType == typeof(Int32) || table.Columns[i].DataType == typeof(Int64))
                            {
                                val += row[i] + ",";
                            }
                            else
                            {
                                val += "'" + row[i] + "',";
                            }
                        }

                        if (val.EndsWith(","))
                        {
                            val = val.Substring(0, val.Length - 1);
                        }

                        #endregion

                        switch (k - delcount)
                        {
                            case 0:
                                sql = string.Format("insert into t1 select {0} where not exists (select c0 from t1 where c0={1});", val, row[0]);
                                break;
                            case 1:
                                sql = string.Format("insert into t2 select {0} where not exists (select c0 from t2 where c0={1});", val, row[0]);
                                break;
                            case 2:
                                sql = string.Format("insert into t3 select {0} where not exists (select c0 from t3 where c0={1});", val, row[0]);
                                break;
                            case 3:
                                sql = string.Format("insert into t4 select {0} where not exists (select c0 from t4 where c0={1});", val, row[0]);
                                break;
                            case 4:
                                sql = string.Format("insert into t5 select {0} where not exists (select c0 from t5 where c0={1});", val, row[0]);
                                break;
                            case 5:
                                sql = string.Format("insert into t6 select {0} where not exists (select c0 from t6 where c0={1});", val, row[0]);
                                break;
                            case 6:
                                sql = string.Format("insert into t12 select {0} where not exists (select c0 from t12 where c0={1});", val, row[0]);
                                break;
                            case 7:
                                sql = string.Format("insert into t7 select {0} where not exists (select c0 from t7 where c0={1});", val, row[0]);
                                break;
                            case 8:
                                sql = string.Format("insert into t8 select {0} where not exists (select c0 from t8 where c0={1});", val, row[0]);
                                break;
                            case 9:
                                sql = string.Format("insert into t9 select {0} where not exists (select c0 from t9 where c0={1});", val, row[0]);
                                break;
                            case 10:
                                sql = string.Format("insert into t10 select {0} where not exists (select c0 from t10 where c0={1});", val, row[0]);
                                break;
                            case 11:
                                sql = string.Format("insert into t11 select {0} where not exists (select c0 from t11 where c0={1});", val, row[0]);
                                break;
                            //case 12:
                            //    sql = string.Format("insert into t13 select {0} where not exists (select c0 from t13 where c0={1});", val, row[0]);
                            //    break;
                            case 12://处理芯片更换
                                sql = string.Format("UPDATE t7 SET c9='{0}' WHERE c0={1} OR c9='{2}';", row[2], row[0], row[1]);
                                break;
                            default:
                                break;
                        }
                    }
                    m++;
                    comm.CommandText = "insert into t0 (c1) values (@p1);";

                    param = comm.CreateParameter();
                    param.ParameterName = "@p1";
                    param.DbType = DbType.String;
                    param.Value = sql;
                    comm.Parameters.Add(param);
                    comm.ExecuteNonQuery();

                    if (m % 5000 == 0)
                    {
                        trans.Commit();
                        trans = conn.BeginTransaction();
                    }
                }
            }
            trans.Commit();
        }

        #region 第一次同步（sqlite总库初始化）

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="path">根路径</param>
        /// <param name="floder">文件夹</param>
        /// <param name="dbname">数据库名称</param>
        /// <returns>true：已经存在 false：不存在</returns>
        private bool CreateDBFile(string path, string floder, string dbname, bool overwrite = false)
        {
            string source = string.Format("{0}DBData\\{1}\\{2}", path, floder, dbname);
            string dest = string.Format("{0}DBData\\{1}", path, floder);

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }
            if (overwrite)
            {
                File.Copy(path + "\\App_Data\\" + dbname, source, overwrite);
            }
            else
            {
                if (!File.Exists(source))
                {
                    File.Copy(path + "\\App_Data\\" + dbname, source);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取sqlite数据库中最新版本号
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private SyncParamModel GetVersion(string path)
        {
            SyncParamModel model = new SyncParamModel() { Version = 0, CreateTime = DateTime.MinValue };

            string connectionstr = string.Format("Data Source={0};Version=3;Pooling=False;BinaryGUID=False;Max Pool Size=100;", path);
            DbConnection conn = null;
            DbCommand comm = null;
            DbDataReader reader = null;

            try
            {
                conn = new SQLiteConnection(connectionstr);
                conn.Open();
                comm = conn.CreateCommand();
                comm.CommandText = "select c0,c1,c2 from t0;";
                reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    model.Version = Convert.ToInt32(reader["c1"]);
                    DateTime time = FromUnixTime(Convert.ToInt64(reader["c2"]));
                    model.CreateTime = time;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return model;
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns></returns>
        private DataSet GetData()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ID,ParentID,FullName,RegionName,RegionType,RegionMode,BrandID,LogoUrl,Sort,DeliveryTime,HandConfirm FROM Region WHERE IsDelete=0;");
            sb.Append("SELECT ID,BagNo,BagRFIDNo,RegionID FROM Bag WHERE IsDelete=0;");
            sb.Append("SELECT ID,SizeName,Sort FROM Size WHERE IsDelete=0;");
            sb.Append("SELECT ID,ScrapName,ScrapType,ScrapDesc FROM Scrap WHERE IsDelete=0;");
            sb.Append("SELECT ID,BrandName,BrandCode,BrandDesc,Sort FROM BrandType WHERE IsDelete=0;");
            sb.Append("SELECT ID,UserID,RegionID FROM sys_user_dataview WHERE IsDelete=0;");
            sb.Append("SELECT ID,ClassID,SizeID FROM ClassSize;");
            sb.Append("SELECT ID,ClassID,SizeID,FabricID,IsFlag,RegionID,BrandID,WashTimes,unix_timestamp(RegisterTime)*1000 AS RegisterTime,TagNo,TextileState,0 AS RecieveTime,0 AS SendTime,0 AS InFactoryTime FROM Textile WHERE LogoutType=0;");
            sb.Append("SELECT ID,CateName FROM Category WHERE IsDelete=0;");
            sb.Append("SELECT ID,ClassName,ClassCode,Sort,ClassLeft,PackCount,CateID,IsRFID FROM TextileClass WHERE IsDelete=0;");
            sb.Append("SELECT ID,FabricName FROM Fabric WHERE IsDelete=0;");
            sb.Append("SELECT ID,ColorName,Sort FROM Color WHERE IsDelete=0;");
            //sb.Append("SELECT ID,RFIDTagNo,RFIDWashtime,unix_timestamp(CostTime)*1000 AS CostTime,RFIDState FROM rfidtag;");
            sb.Append("SELECT `Code` FROM sys_customer");//客户的系统编码

            MySqlConnection conn = null;
            MySqlCommand comm = null;

            DataSet ds = new DataSet();

            try
            {
                string connstr = mysqlconnstr.Substring(mysqlconnstr.IndexOf("\"") + 1);
                connstr = connstr.Substring(0, connstr.Length - 1);

                conn = new MySqlConnection(connstr);
                comm = conn.CreateCommand();
                comm.CommandText = sb.ToString();
                comm.CommandTimeout = 0;

                MySqlDataAdapter adapter = new MySqlDataAdapter(comm);
                adapter.Fill(ds);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ds;
        }

        /// <summary>
        /// 拼接指令
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="comm"></param>
        private void CreateCommond(DataSet ds, DbConnection conn, DbTransaction trans, DbCommand comm)
        {
            string[] array = new string[] { "@p1", "@p2", "@p3", "@p4", "@p5", "@p6", "@p7", "@p8", "@p9", "@p10", "@p11", "@p12", "@p13", "@p14" };
            DbParameter[] paramarray = null;

            int m = 0;
            //最后一张表为系统编码，消息推送使用
            for (int k = 0; k < ds.Tables.Count - 1; k++)
            {
                DataTable table = ds.Tables[k];

                paramarray = new DbParameter[table.Columns.Count];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    paramarray[j] = comm.CreateParameter();
                    paramarray[j].ParameterName = array[j];
                    comm.Parameters.Add(paramarray[j]);
                    //paramarray[j].DbType = table.Columns[j].DataType == typeof(Int32) ? DbType.Int32 : (table.Columns[j].DataType == typeof(Int64) ? DbType.Int64 : DbType.String);
                }

                switch (k)
                {
                    case 0:
                        comm.CommandText = "insert into t1 select @p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11 where not exists (select c0 from t1 where c0=@p1);";
                        break;
                    case 1:
                        comm.CommandText = "insert into t2 select @p1,@p2,@p3,@p4 where not exists (select c0 from t2 where c0=@p1);";
                        break;
                    case 2:
                        comm.CommandText = "insert into t3 select @p1,@p2,@p3 where not exists (select c0 from t3 where c0=@p1);";
                        break;
                    case 3:
                        comm.CommandText = "insert into t4 select @p1,@p2,@p3,@p4 where not exists (select c0 from t4 where c0=@p1);";
                        break;
                    case 4:
                        comm.CommandText = "insert into t5 select @p1,@p2,@p3,@p4,@p5 where not exists (select c0 from t5 where c0=@p1);";
                        break;
                    case 5:
                        comm.CommandText = "insert into t6 select @p1,@p2,@p3 where not exists (select c0 from t6 where c0=@p1);";
                        break;
                    case 6:
                        comm.CommandText = "insert into t12 select @p1,@p2,@p3 where not exists (select c0 from t12 where c0=@p1);";
                        break;
                    case 7:
                        comm.CommandText = "insert into t7 select @p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14 where not exists (select c0 from t7 where c0=@p1);";
                        break;
                    case 8:
                        comm.CommandText = "insert into t8 select @p1,@p2 where not exists (select c0 from t8 where c0=@p1);";
                        break;
                    case 9:
                        comm.CommandText = "insert into t9 select @p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8 where not exists (select c0 from t9 where c0=@p1);";
                        break;
                    case 10:
                        comm.CommandText = "insert into t10 select @p1,@p2 where not exists (select c0 from t10 where c0=@p1);";
                        break;
                    case 11:
                        comm.CommandText = "insert into t11 select @p1,@p2,@p3 where not exists (select c0 from t11 where c0=@p1);";
                        break;
                    //case 12:
                    //    comm.CommandText = "insert into t13 select @p1,@p2,@p3,@p4,@p5 where not exists (select c0 from t13 where c0=@p1);";
                    //    break;
                    case 12://处理芯片更换
                        comm.CommandText = "UPDATE t7 SET c9=@p3 WHERE c0=@p1 OR c9=@p2;";
                        break;
                    default:
                        break;
                }

                DataRow row = null;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    row = table.Rows[i];
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        paramarray[j].Value = row[j];
                    }
                    m++;
                    comm.ExecuteNonQuery();
                    if (m % 5000 == 0)
                    {
                        trans.Commit();
                        trans = conn.BeginTransaction();
                    }
                }
            }
            trans.Commit();
        }

        /// <summary>
        /// 组装commond指令，分页事务提交
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="comm"></param>
        /// <param name="delcount">装有删除数据的表格个数</param>
        private void CreateCommond(DataSet ds, DbConnection conn, DbTransaction trans, DbCommand comm, int delcount)
        {
            string[] array = new string[] { "@p1", "@p2", "@p3", "@p4", "@p5", "@p6", "@p7", "@p8", "@p9", "@p10", "@p11", "@p12", "@p13", "@p14" };
            DbParameter[] paramarray = null;

            comm.CommandText = "delete from t12;";
            comm.ExecuteNonQuery();
            int m = 0;
            //最后一张表为系统编码，消息推送使用
            for (int k = 0; k < ds.Tables.Count - 1; k++)
            {
                DataTable table = ds.Tables[k];

                paramarray = new DbParameter[table.Columns.Count];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    paramarray[j] = comm.CreateParameter();
                    paramarray[j].ParameterName = array[j];
                    comm.Parameters.Add(paramarray[j]);
                    paramarray[j].DbType = table.Columns[j].DataType == typeof(Int32) ? DbType.Int32 : (table.Columns[j].DataType == typeof(Int64) ? DbType.Int64 : DbType.String);
                }

                if (k >= 0 && k < delcount)
                {
                    comm.CommandText = string.Format("DELETE FROM t{0} WHERE c0=@p1", k + 1);
                }
                else
                {
                    switch (k - delcount)
                    {
                        case 0:
                            comm.CommandText = "insert into t1 select @p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11 where not exists (select c0 from t1 where c0=@p1);";
                            break;
                        case 1:
                            comm.CommandText = "insert into t2 select @p1,@p2,@p3,@p4 where not exists (select c0 from t2 where c0=@p1);";
                            break;
                        case 2:
                            comm.CommandText = "insert into t3 select @p1,@p2,@p3 where not exists (select c0 from t3 where c0=@p1);";
                            break;
                        case 3:
                            comm.CommandText = "insert into t4 select @p1,@p2,@p3,@p4 where not exists (select c0 from t4 where c0=@p1);";
                            break;
                        case 4:
                            comm.CommandText = "insert into t5 select @p1,@p2,@p3,@p4,@p5 where not exists (select c0 from t5 where c0=@p1);";
                            break;
                        case 5:
                            comm.CommandText = "insert into t6 select @p1,@p2,@p3 where not exists (select c0 from t6 where c0=@p1);";
                            break;
                        case 6:
                            comm.CommandText = "insert into t12 select @p1,@p2,@p3 where not exists (select c0 from t12 where c0=@p1);";
                            break;
                        case 7:
                            comm.CommandText = "insert into t7 select @p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14 where not exists (select c0 from t7 where c0=@p1);";
                            break;
                        case 8:
                            comm.CommandText = "insert into t8 select @p1,@p2 where not exists (select c0 from t8 where c0=@p1);";
                            break;
                        case 9:
                            comm.CommandText = "insert into t9 select @p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8 where not exists (select c0 from t9 where c0=@p1);";
                            break;
                        case 10:
                            comm.CommandText = "insert into t10 select @p1,@p2 where not exists (select c0 from t10 where c0=@p1);";
                            break;
                        case 11:
                            comm.CommandText = "insert into t11 select @p1,@p2,@p3 where not exists (select c0 from t11 where c0=@p1);";
                            break;
                        //case 12:
                        //    comm.CommandText = "insert into t13 select @p1,@p2,@p3,@p4,@p5 where not exists (select c0 from t13 where c0=@p1);";
                        //    break;
                        case 12://处理芯片更换
                            comm.CommandText = "UPDATE t7 SET c9=@p3 WHERE c0=@p1 OR c9=@p2;";
                            break;
                        default:
                            break;
                    }
                }
                DataRow row = null;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    row = table.Rows[i];
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        paramarray[j].Value = row[j];
                    }
                    m++;
                    comm.ExecuteNonQuery();
                    if (m % 5000 == 0)
                    {
                        trans.Commit();
                        trans = conn.BeginTransaction();
                    }
                }
            }
            trans.Commit();
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
