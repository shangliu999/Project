using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Utilities.PrintBase
{
    public class PrintAttachment
    {
        #region 公共属性

        /// <summary>
        /// 当前打印任务需要打印的行数
        /// </summary>
        internal int RowCount
        {
            get;
            set;
        }

        /// <summary>
        /// 一页纸可以打印的行数
        /// </summary>
        internal int PageSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前打印页
        /// </summary>
        internal int PageCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前打印的页码
        /// </summary>
        internal int CurrentPageIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 需要进行打印的数据表
        /// </summary>
        public DataTable PrintDataTable
        {
            get;
            set;
        }

        /// <summary>
        /// 当前单据的标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// 打印时间
        /// </summary>
        public string PrintTime
        {
            get;
            set;
        }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string DocumentNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 制单人
        /// </summary>
        public string HandlerName
        {
            get;
            set;
        }
        /// <summary>
        /// 科室名称
        /// </summary>
        public string RegionName
        {
            get;
            set;
        }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName
        {
            get;
            set;
        }

        /// <summary>
        /// english 名称
        /// </summary>
        public string UsingEnglishName
        {
            get;
            set;
        }

        /// <summary>
        /// 总计
        /// </summary>
        public int Total
        {
            get;
            set;
        }

        /// <summary>
        /// 当前打印的数据行索引
        /// </summary>
        public int CurrentDataIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 洗涤公司名称
        /// </summary>
        public string CompanyName
        {
            get;
            set;
        }

        /// <summary>
        /// 包号
        /// </summary>
        public List<string> BagCodes
        {
            get;
            set;
        }

        /// <summary>
        /// 打印类型 0 热塑打印 1 热敏打印
        /// </summary>
        public int PrintType { get; set; }

        /// <summary>
        /// 表格列头
        /// </summary>
        public List<TableColumnHeaderModel> TableColumns { get; set; }

        /// <summary>
        /// 纸张类型 1 宽度58mm  2 宽度80mm
        /// </summary>
        public int PaperType { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

        public Image PrintLogoImage { get; set; }

        public Image FirstQRImage { get; set; }

        public Image SecondQRImage { get; set; }

        public Image ThirdQRImage { get; set; }

        /// <summary>
        /// 包号
        /// </summary>
        public List<string> Trucks
        {
            get;
            set;
        }

        #endregion

        public PrintAttachment()
        {
            this.PageCount = 1;
            this.EOF = this.CurrentPageIndex > this.PageCount - 1;
        }


        /// <summary>
        /// 设置当前一页纸需要打印的数据
        /// </summary>
        private void SetCurrentPageData()
        {
            if (!EOF)
            {
                this.CurrentPageData = new List<DataRow>();
                for (int i = this.CurrentDataIndex; i < this.PageSize * (this.CurrentPageIndex + 1) && i < this.RowCount; i++)
                {
                    this.CurrentPageData.Add(this.PrintDataTable.Rows[i]);
                    continue;
                }
            }
        }

        /// <summary>
        /// 当前打印页中所需打印的数据
        /// </summary>
        public List<DataRow> CurrentPageData
        {
            get;
            set;
        }

        /// <summary>
        /// 是否已打印完毕
        /// </summary>
        public bool EOF
        {
            get;
            private set;
        }

        /// <summary>
        /// 打印下一页
        /// </summary>
        public void NextPage()
        {
            EOF = true; ;
            if (EOF)
            {
                return;
            }
            else
            {
                this.CurrentPageIndex++;
            }
        }
    }

    public class TableColumnHeaderModel
    {
        public string Name { get; set; }

        public int Width { get; set; }

        public ColumnHAlignment Alignment { get; set; }
    }

    public enum ColumnHAlignment
    {
        Left,
        Right,
        Center
    }
}
