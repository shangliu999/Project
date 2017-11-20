using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.Common.ExcelHelp
{
    public enum ColumnAlignments
    {
        Left,
        Right,
        Centert
    }

    public class ExcelTableColumnModel
    {
        public string HeaderText { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public ColumnAlignments ColumnAlignment { get; set; }
    }

    public class ExcelTableModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 条件
        /// </summary>
        public List<string> Condition { get; set; }

        /// <summary>
        /// 表格
        /// </summary>
        public DataTable ContentTable { get; set; }

        /// <summary>
        /// Sheet名称
        /// </summary>
        public string SheetName { get; set; }

        public List<ExcelTableColumnModel> Column { get; set; }
    }
}
