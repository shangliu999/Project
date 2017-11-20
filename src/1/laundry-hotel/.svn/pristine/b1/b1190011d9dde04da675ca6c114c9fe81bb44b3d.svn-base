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

namespace ETexsys.Common.ExcelHelp
{
    public class ExcelRender
    {
        /// <summary>
        /// 文件转Stream 
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

        public static MemoryStream RenderToExcel(List<ExcelTableModel> list)
        {
            MemoryStream ms = new MemoryStream();

            IWorkbook workbook = new HSSFWorkbook();

            ISheet sheet = null;

            #region Style 

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



            IFont rowFont = workbook.CreateFont();
            rowFont.FontHeightInPoints = 11;
            rowFont.FontName = "微软雅黑";

            ICellStyle leftstyle = workbook.CreateCellStyle();
            leftstyle.BorderBottom = BorderStyle.Thin;
            leftstyle.BorderLeft = BorderStyle.Thin;
            leftstyle.BorderRight = BorderStyle.Thin;
            leftstyle.BorderTop = BorderStyle.Thin;
            leftstyle.BottomBorderColor = HSSFColor.Black.Index;
            leftstyle.LeftBorderColor = HSSFColor.Black.Index;
            leftstyle.RightBorderColor = HSSFColor.Black.Index;
            leftstyle.TopBorderColor = HSSFColor.Black.Index;
            leftstyle.VerticalAlignment = VerticalAlignment.Center;
            leftstyle.Alignment = HorizontalAlignment.Left;
            leftstyle.SetFont(rowFont);

            ICellStyle rightstyle = workbook.CreateCellStyle();
            rightstyle.BorderBottom = BorderStyle.Thin;
            rightstyle.BorderLeft = BorderStyle.Thin;
            rightstyle.BorderRight = BorderStyle.Thin;
            rightstyle.BorderTop = BorderStyle.Thin;
            rightstyle.BottomBorderColor = HSSFColor.Black.Index;
            rightstyle.LeftBorderColor = HSSFColor.Black.Index;
            rightstyle.RightBorderColor = HSSFColor.Black.Index;
            rightstyle.TopBorderColor = HSSFColor.Black.Index;
            rightstyle.VerticalAlignment = VerticalAlignment.Center;
            rightstyle.Alignment = HorizontalAlignment.Right;
            rightstyle.SetFont(rowFont);

            ICellStyle centerstyle = workbook.CreateCellStyle();
            centerstyle.BorderBottom = BorderStyle.Thin;
            centerstyle.BorderLeft = BorderStyle.Thin;
            centerstyle.BorderRight = BorderStyle.Thin;
            centerstyle.BorderTop = BorderStyle.Thin;
            centerstyle.BottomBorderColor = HSSFColor.Black.Index;
            centerstyle.LeftBorderColor = HSSFColor.Black.Index;
            centerstyle.RightBorderColor = HSSFColor.Black.Index;
            centerstyle.TopBorderColor = HSSFColor.Black.Index;
            centerstyle.VerticalAlignment = VerticalAlignment.Center;
            centerstyle.Alignment = HorizontalAlignment.Center;
            centerstyle.SetFont(rowFont);

            #endregion

            int currentRowIndex = 1;

            foreach (var item in list)
            {
                sheet = workbook.CreateSheet(item.SheetName);
                sheet.DisplayGridlines = false;
                sheet.DefaultRowHeight = 20 * 20;

                #region Title
                //title
                IRow titleRow = sheet.CreateRow(currentRowIndex);
                ICell titleCell = titleRow.CreateCell(0, CellType.String);
                titleCell.CellStyle = ReportHeaderStyle;
                titleCell.SetCellValue(item.Title);
                sheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 0, item.Column.Count - 1));

                #endregion

                #region 条件 

                IRow conditonRow = null;
                ICell condtionCell = null;

                for (int i = 0; i < item.Condition.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        currentRowIndex += i / 2 + 1;
                        conditonRow = sheet.CreateRow(currentRowIndex);
                        condtionCell = conditonRow.CreateCell(0, CellType.String);
                        condtionCell.SetCellValue(item.Condition[i]);
                        condtionCell.CellStyle = ConditionStyle;
                        sheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 0, (item.Column.Count) / 2 - 1));
                    }
                    else
                    {
                        condtionCell = conditonRow.CreateCell((item.Column.Count) / 2, CellType.String);
                        condtionCell.SetCellValue(item.Condition[i]);
                        condtionCell.CellStyle = rConditionStyle;
                        sheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, (item.Column.Count) / 2, item.Column.Count - 1));
                    }
                }

                #endregion

                #region Column Header 

                currentRowIndex += 1;

                IRow headerRow = sheet.CreateRow(currentRowIndex);

                // colunm header. 
                for (int i = 0; i < item.Column.Count; i++)
                {

                    HSSFCell newCell = headerRow.CreateCell(i) as HSSFCell;
                    newCell.SetCellValue(item.Column[i].HeaderText);
                    switch (item.Column[i].ColumnAlignment)
                    {
                        case ColumnAlignments.Left:
                            newCell.CellStyle = leftstyle;
                            break;
                        case ColumnAlignments.Centert:
                            newCell.CellStyle = centerstyle;
                            break;
                        case ColumnAlignments.Right:
                            newCell.CellStyle = rightstyle;
                            break;
                        default:
                            newCell.CellStyle = leftstyle;
                            break;
                    }
                    sheet.SetColumnWidth(i, (int)((item.Column[i].Width / 7.5 + 0.72) * 256));
                }

                currentRowIndex++;

                #endregion

                #region Table Content

                foreach (DataRow row in item.ContentTable.Rows)
                {
                    IRow dataRow = sheet.CreateRow(currentRowIndex);
                    currentRowIndex++;

                    for (int i = 0; i < item.Column.Count; i++)
                    {
                        HSSFCell newCell = dataRow.CreateCell(i) as HSSFCell;
                        switch (item.Column[i].ColumnAlignment)
                        {
                            case ColumnAlignments.Left:
                                newCell.CellStyle = leftstyle;
                                break;
                            case ColumnAlignments.Centert:
                                newCell.CellStyle = centerstyle;
                                break;
                            case ColumnAlignments.Right:
                                newCell.CellStyle = rightstyle;
                                break;
                            default:
                                newCell.CellStyle = leftstyle;
                                break;
                        }

                        string drValue = row[i].ToString();
                        int v = 0;
                        int.TryParse(drValue, out v);
                        if (drValue == "0")
                        {
                            if (v == 0)
                            {
                                newCell.SetCellValue("");
                            }
                            else
                            {
                                newCell.SetCellValue(v);
                            }
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                    }
                }

                #endregion
            }


            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;


            return ms;
        }

        /// <summary>
        /// 多个DataTable转换成Excel（多个Sheet）文档流
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static MemoryStream ReanderToExcel(Dictionary<string, DataTable> dic, string createtime)
        {
            MemoryStream ms = new MemoryStream();

            IWorkbook workbook = new HSSFWorkbook();

            ISheet sheet0 = workbook.CreateSheet("SFD");
            IRow sRow = sheet0.CreateRow(0);
            ICell sCell = sRow.CreateCell(0);
            sCell.SetCellValue(createtime);

            int i = 1;
            foreach (var item in dic)
            {
                sheet0.CreateRow(i).CreateCell(0).SetCellValue(item.Key);

                ISheet sheet = workbook.CreateSheet(item.Key);

                IRow headerRow = sheet.CreateRow(0);

                // handling header.
                foreach (DataColumn column in item.Value.Columns)
                    headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value

                // handling value.
                int rowIndex = 1;

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

                i++;
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;

            return ms;
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
