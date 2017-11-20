using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ETextsys.Terminal.Utilities
{
    public class ExcelRender
    {/// <summary>
     /// 文件转Stream
     /// 
     /// </summary>
     /// <param name="file"></param>
     /// <returns></returns>
        public static Stream FileToStream(string file)
        {
            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]  
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 转换成 Stream  
            Stream stream = new MemoryStream(bytes);
            return stream;

        }

        /// <summary>
        /// 根据Excel列类型获取列的值
        /// </summary>
        /// <param name="cell">Excel列</param>
        /// <returns></returns>
        private static string GetCellValue(ICell cell)
        {
            if (cell == null)
                return string.Empty;
            switch (cell.CellType)
            {
                case CellType.Blank:
                    return string.Empty;
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Error:
                    return cell.ErrorCellValue.ToString();
                case CellType.Numeric:
                    short format = cell.CellStyle.DataFormat;
                    string de = cell.ToString();
                    if (format == 14 || format == 31 || format == 57 || format == 58)
                    {
                        DateTime date = cell.DateCellValue;
                        de = date.ToString("yyyy-MM-dd HH:mm");

                    }
                    return de;
                    break;
                case CellType.Unknown:
                default:
                    return cell.ToString();//This is a trick to get the correct value of the cell. NumericCellValue will return a numeric value no matter the cell value is a date or a number
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Formula:
                    try
                    {
                        HSSFFormulaEvaluator e = new HSSFFormulaEvaluator(cell.Sheet.Workbook);
                        e.EvaluateInCell(cell);
                        return cell.ToString();
                    }
                    catch
                    {
                        return cell.NumericCellValue.ToString();
                    }
            }
        }

        /// <summary>
        /// 自动设置Excel列宽
        /// </summary>
        /// <param name="sheet">Excel表</param>
        private static void AutoSizeColumns(ISheet sheet)
        {
            if (sheet.PhysicalNumberOfRows > 0)
            {
                IRow headerRow = sheet.GetRow(0);

                for (int i = 0, l = headerRow.LastCellNum; i < l; i++)
                {
                    sheet.AutoSizeColumn(i, true);
                }
            }
        }

        /// <summary>
        /// 自动设置Excel列宽
        /// </summary>
        /// <param name="sheet">Excel表</param>
        private static void AutoSizeColumns(ISheet sheet, int index)
        {
            if (sheet.PhysicalNumberOfRows > index)
            {
                IRow headerRow = sheet.GetRow(index);

                for (int i = 0, l = headerRow.LastCellNum; i < l; i++)
                {
                    sheet.AutoSizeColumn(i, true);
                }
            }
        }

        /// <summary>
        /// 保存Excel文档流到文件
        /// </summary>
        /// <param name="ms">Excel文档流</param>
        /// <param name="fileName">文件名</param>
        private static void SaveToFile(MemoryStream ms, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                byte[] data = ms.ToArray();

                fs.Write(data, 0, data.Length);
                fs.Flush();

                data = null;
            }
        }

        /// <summary>
        /// 输出文件到浏览器
        /// </summary>
        /// <param name="ms">Excel文档流</param>
        /// <param name="context">HTTP上下文</param>
        /// <param name="fileName">文件名</param>
        private static void RenderToBrowser(MemoryStream ms, HttpContext context, string fileName)
        {
            context.Response.Buffer = true;
            context.Response.Clear();
            context.Response.ClearContent();
            context.Response.ClearHeaders();
            if (context.Request.Browser.Browser == "IE")
                fileName = HttpUtility.UrlEncode(fileName);
            context.Response.AddHeader("Content-Disposition", "attachment;fileName=" + fileName);
            context.Response.BinaryWrite(ms.ToArray());
        }

        /// <summary>
        /// DataReader转换成Excel文档流
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static MemoryStream RenderToExcel(IDataReader reader)
        {
            MemoryStream ms = new MemoryStream();

            using (reader)
            {
                IWorkbook workbook = new HSSFWorkbook();

                ISheet sheet = workbook.CreateSheet();

                IRow headerRow = sheet.CreateRow(0);
                int cellCount = reader.FieldCount;

                // handling header.
                for (int i = 0; i < cellCount; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(reader.GetName(i));
                }

                // handling value.
                int rowIndex = 1;
                while (reader.Read())
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    for (int i = 0; i < cellCount; i++)
                    {
                        dataRow.CreateCell(i).SetCellValue(reader[i].ToString());
                    }

                    rowIndex++;
                }

                AutoSizeColumns(sheet);

                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }
            return ms;
        }

        /// <summary>
        /// DataReader转换成Excel文档流，并保存到文件
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fileName">保存的路径</param>
        public static void RenderToExcel(IDataReader reader, string fileName)
        {
            using (MemoryStream ms = RenderToExcel(reader))
            {
                SaveToFile(ms, fileName);
            }
        }

        /// <summary>
        /// DataReader转换成Excel文档流，并输出到客户端
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="context">HTTP上下文</param>
        /// <param name="fileName">输出的文件名</param>
        public static void RenderToExcel(IDataReader reader, HttpContext context, string fileName)
        {
            using (MemoryStream ms = RenderToExcel(reader))
            {
                RenderToBrowser(ms, context, fileName);
            }
        }

        /// <summary>
        /// DataTable转换成Excel文档流
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static MemoryStream RenderToExcel(DataTable table)
        {
            MemoryStream ms = new MemoryStream();

            using (table)
            {
                IWorkbook workbook = new HSSFWorkbook();

                ISheet sheet = workbook.CreateSheet();

                IRow headerRow = sheet.CreateRow(0);

                // handling header.
                foreach (DataColumn column in table.Columns)
                    headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value

                // handling value.
                int rowIndex = 1;

                foreach (DataRow row in table.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in table.Columns)
                    {
                        dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }

                    rowIndex++;
                }
                AutoSizeColumns(sheet);

                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }

            return ms;
        }

        /// <summary>
        /// DataTable转换成Excel文档流 包含标题
        /// </summary>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(DataTable table, string title, List<string> condition, string createTime)
        {
            MemoryStream ms = new MemoryStream();

            using (table)
            {
                IWorkbook workbook = new HSSFWorkbook();

                ISheet sheet = workbook.CreateSheet();
                sheet.DisplayGridlines = false;

                NPOI.SS.UserModel.ICellStyle ReportHeaderStyle = workbook.CreateCellStyle();
                ReportHeaderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                ReportHeaderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                IFont font1 = workbook.CreateFont();
                font1.FontHeightInPoints = 17;
                font1.FontName = "微软雅黑";
                ReportHeaderStyle.SetFont(font1);

                NPOI.SS.UserModel.ICellStyle ConditionStyle = workbook.CreateCellStyle();
                ConditionStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                ConditionStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                IFont conditionfont1 = workbook.CreateFont();
                conditionfont1.FontHeightInPoints = 12;
                conditionfont1.FontName = "微软雅黑";
                ConditionStyle.SetFont(conditionfont1);

                NPOI.SS.UserModel.ICellStyle rConditionStyle = workbook.CreateCellStyle();
                rConditionStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
                rConditionStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                rConditionStyle.SetFont(conditionfont1);

                //title header
                IRow titleRow = sheet.CreateRow(0);
                ICell titleCell = titleRow.CreateCell(0, CellType.String);
                titleCell.CellStyle = ReportHeaderStyle;
                titleCell.SetCellValue(title);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, table.Columns.Count - 1));

                IRow conditonRow = null;
                ICell condtionCell = null;
                int trow = 0;
                for (int i = 0; i < condition.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        trow = i / 2 + 1;
                        conditonRow = sheet.CreateRow(trow);
                        condtionCell = conditonRow.CreateCell(0, CellType.String);
                        condtionCell.SetCellValue(condition[i]);
                        condtionCell.CellStyle = ConditionStyle;
                        sheet.AddMergedRegion(new CellRangeAddress(trow, trow, 0, (table.Columns.Count - 1) / 2));
                    }
                    else
                    {
                        condtionCell = conditonRow.CreateCell((table.Columns.Count - 1) / 2 + 1, CellType.String);
                        condtionCell.SetCellValue(condition[i]);
                        condtionCell.CellStyle = rConditionStyle;
                        sheet.AddMergedRegion(new CellRangeAddress(trow, trow, (table.Columns.Count - 1) / 2 + 1, (table.Columns.Count - 1)));
                    }
                }

                ICellStyle style = workbook.CreateCellStyle();
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BottomBorderColor = HSSFColor.Black.Index;
                style.LeftBorderColor = HSSFColor.Black.Index;
                style.RightBorderColor = HSSFColor.Black.Index;
                style.TopBorderColor = HSSFColor.Black.Index;
                style.VerticalAlignment = VerticalAlignment.Center;

                IFont rowFont = workbook.CreateFont();
                rowFont.FontHeightInPoints = 11;
                rowFont.FontName = "微软雅黑";
                style.SetFont(rowFont);

                trow++;

                IRow headerRow = sheet.CreateRow(trow + 1);

                // handling header.
                foreach (DataColumn column in table.Columns)
                {
                    HSSFCell newCell = headerRow.CreateCell(column.Ordinal) as HSSFCell;
                    if (column.Caption.Substring(0, 1) == "D")
                        newCell.SetCellValue(column.Caption.Substring(1, column.Caption.Length - 1));
                    else
                        newCell.SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value
                    newCell.CellStyle = style;
                }

                // handling value.
                int rowIndex = trow + 2;

                foreach (DataRow row in table.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in table.Columns)
                    {
                        HSSFCell newCell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                        string drValue = row[column].ToString();
                        int v = 0;
                        int.TryParse(drValue, out v);
                        if ((v > 0 || v < 0) || drValue == "0")
                        {
                            if (v == 0)
                            {
                                newCell.SetCellValue("");
                            }
                            else
                                newCell.SetCellValue(v);
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                        newCell.CellStyle = style;
                        //dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }

                    rowIndex++;
                }

                rowIndex++;
                NPOI.SS.UserModel.ICellStyle timeStyle = workbook.CreateCellStyle();
                timeStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
                timeStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;

                IRow timeRow = sheet.CreateRow(rowIndex);
                ICell timeCell = timeRow.CreateCell(0, CellType.String);
                timeCell.SetCellValue(string.Format("制表时间：{0}", createTime));
                timeCell.CellStyle = timeStyle;
                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, table.Columns.Count - 1));

                AutoSizeColumns(sheet, 1);

                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }

            return ms;
        }

        /// <summary>
        /// DataTable转换成Excel文档流 包含标题
        /// </summary>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(Dictionary<string, DataTable> dic, List<string> condition, string createTime)
        {
            MemoryStream ms = new MemoryStream();


            IWorkbook workbook = new HSSFWorkbook();

            ISheet sheet = workbook.CreateSheet();
            sheet.DisplayGridlines = false;

            NPOI.SS.UserModel.ICellStyle ReportHeaderStyle = workbook.CreateCellStyle();
            ReportHeaderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            ReportHeaderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            IFont font1 = workbook.CreateFont();
            font1.FontHeightInPoints = 17;
            font1.FontName = "微软雅黑";
            ReportHeaderStyle.SetFont(font1);

            NPOI.SS.UserModel.ICellStyle ConditionStyle = workbook.CreateCellStyle();
            ConditionStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            ConditionStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            IFont conditionfont1 = workbook.CreateFont();
            conditionfont1.FontHeightInPoints = 12;
            conditionfont1.FontName = "微软雅黑";
            ConditionStyle.SetFont(conditionfont1);

            NPOI.SS.UserModel.ICellStyle rConditionStyle = workbook.CreateCellStyle();
            rConditionStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
            rConditionStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            rConditionStyle.SetFont(conditionfont1);

            int currentRowIndex = 0;

            foreach (var item in dic)
            {
                DataTable table = item.Value;

                if (table.Rows.Count == 0)
                    continue;

                //title header
                IRow titleRow = sheet.CreateRow(currentRowIndex);
                ICell titleCell = titleRow.CreateCell(0, CellType.String);
                titleCell.CellStyle = ReportHeaderStyle;
                titleCell.SetCellValue(item.Key);
                sheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 0, table.Columns.Count - 1));

                IRow conditonRow = null;
                ICell condtionCell = null;

                for (int i = 0; i < condition.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        currentRowIndex += i / 2 + 1;
                        conditonRow = sheet.CreateRow(currentRowIndex);
                        condtionCell = conditonRow.CreateCell(0, CellType.String);
                        condtionCell.SetCellValue(condition[i]);
                        condtionCell.CellStyle = ConditionStyle;
                        sheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 0, (table.Columns.Count - 1) / 2));
                    }
                    else
                    {
                        condtionCell = conditonRow.CreateCell((table.Columns.Count - 1) / 2 + 1, CellType.String);
                        condtionCell.SetCellValue(condition[i]);
                        condtionCell.CellStyle = rConditionStyle;
                        sheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, (table.Columns.Count - 1) / 2 + 1, (table.Columns.Count - 1)));
                    }
                }

                ICellStyle style = workbook.CreateCellStyle();
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BottomBorderColor = HSSFColor.Black.Index;
                style.LeftBorderColor = HSSFColor.Black.Index;
                style.RightBorderColor = HSSFColor.Black.Index;
                style.TopBorderColor = HSSFColor.Black.Index;
                style.VerticalAlignment = VerticalAlignment.Center;

                IFont rowFont = workbook.CreateFont();
                rowFont.FontHeightInPoints = 11;
                rowFont.FontName = "微软雅黑";
                style.SetFont(rowFont);

                currentRowIndex += 2;

                IRow headerRow = sheet.CreateRow(currentRowIndex);

                // handling header.
                foreach (DataColumn column in table.Columns)
                {
                    HSSFCell newCell = headerRow.CreateCell(column.Ordinal) as HSSFCell;
                    if (column.Caption.Substring(0, 1) == "D")
                        newCell.SetCellValue(column.Caption.Substring(1, column.Caption.Length - 1));
                    else
                        newCell.SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value
                    newCell.CellStyle = style;
                }

                // handling value.
                currentRowIndex = currentRowIndex + 1;

                foreach (DataRow row in table.Rows)
                {
                    IRow dataRow = sheet.CreateRow(currentRowIndex);

                    foreach (DataColumn column in table.Columns)
                    {
                        HSSFCell newCell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                        string drValue = row[column].ToString();
                        int v = 0;
                        int.TryParse(drValue, out v);
                        if ((v > 0 || v < 0) || drValue == "0")
                        {
                            if (v == 0)
                            {
                                newCell.SetCellValue("");
                            }
                            else
                                newCell.SetCellValue(v);
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                        newCell.CellStyle = style;
                        //dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }

                    currentRowIndex++;
                }
                NPOI.SS.UserModel.ICellStyle timeStyle = workbook.CreateCellStyle();
                timeStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
                timeStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;

                IRow timeRow = sheet.CreateRow(currentRowIndex);
                ICell timeCell = timeRow.CreateCell(0, CellType.String);
                timeCell.SetCellValue(string.Format("制表时间：{0}", createTime));
                timeCell.CellStyle = timeStyle;
                sheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 0, table.Columns.Count - 1));

                currentRowIndex++;
            }

            AutoSizeColumns(sheet, 1);

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;


            return ms;
        }

        /// <summary>
        /// DataTable转换成Excel文档流 包含标题
        /// </summary>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(DataTable table, string title)
        {
            MemoryStream ms = new MemoryStream();

            using (table)
            {
                IWorkbook workbook = new HSSFWorkbook();

                ISheet sheet = workbook.CreateSheet();

                NPOI.SS.UserModel.ICellStyle ReportHeaderStyle = workbook.CreateCellStyle();
                ReportHeaderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                ReportHeaderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                IFont font1 = workbook.CreateFont();
                font1.Boldweight = short.MaxValue;
                font1.FontHeightInPoints = 17;
                ReportHeaderStyle.SetFont(font1);

                //title header
                IRow titleRow = sheet.CreateRow(0);
                ICell titleCell = titleRow.CreateCell(0, CellType.String);
                titleCell.CellStyle = ReportHeaderStyle;
                titleCell.SetCellValue(title);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, table.Columns.Count - 1));

                IRow headerRow = sheet.CreateRow(1);

                // handling header.
                foreach (DataColumn column in table.Columns)
                {
                    if (column.Caption.Substring(0, 1) == "D")
                        headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption.Substring(1, column.Caption.Length - 1));
                    else
                        headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value
                }

                // handling value.
                int rowIndex = 2;

                foreach (DataRow row in table.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in table.Columns)
                    {
                        HSSFCell newCell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                        string drValue = row[column].ToString();
                        int v = 0;
                        int.TryParse(drValue, out v);
                        if ((v > 0 || v < 0) || drValue == "0")
                        {
                            if (v == 0)
                            {
                                newCell.SetCellValue("");
                            }
                            else
                                newCell.SetCellValue(v);
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                        //dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }

                    rowIndex++;
                }
                AutoSizeColumns(sheet, 1);

                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }

            return ms;
        }

                 /// <summary>
        /// DataTable转换成Excel文档流 包含标题
        /// </summary>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(DataTable table, string title, bool isAutoSizeColumns)
        {
            MemoryStream ms = new MemoryStream();

            using (table)
            {
                IWorkbook workbook = new HSSFWorkbook();

                ISheet sheet = workbook.CreateSheet();

                NPOI.SS.UserModel.ICellStyle ReportHeaderStyle = workbook.CreateCellStyle();
                ReportHeaderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                ReportHeaderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                IFont font1 = workbook.CreateFont();
                font1.Boldweight = short.MaxValue;
                font1.FontHeightInPoints = 17;
                ReportHeaderStyle.SetFont(font1);

                //title header
                IRow titleRow = sheet.CreateRow(0);
                ICell titleCell = titleRow.CreateCell(0, CellType.String);
                titleCell.CellStyle = ReportHeaderStyle;
                titleCell.SetCellValue(title);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, table.Columns.Count - 1));

                IRow headerRow = sheet.CreateRow(1);

                // handling header.
                foreach (DataColumn column in table.Columns)
                {
                    if (column.Caption.Substring(0, 1) == "D")
                        headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption.Substring(1, column.Caption.Length - 1));
                    else
                        headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value
                }

                // handling value.
                int rowIndex = 2;

                foreach (DataRow row in table.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in table.Columns)
                    {
                        HSSFCell newCell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                        string drValue = row[column].ToString();
                        int v = 0;
                        int.TryParse(drValue, out v);
                        if ((v > 0 || v < 0) || drValue == "0")
                        {
                            if (v == 0)
                            {
                                newCell.SetCellValue("");
                            }
                            else
                                newCell.SetCellValue(v);
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                        //dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }

                    rowIndex++;
                }

                if (isAutoSizeColumns)
                    AutoSizeColumns(sheet, 1);

                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }

            return ms;
        }

        /// <summary>
        /// 多个DataTable转换成Excel（多个Sheet）文档流
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(Dictionary<string, DataTable> dic, List<string> sheetList)
        {
            MemoryStream ms = new MemoryStream();

            IWorkbook workbook = new HSSFWorkbook();
            NPOI.SS.UserModel.ICellStyle ReportHeaderStyle = workbook.CreateCellStyle();
            ReportHeaderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            ReportHeaderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            IFont font1 = workbook.CreateFont();
            font1.Boldweight = short.MaxValue;
            font1.FontHeightInPoints = 17;
            ReportHeaderStyle.SetFont(font1);

            int i = 0;
            foreach (var item in dic)
            {
                ISheet sheet = workbook.CreateSheet(item.Key);

                //title header
                IRow titleRow = sheet.CreateRow(0);
                ICell titleCell = titleRow.CreateCell(0, CellType.String);
                titleCell.CellStyle = ReportHeaderStyle;
                titleCell.SetCellValue(sheetList[i]);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, item.Value.Columns.Count - 1));
                i++;

                IRow headerRow = sheet.CreateRow(1);

                // handling header.
                foreach (DataColumn column in item.Value.Columns)
                    headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value

                // handling value.
                int rowIndex = 2;

                foreach (DataRow row in item.Value.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in item.Value.Columns)
                    {
                        HSSFCell newCell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                        string drValue = row[column].ToString();
                        int v = 0;
                        int.TryParse(drValue, out v);
                        if (v > 0)
                        {
                            newCell.SetCellValue(v);
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                        //dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }

                    rowIndex++;
                }
                //AutoSizeColumns(sheet, 1);
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;

            return ms;
        }

        /// <summary>
        /// DataSet转换成Excel（多个Sheet）文档流
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(DataSet ds, List<string> sheetList)
        {
            MemoryStream ms = new MemoryStream();

            IWorkbook workbook = new HSSFWorkbook();
            NPOI.SS.UserModel.ICellStyle ReportHeaderStyle = workbook.CreateCellStyle();
            ReportHeaderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            ReportHeaderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            IFont font1 = workbook.CreateFont();
            font1.Boldweight = short.MaxValue;
            font1.FontHeightInPoints = 17;
            ReportHeaderStyle.SetFont(font1);

            int i = 0;
            foreach (DataTable item in ds.Tables)
            {
                ISheet sheet = workbook.CreateSheet(sheetList[i]);
                i++;

                IRow headerRow = sheet.CreateRow(0);
                // handling header.
                foreach (DataColumn column in item.Columns)
                    headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value

                // handling value.
                int rowIndex = 1;

                foreach (DataRow row in item.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in item.Columns)
                    {
                        HSSFCell newCell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                        string drValue = row[column].ToString();
                        int v = 0;
                        int.TryParse(drValue, out v);
                        if (v > 0)
                        {
                            newCell.SetCellValue(v);
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                        //dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }

                    rowIndex++;
                }
                AutoSizeColumns(sheet, 1);
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;

            return ms;
        }


        /// <summary>
        /// DataTable转换成Excel文档流，以表名为Sheet名
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(DataTable table)
        {
            MemoryStream ms = new MemoryStream();

            IWorkbook workbook = new HSSFWorkbook();
            NPOI.SS.UserModel.ICellStyle ReportHeaderStyle = workbook.CreateCellStyle();
            ReportHeaderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            ReportHeaderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            IFont font1 = workbook.CreateFont();
            font1.Boldweight = short.MaxValue;
            font1.FontHeightInPoints = 17;
            ReportHeaderStyle.SetFont(font1);

            int i = 0;

            ISheet sheet = workbook.CreateSheet(table.TableName);
            i++;

            IRow headerRow = sheet.CreateRow(0);
            // handling header.
            foreach (DataColumn column in table.Columns)
                headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value

            // handling value.
            int rowIndex = 1;

            foreach (DataRow row in table.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);

                foreach (DataColumn column in table.Columns)
                {
                    HSSFCell newCell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                    string drValue = row[column].ToString();
                    int v = 0;
                    int.TryParse(drValue, out v);
                    if (v > 0)
                    {
                        newCell.SetCellValue(v);
                    }
                    else
                    {
                        newCell.SetCellValue(drValue);
                    }
                    //dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                }

                rowIndex++;
            }
            AutoSizeColumns(sheet, 1);

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;

            return ms;
        }

        /// <summary>
        /// DataTable转换成Excel文档流，并保存到文件
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileName">保存的路径</param>
        public static void RenderToExcel(DataTable table, string fileName)
        {
            using (MemoryStream ms = RenderToExcel(table))
            {
                SaveToFile(ms, fileName);
            }
        }

        /// <summary>
        /// DataTable转换成Excel文档流，并保存到文件 包含标题头
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileName"></param>
        /// <param name="title"></param>
        public static void RenderToExcel(DataTable table, string fileName, string title)
        {
            using (MemoryStream ms = ReanderToExcel(table, title))
            {
                SaveToFile(ms, fileName);
            }
        }

        /// <summary>
        /// DataTable转换成Excel文档流，并保存到文件 包含标题头
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileName"></param>
        /// <param name="title"></param>
        public static void RenderToExcel(Dictionary<string, DataTable> dic, string fileName, List<string> sheetList)
        {
            using (MemoryStream ms = ReanderToExcel(dic, sheetList))
            {
                SaveToFile(ms, fileName);
            }
        }

        /// <summary>
        /// DataTable转换成Excel文档流，并输出到客户端
        /// </summary>
        /// <param name="table"></param>
        /// <param name="response"></param>
        /// <param name="fileName">输出的文件名</param>
        public static void RenderToExcel(DataTable table, HttpContext context, string fileName)
        {
            using (MemoryStream ms = RenderToExcel(table))
            {
                RenderToBrowser(ms, context, fileName);
            }
        }

        /// <summary>
        /// Excel文档流是否有数据
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <returns></returns>
        public static bool HasData(Stream excelFileStream)
        {
            return HasData(excelFileStream, 0);
        }

        /// <summary>
        /// Excel文档流是否有数据
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <param name="sheetIndex">表索引号，如第一个表为0</param>
        /// <returns></returns>
        public static bool HasData(Stream excelFileStream, int sheetIndex)
        {
            using (excelFileStream)
            {
                IWorkbook workbook = new HSSFWorkbook(excelFileStream);

                if (workbook.NumberOfSheets > 0)
                {
                    if (sheetIndex < workbook.NumberOfSheets)
                    {
                        ISheet sheet = workbook.GetSheetAt(sheetIndex);

                        return sheet.PhysicalNumberOfRows > 0;
                    }

                }
            }
            return false;
        }

        /// <summary>
        /// Excel文档流转换成DataTable
        /// 第一行必须为标题行
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <param name="sheetName">表名称</param>
        /// <returns></returns>
        public static DataTable RenderFromExcel(Stream excelFileStream, string sheetName)
        {
            return RenderFromExcel(excelFileStream, sheetName, 0);
        }

        /// <summary>
        /// Excel文档流转换成DataTable
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <param name="sheetName">表名称</param>
        /// <param name="headerRowIndex">标题行索引号，如第一行为0</param>
        /// <returns></returns>
        public static DataTable RenderFromExcel(Stream excelFileStream, string sheetName, int headerRowIndex)
        {
            DataTable table = null;

            using (excelFileStream)
            {
                IWorkbook workbook = new HSSFWorkbook(excelFileStream);

                ISheet sheet = workbook.GetSheet(sheetName);

                table = RenderFromExcel(sheet, headerRowIndex);

            }
            return table;
        }

        /// <summary>
        /// Excel文档流转换成DataTable
        /// 默认转换Excel的第一个表
        /// 第一行必须为标题行
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <returns></returns>
        public static DataTable RenderFromExcel(Stream excelFileStream)
        {
            return RenderFromExcel(excelFileStream, 0, 0);
        }

        /// <summary>
        /// Excel文档流转换成DataTable
        /// 第一行必须为标题行
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <param name="sheetIndex">表索引号，如第一个表为0</param>
        /// <returns></returns>
        public static DataTable RenderFromExcel(Stream excelFileStream, int sheetIndex)
        {
            return RenderFromExcel(excelFileStream, sheetIndex, 0);
        }

        /// <summary>
        /// Excel文档流转换成DataTable
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <param name="sheetIndex">表索引号，如第一个表为0</param>
        /// <param name="headerRowIndex">标题行索引号，如第一行为0</param>
        /// <returns></returns>
        public static DataTable RenderFromExcel(Stream excelFileStream, int sheetIndex, int headerRowIndex)
        {
            DataTable table = null;

            using (excelFileStream)
            {
                IWorkbook workbook = new HSSFWorkbook(excelFileStream);

                ISheet sheet = workbook.GetSheetAt(sheetIndex);

                table = RenderFromExcel(sheet, headerRowIndex);

            }
            return table;
        }

        /// <summary>
        /// Excel表格转换成DataTable
        /// </summary>
        /// <param name="sheet">表格</param>
        /// <param name="headerRowIndex">标题行索引号，如第一行为0</param>
        /// <returns></returns>
        private static DataTable RenderFromExcel(ISheet sheet, int headerRowIndex)
        {
            DataTable table = new DataTable();

            IRow headerRow = sheet.GetRow(headerRowIndex);
            int cellCount = headerRow.LastCellNum;//LastCellNum = PhysicalNumberOfCells
            int rowCount = sheet.LastRowNum;//LastRowNum = PhysicalNumberOfRows - 1

            //handling header.
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                table.Columns.Add(column);
            }

            for (int i = (sheet.FirstRowNum + 1); i <= rowCount; i++)
            {
                IRow row = sheet.GetRow(i);
                DataRow dataRow = table.NewRow();

                if (row != null)
                {
                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {
                        if (row.GetCell(j) != null)
                            dataRow[j] = GetCellValue(row.GetCell(j));
                    }
                }

                table.Rows.Add(dataRow);
            }

            return table;
        }

        /// <summary>
        /// Excel文档导入到数据库
        /// 默认取Excel的第一个表
        /// 第一行必须为标题行
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <param name="insertSql">插入语句</param>
        /// <param name="dbAction">更新到数据库的方法</param>
        /// <returns></returns>
        public static int RenderToDb(Stream excelFileStream, string insertSql, DBAction dbAction)
        {
            return RenderToDb(excelFileStream, insertSql, dbAction, 0, 0);
        }

        public delegate int DBAction(string sql, params IDataParameter[] parameters);

        /// <summary>
        /// Excel文档导入到数据库
        /// </summary>
        /// <param name="excelFileStream">Excel文档流</param>
        /// <param name="insertSql">插入语句</param>
        /// <param name="dbAction">更新到数据库的方法</param>
        /// <param name="sheetIndex">表索引号，如第一个表为0</param>
        /// <param name="headerRowIndex">标题行索引号，如第一行为0</param>
        /// <returns></returns>
        public static int RenderToDb(Stream excelFileStream, string insertSql, DBAction dbAction, int sheetIndex, int headerRowIndex)
        {
            int rowAffected = 0;
            using (excelFileStream)
            {
                IWorkbook workbook = new HSSFWorkbook(excelFileStream);

                ISheet sheet = workbook.GetSheetAt(sheetIndex);

                StringBuilder builder = new StringBuilder();

                IRow headerRow = sheet.GetRow(headerRowIndex);
                int cellCount = headerRow.LastCellNum;//LastCellNum = PhysicalNumberOfCells
                int rowCount = sheet.LastRowNum;//LastRowNum = PhysicalNumberOfRows - 1

                for (int i = (sheet.FirstRowNum + 1); i <= rowCount; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row != null)
                    {
                        builder.Append(insertSql);
                        builder.Append(" values (");
                        for (int j = row.FirstCellNum; j < cellCount; j++)
                        {
                            builder.AppendFormat("'{0}',", GetCellValue(row.GetCell(j)).Replace("'", "''"));
                        }
                        builder.Length = builder.Length - 1;
                        builder.Append(");");
                    }

                    if ((i % 50 == 0 || i == rowCount) && builder.Length > 0)
                    {
                        //每50条记录一次批量插入到数据库
                        rowAffected += dbAction(builder.ToString());
                        builder.Length = 0;
                    }


                }
            }
            return rowAffected;
        }
    }
}
