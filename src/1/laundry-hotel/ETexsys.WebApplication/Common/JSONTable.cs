using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ETexsys.WebApplication.Common
{
    /// <summary>
    /// JSON 与 Table 互转
    /// </summary>
    public static class JSONTable
    {


        /// <summary>
        /// DataTable 对象转 JSON 字符串
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToJson(DataTable dt)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>(); //实例化一个参数集合
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToStr());

                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值 
            }
            return javaScriptSerializer.Serialize(arrayList);
        }

        private static string ToStr(this object s, string format = "")
        {
            string result = "";
            try
            {
                if (format == "")
                {
                    result = s.ToString();
                }
                else
                {
                    result = string.Format("{0:" + format + "}", s);
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        /// <summary> 
        /// Json 字符串 转换为 DataTable数据集合 
        /// </summary> 
        /// <param name="json"></param> 
        /// <returns></returns> 
        public static DataTable ToDataTable(string json)
        {
            DataTable dataTable = new DataTable();  //实例化 
            DataTable result;

            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值 

                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);

                if (arrayList.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in arrayList)
                    {
                        if (dictionary.Keys.Count<string>() == 0)
                        {
                            result = dataTable;
                            return result;
                        }
                        if (dataTable.Columns.Count == 0)
                        {
                            foreach (string current in dictionary.Keys)
                            {
                                dataTable.Columns.Add(current, typeof(string));
                            }

                        }

                        DataRow dataRow = dataTable.NewRow();

                        foreach (string current in dictionary.Keys)
                        {
                            dataRow[current] = dictionary[current];
                        }

                        dataTable.Rows.Add(dataRow); //循环添加行到DataTable中 
                    }
                }
            }
            catch
            {

            }

            result = dataTable;
            return result;
        }
    }
}