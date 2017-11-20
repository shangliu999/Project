﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Utilities.PrintBase
{
    public class PrintTools
    {
        #region --- 常量及属性 ---

        /// <summary>
        /// 数据显示表格宽度
        /// </summary>
        internal static int TableTotalWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// 数据显示表格高度
        /// </summary>
        internal static int TableTotalHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 数据显示表格头部高度
        /// </summary>
        internal static int TableHeaderHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 数据显示表格头部宽度
        /// </summary>
        internal static int TableHeaderWidth
        {
            get;
            private set;
        }


        /// <summary>
        /// 数据显示数据行高度
        /// </summary>
        internal static int TableColumnHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 数据显示数据行宽度
        /// </summary>
        internal static int TableColumnWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// 页头高度
        /// </summary>
        internal static int PageHeaderHeight
        {
            get;
            set;
        }

        /// <summary>
        /// 两数据表之间的间隔距离
        /// </summary>
        internal static int TablePadding
        {
            get;
            set;
        }

        /// <summary>
        /// 当前打印中需要绘制的数据表格个数
        /// </summary>
        /// <returns></returns>
        internal static int TableCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 顶部预留高度
        /// </summary>
        internal static int MaginTop
        { get; private set; }

        /// <summary>
        /// 左侧预留宽度
        /// </summary>
        internal static int MaginLeft
        {
            get;
            private set;
        }

        /// <summary>
        /// 单据头部高度
        /// </summary>
        internal static int HeaderHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 本页中的绘图原点
        /// </summary>
        internal static Point OrignalPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// 绘制的Logo尺寸
        /// </summary>
        internal static Size LogoSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Logo图片
        /// </summary>
        internal static Image Logo
        {
            get;
            private set;
        }

        /// <summary>
        /// 文本字体
        /// </summary>
        internal static Font ContentFont
        {
            get;
            private set;
        }

        /// <summary>
        /// 数字字体
        /// </summary>
        internal static Font ContentNumberFont
        {
            get;
            private set;
        }

        /// <summary>
        /// 标题字体
        /// </summary>
        internal static Font TitleFont
        {
            get;
            private set;
        }

        internal static Font ColumnHeaderFont
        {
            get;
            private set;
        }

        internal static Font TotalFont
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前文档中打印的位置高度
        /// </summary>
        internal static int CurrentHeight
        {
            get;
            private set;
        }

        #endregion

        #region --- InitTools ---

        private static void InitTools(PrintAttachment ph)
        {
            if (ph.PaperType == 1)
            {
                TableCount = 2;
                TablePadding = 0;
                TableTotalWidth = 190;
                TableHeaderHeight = 20;
                //获取数据表格的列头宽度
                TableHeaderWidth = 190;
                //获取表格数据行高度
                TableColumnHeight = 18;

                ContentNumberFont = new Font("微软雅黑", 9, FontStyle.Regular, GraphicsUnit.Point);
                //内容字体
                ContentFont = new Font("微软雅黑", 10, FontStyle.Regular, GraphicsUnit.Point);
                //标题字体
                TitleFont = new Font("微软雅黑", 16, FontStyle.Bold, GraphicsUnit.Point);
                //列头文本字体
                ColumnHeaderFont = new Font("微软雅黑", 10, FontStyle.Regular, GraphicsUnit.Point);
                //合计字体
                TotalFont = new Font("微软雅黑", 13, FontStyle.Regular, GraphicsUnit.Point);

                MaginLeft = 2;
                MaginTop = 1;
                OrignalPoint = new Point(MaginLeft, MaginTop);
                CurrentHeight = OrignalPoint.Y;
            }
            else if (ph.PaperType == 2)
            {
                TableCount = 2;
                TablePadding = 6;
                TableTotalWidth = 280;
                TableHeaderHeight = 20;
                //获取数据表格的列头宽度
                TableHeaderWidth = 280;
                //获取表格数据行高度
                TableColumnHeight = 18;

                ContentNumberFont = new Font("微软雅黑", 9, FontStyle.Regular, GraphicsUnit.Point);
                //内容字体
                ContentFont = new Font("微软雅黑", 10, FontStyle.Regular, GraphicsUnit.Point);
                //标题字体
                TitleFont = new Font("微软雅黑", 16, FontStyle.Bold, GraphicsUnit.Point);
                //列头文本字体
                ColumnHeaderFont = new Font("微软雅黑", 10, FontStyle.Regular, GraphicsUnit.Point);
                //合计字体
                TotalFont = new Font("微软雅黑", 13, FontStyle.Regular, GraphicsUnit.Point);

                MaginLeft = 2;
                MaginTop = 1;
                OrignalPoint = new Point(MaginLeft, MaginTop);
                CurrentHeight = OrignalPoint.Y;
            }
            else
            {
                TableHeaderHeight = 24;
                //获取数据表格的列头宽度
                TableHeaderWidth = 280;
                //获取表格数据行高度
                TableColumnHeight = 24;
                TableTotalWidth = 296;
                ContentNumberFont = new Font("ArialMT", 9, FontStyle.Regular, GraphicsUnit.Point);
                //内容字体
                ContentFont = new Font("微软雅黑", 9, FontStyle.Regular, GraphicsUnit.Point);
                //标题字体
                TitleFont = new Font("微软雅黑", 12, FontStyle.Regular, GraphicsUnit.Point);

                //列头文本字体
                ColumnHeaderFont = new Font("微软雅黑", 10, FontStyle.Bold, GraphicsUnit.Point);
                //合计字体
                TotalFont = new Font("微软雅黑", 10, FontStyle.Bold, GraphicsUnit.Point);

                MaginLeft = 28;
                MaginTop = 22;
                OrignalPoint = new Point(MaginLeft, MaginTop);
                CurrentHeight = OrignalPoint.Y;
            }

        }

        #endregion

        #region Print 

        public static void DrawBill(PrintAttachment ph, Graphics g)
        {
            if (ph.PrintType == 0)//热塑打印
            {
                Rectangle destRect = new Rectangle(7, 7, 35, 35);

                g.DrawImage(ph.FirstQRImage, destRect);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.ResetTransform();

                //g.DrawLine(Pens.Black, new Point(46, 1), new Point(46, 48));

                if (ph.SecondQRImage != null)
                {
                    destRect = new Rectangle(50, 7, 35, 35);
                    g.DrawImage(ph.SecondQRImage, destRect);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.ResetTransform();

                    //g.DrawLine(Pens.Black, new Point(89, 1), new Point(89, 48));
                }

                if (ph.ThirdQRImage != null)
                {
                    destRect = new Rectangle(93, 7, 35, 35);
                    g.DrawImage(ph.ThirdQRImage, destRect);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.ResetTransform();

                }
                //g.DrawLine(Pens.Black, new Point(0, 48), new Point(132, 48));
            }
            else//热敏打印
            {
                InitTools(ph);
                if (ph.PaperType == 1 || ph.PaperType == 2)
                {
                    DrawHeader(ph, ref g);
                    DrawColumnHeader(ph, ref g);
                    DrawTable(ph, ref g);
                    DrawTotal(ph, ref g);
                    DrawBag(ph, ref g);
                    DrawTruck(ph, ref g);
                    DrawFooter(ph, ref g);
                }
                else
                {
                    DrawHeader_240_140(ph, ref g);
                    DrawColumnHeader_240_140(ph, ref g);
                    DrawTable_240_140(ph, ref g);
                    DrawBag_240_140(ph, ref g);
                    DrawTotal_240_140(ph, ref g);
                    DrawFooter_240_140(ph, ref g);
                }
            }
        }

        #region 240*140mm尺寸

        private static void DrawHeader_240_140(PrintAttachment ph, ref Graphics g)
        {
            Point p = new Point();

            string txt = string.Empty;
            if (ph.CustomerName != null && ph.CustomerName != "")
            {
                txt = ph.CustomerName;
                if (ph.RegionName != null && ph.RegionName != "")
                {
                    txt = string.Format("{0}/{1}", txt, ph.RegionName);
                }
            }
            else
            {
                if (ph.RegionName != "")
                {
                    txt = ph.RegionName;
                }
            }
            Font f = new Font("微软雅黑", 15, FontStyle.Regular, GraphicsUnit.Point);
            p.Y = OrignalPoint.Y;
            p.X = (624 - GetStringWidth(txt, f, g)) / 2 + OrignalPoint.X;

            DrawString(p, txt, f, ref g);
            CurrentHeight = p.Y + Convert.ToInt32(GetStringSize(txt, f, g).Height);

            p.X = (624 - GetStringWidth(txt, f, g)) / 2 + OrignalPoint.X;
            p.Y = CurrentHeight;

            Point p1 = new Point();
            p1.X = p.X + GetStringWidth(txt, f, g);
            p1.Y = CurrentHeight;

            DrawLine(p, p1, 2, ref g);

            CurrentHeight += 2;

            p = new Point();
            p.X = (624 - GetStringWidth(ph.Title, f, g)) / 2 + OrignalPoint.X;
            p.Y = CurrentHeight;
            CurrentHeight = p.Y + Convert.ToInt32(GetStringSize(txt, f, g).Height);
            DrawString(p, ph.Title, TitleFont, ref g);

            //if (ph.FirstQRImage != null)
            //{
            //    Rectangle destRect = new Rectangle(OrignalPoint.X, CurrentHeight, 100, 100);
            //    g.DrawImage(ph.FirstQRImage, destRect);
            //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //    g.ResetTransform();
            //}

            if (ph.PrintLogoImage != null)
            {
                Rectangle destRect = new Rectangle(624 - 60 + OrignalPoint.X, CurrentHeight, 60, 60);
                g.DrawImage(ph.PrintLogoImage, destRect);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.ResetTransform();
            }

            if (ph.HandlerName != null && ph.HandlerName != "")
            {
                p = new Point();
                p.X = OrignalPoint.X;
                p.Y = CurrentHeight;

                txt = string.Format("制单人：{0}", ph.HandlerName);
                DrawString(p, txt, ColumnHeaderFont, ref g);
                CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);
            }

            p = new Point();
            p.X = OrignalPoint.X;
            p.Y = CurrentHeight;

            txt = string.Format("时间：{0}", ph.PrintTime);
            DrawString(p, txt, ColumnHeaderFont, ref g);
            CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);

            if (ph.DocumentNumber != "" && ph.DocumentNumber != null)
            {
                p = new Point();
                p.X = OrignalPoint.X;
                p.Y = CurrentHeight;

                txt = string.Format("编号：{0}", ph.DocumentNumber);
                DrawString(p, txt, ColumnHeaderFont, ref g);
                CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);
            }

            p = new Point();
            p.X = OrignalPoint.X;
            p.Y = CurrentHeight + 2;

            p1 = new Point();
            p1.X = 624 + OrignalPoint.X;
            p1.Y = CurrentHeight + 2;

            DrawDashLine(p, p1, 1, ref g);

            CurrentHeight += 2;
        }

        private static void DrawColumnHeader_240_140(PrintAttachment ph, ref Graphics g)
        {
            CurrentHeight += 10;
            Point startPoint = new Point();
            startPoint.X = OrignalPoint.X;
            startPoint.Y = CurrentHeight;
            int currentWidth = 0;

            foreach (var item in ph.TableColumns)
            {
                int colwidth = item.Width;
                string headerText = item.Name;

                Point p = new Point();
                p.X = OrignalPoint.X + currentWidth;
                p.Y = startPoint.Y;

                Point textPoint = new Point();
                if (item.Alignment == ColumnHAlignment.Center)
                {
                    textPoint.X = p.X + (colwidth - GetStringWidth(headerText, ColumnHeaderFont, g)) / 2 - 3;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                else if (item.Alignment == ColumnHAlignment.Right)
                {
                    textPoint.X = p.X + colwidth - GetStringWidth(headerText, ColumnHeaderFont, g) - 3;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                else
                {
                    textPoint.X = p.X;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                DrawString(textPoint, headerText, ColumnHeaderFont, ref g);
                currentWidth += colwidth;
            }



            currentWidth = 328 + OrignalPoint.X;

            foreach (var item in ph.TableColumns)
            {
                int colwidth = item.Width;
                string headerText = item.Name;

                Point p = new Point();
                p.X = currentWidth;
                p.Y = startPoint.Y;

                Point textPoint = new Point();
                if (item.Alignment == ColumnHAlignment.Center)
                {
                    textPoint.X = p.X + (colwidth - GetStringWidth(headerText, ColumnHeaderFont, g)) / 2 - 3;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                else if (item.Alignment == ColumnHAlignment.Right)
                {
                    textPoint.X = p.X + colwidth - GetStringWidth(headerText, ColumnHeaderFont, g) - 3;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                else
                {
                    textPoint.X = p.X;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                DrawString(textPoint, headerText, ColumnHeaderFont, ref g);
                currentWidth += colwidth;
            }

            CurrentHeight += TableHeaderHeight;

            Point p1 = new Point();
            p1.X = OrignalPoint.X;
            p1.Y = CurrentHeight;

            Point p2 = new Point();
            p2.X = 296 + OrignalPoint.X;
            p2.Y = CurrentHeight;

            DrawLine(p1, p2, 2, ref g);

            p1 = new Point();
            p1.X = OrignalPoint.X + 296 + 32;
            p1.Y = CurrentHeight;

            p2 = new Point();
            p2.X = 296 + 32 + 296 + OrignalPoint.X;
            p2.Y = CurrentHeight;

            DrawLine(p1, p2, 2, ref g);
        }

        private static void DrawTable_240_140(PrintAttachment ph, ref Graphics g)
        {
            int h = CurrentHeight;
            for (int i = 0; i < ph.PrintDataTable.Rows.Count; i++)
            {

                Point startPoint = new Point();
                startPoint.X = i > 6 ? 296 + 32 + OrignalPoint.X : OrignalPoint.X;
                startPoint.Y = h;
                int height = TableColumnHeight;
                int currentWidth = 0;
                int lineHeight = 0;

                if (i == 6)
                {
                    h = CurrentHeight;
                    startPoint.X = 296 + 32 + OrignalPoint.X;
                    startPoint.Y = h;
                }

                for (int j = 0; j < ph.TableColumns.Count; j++)
                {
                    int colWidth = ph.TableColumns[j].Width;
                    string txt = ph.PrintDataTable.Rows[i][j].ToString();
                    Point p = new Point();

                    p.Y = startPoint.Y + lineHeight + 1;

                    if (ph.TableColumns[j].Alignment == ColumnHAlignment.Center)
                    {
                        p.X = startPoint.X + currentWidth + (colWidth - GetStringWidth(txt, ContentFont, g)) / 2 - 3;
                    }
                    else if (ph.TableColumns[j].Alignment == ColumnHAlignment.Right)
                    {
                        p.X = startPoint.X + currentWidth + colWidth - GetStringWidth(txt, ContentFont, g) - 3;
                    }
                    else
                    {
                        p.X = startPoint.X + currentWidth;
                    }

                    DrawString(p, txt, ContentFont, ref g);
                    currentWidth += colWidth;
                }

                lineHeight += TableColumnHeight;
                h += height;
            }

            CurrentHeight += 6 * TableColumnHeight;

            CurrentHeight += 5;

            Point p1 = new Point();
            p1.X = OrignalPoint.X;
            p1.Y = CurrentHeight;

            Point p2 = new Point();
            p2.X = 296 + OrignalPoint.X;
            p2.Y = CurrentHeight;

            DrawLine(p1, p2, 2, ref g);

            p1 = new Point();
            p1.X = OrignalPoint.X + 296 + 32;
            p1.Y = CurrentHeight;

            p2 = new Point();
            p2.X = 296 + 32 + 296 + OrignalPoint.X;
            p2.Y = CurrentHeight;

            DrawLine(p1, p2, 2, ref g);
        }

        private static void DrawBag_240_140(PrintAttachment ph, ref Graphics g)
        {
            CurrentHeight += 10;

            Point p = new Point();
            p.X = OrignalPoint.X;
            p.Y = CurrentHeight;

            Font f = new Font("微软雅黑", 10, FontStyle.Regular, GraphicsUnit.Point);
            p.X = OrignalPoint.X; ;
            DrawString(p, "包号：", f, ref g);


            if (ph.BagCodes != null && ph.BagCodes.Count > 0)
            {
                int w = OrignalPoint.X + Convert.ToInt32(GetStringSize("包号：", f, g).Width);
                Point p1;
                int count = 0;
                for (int i = 0; i < ph.BagCodes.Count; i++)
                {
                    int avg = i % 13;
                    if (avg == 0 && i != 0)
                    {
                        count++;
                        w = OrignalPoint.X;
                        p1 = new Point();
                        p1.X = w + 2;
                        p1.Y = CurrentHeight + count * 20;
                        DrawString(p1, ph.BagCodes[i], f, ref g);

                        w += Convert.ToInt32(GetStringSize(ph.BagCodes[i], ColumnHeaderFont, g).Width);
                    }
                    else
                    {
                        p1 = new Point();
                        p1.X = w + 2;
                        p1.Y = CurrentHeight + count * 20;
                        DrawString(p1, ph.BagCodes[i], f, ref g);

                        w += Convert.ToInt32(GetStringSize(ph.BagCodes[i], ColumnHeaderFont, g).Width);
                    }
                }

                CurrentHeight += count * 20;
            }

            //Point p3 = new Point();
            //p3.X = OrignalPoint.X;
            //p3.Y = p.Y + 30;

            //Point p2 = new Point();
            //p2.X = 200 + OrignalPoint.X;
            //p2.Y = p.Y + 30;

            //DrawLine(p3, p2, 2, ref g);
        }

        private static void DrawTotal_240_140(PrintAttachment ph, ref Graphics g)
        {
            CurrentHeight += 25;

            Point p = new Point();
            p.X = OrignalPoint.X;
            p.Y = CurrentHeight;

            DrawString(p, "合计:", TotalFont, ref g);

            Point p5 = new Point();
            p5.X = OrignalPoint.X + GetStringWidth("合计:", TotalFont, g);
            p5.Y = p.Y;

            DrawString(p5, ph.Total.ToString(), TotalFont, ref g);

            Point p1 = new Point();
            p1.X = OrignalPoint.X;
            p1.Y = p.Y + 20;

            Point p2 = new Point();
            p2.X = 200 + OrignalPoint.X;
            p2.Y = p.Y + 20;

            DrawLine(p1, p2, 2, ref g);
        }

        private static void DrawFooter_240_140(PrintAttachment ph, ref Graphics g)
        {
            Point p = new Point();
            p.X = OrignalPoint.X + 400;
            p.Y = CurrentHeight;

            Font f = new Font("微软雅黑", 10, FontStyle.Bold, GraphicsUnit.Point);
            DrawString(p, "签名：", f, ref g);

        }
        #endregion

        private static void DrawHeader(PrintAttachment ph, ref Graphics g)
        {
            Point p = new Point();
            p.X = OrignalPoint.X + (TableTotalWidth - GetStringWidth(ph.Title, TitleFont, g)) / 2;
            p.Y = OrignalPoint.Y;

            DrawString(p, ph.Title, TitleFont, ref g);
            CurrentHeight = p.Y + Convert.ToInt32(GetStringSize(ph.Title, TitleFont, g).Height);

            p = new Point();
            p.X = OrignalPoint.X;
            p.Y = OrignalPoint.Y + CurrentHeight;

            Point p1 = new Point();
            p1.X = p.X + TableTotalWidth;
            p1.Y = p.Y;
            CurrentHeight += 12;

            DrawDashLine(p, p1, 1, ref g);

            string txt = string.Empty;
            p = new Point();
            p.X = OrignalPoint.X + 2;
            p.Y = OrignalPoint.Y + CurrentHeight;

            if (ph.CustomerName != null && ph.CustomerName != "")
            {
                txt = ph.CustomerName;
                if (ph.RegionName != null && ph.RegionName != "")
                {
                    txt = string.Format("{0}/{1}", txt, ph.RegionName);
                }

                DrawString(p, txt, ColumnHeaderFont, ref g);
                CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);
            }
            else
            {
                if (ph.RegionName != "")
                {
                    txt = ph.RegionName;
                }
                DrawString(p, txt, ColumnHeaderFont, ref g);
                CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);
            }

            if (ph.HandlerName != null && ph.HandlerName != "")
            {
                p = new Point();
                p.X = OrignalPoint.X + 2;
                p.Y = OrignalPoint.Y + CurrentHeight;

                txt = string.Format("制单人：{0}", ph.HandlerName);
                DrawString(p, txt, ColumnHeaderFont, ref g);
                CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);
            }

            p = new Point();
            p.X = OrignalPoint.X + 2;
            p.Y = OrignalPoint.Y + CurrentHeight;

            txt = string.Format("时间：{0}", ph.PrintTime);
            DrawString(p, txt, ColumnHeaderFont, ref g);
            CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);

            if (ph.DocumentNumber != "" && ph.DocumentNumber != null)
            {
                p = new Point();
                p.X = OrignalPoint.X + 2;
                p.Y = OrignalPoint.Y + CurrentHeight;

                txt = string.Format("编号：{0}", ph.DocumentNumber);
                DrawString(p, txt, ColumnHeaderFont, ref g);
                CurrentHeight += Convert.ToInt32(GetStringSize(txt, ColumnHeaderFont, g).Height);
            }

            p = new Point();
            p.X = OrignalPoint.X;
            p.Y = OrignalPoint.Y + CurrentHeight;

            p1 = new Point();
            p1.X = p.X + TableTotalWidth;
            p1.Y = p.Y;
            CurrentHeight += 12;

            DrawDashLine(p, p1, 1, ref g);
        }

        private static void DrawColumnHeader(PrintAttachment ph, ref Graphics g)
        {
            Point startPoint = new Point();
            startPoint.X = OrignalPoint.X;
            startPoint.Y = OrignalPoint.Y + CurrentHeight;
            int currentWidth = 0;

            foreach (var item in ph.TableColumns)
            {
                int colwidth = item.Width;
                string headerText = item.Name;

                Point p = new Point();
                p.X = OrignalPoint.X + currentWidth;
                p.Y = startPoint.Y;

                Point textPoint = new Point();
                if (item.Alignment == ColumnHAlignment.Center)
                {
                    textPoint.X = p.X + (colwidth - GetStringWidth(headerText, ColumnHeaderFont, g)) / 2 - 3;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                else if (item.Alignment == ColumnHAlignment.Right)
                {
                    textPoint.X = p.X + colwidth - GetStringWidth(headerText, ColumnHeaderFont, g) - 3;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                else
                {
                    textPoint.X = p.X;
                    textPoint.Y = p.Y + (TableHeaderHeight - Convert.ToInt32(GetStringSize(headerText, ColumnHeaderFont, g).Height)) / 2;
                }
                DrawString(textPoint, headerText, ColumnHeaderFont, ref g);
                currentWidth += colwidth;
            }

            CurrentHeight += TableHeaderHeight;
        }

        private static void DrawTable(PrintAttachment ph, ref Graphics g)
        {
            for (int i = 0; i < ph.PrintDataTable.Rows.Count; i++)
            {
                Point startPoint = new Point();
                startPoint.X = OrignalPoint.X;
                startPoint.Y = OrignalPoint.Y + CurrentHeight;
                int height = TableColumnHeight;
                int currentWidth = 0;
                int lineHeight = 0;

                for (int j = 0; j < ph.TableColumns.Count; j++)
                {
                    int colWidth = ph.TableColumns[j].Width;
                    string txt = ph.PrintDataTable.Rows[i][j].ToString();
                    Point p = new Point();

                    p.Y = startPoint.Y + lineHeight + 1;

                    if (ph.TableColumns[j].Alignment == ColumnHAlignment.Center)
                    {
                        p.X = startPoint.X + currentWidth + (colWidth - GetStringWidth(txt, ContentFont, g)) / 2 - 3;
                    }
                    else if (ph.TableColumns[j].Alignment == ColumnHAlignment.Right)
                    {
                        p.X = startPoint.X + currentWidth + colWidth - GetStringWidth(txt, ContentFont, g) - 3;
                    }
                    else
                    {
                        p.X = startPoint.X + currentWidth;
                    }

                    DrawString(p, txt, ContentFont, ref g);
                    currentWidth += colWidth;
                }

                lineHeight += TableColumnHeight;
                CurrentHeight += height;
            }

            CurrentHeight += TableColumnHeight;
        }

        private static void DrawTotal(PrintAttachment ph, ref Graphics g)
        {
            Point p = new Point();
            p.X = OrignalPoint.X + TableTotalWidth - (TableTotalWidth / 28) * 16 + 4;
            int width = (TableTotalWidth / 28) * 16;
            p.Y = CurrentHeight;

            Point p1 = new Point();
            p1.X = p.X;
            p1.Y = p.Y + 30;

            Point p2 = new Point();
            p2.X = p1.X + width - 7;
            p2.Y = p1.Y;

            Point p3 = new Point();
            p3.X = p2.X;
            p3.Y = p.Y;

            DrawLine(p1, p2, 2, ref g);//绘制合计下面直线

            p.X += 0;
            p.Y += 5;
            DrawString(p, "合计", TotalFont, ref g);

            Point p5 = new Point();
            p5.X = TableTotalWidth - GetStringWidth(ph.Total.ToString(), TotalFont, g) - 3;
            p5.Y = p.Y;

            DrawString(p5, ph.Total.ToString(), TotalFont, ref g);


            CurrentHeight += 35;
        }

        private static void DrawBag(PrintAttachment ph, ref Graphics g)
        {
            if (ph.BagCodes != null && ph.BagCodes.Count > 0)
            {
                Point p = new Point();
                p.X = OrignalPoint.X;
                p.Y = OrignalPoint.Y + CurrentHeight;

                Font f = new Font("微软雅黑", 9, FontStyle.Regular, GraphicsUnit.Point);
                p.X = OrignalPoint.X; ;
                DrawString(p, "包号：", f, ref g);

                CurrentHeight += Convert.ToInt32(GetStringSize("包号", f, g).Height);

                Point p1;

                int width = Convert.ToInt32(GetStringSize(ph.BagCodes[0], f, g).Width) + 5;
                int count = TableHeaderWidth / width;

                for (int i = 0; i < ph.BagCodes.Count; i++)
                {
                    int avg = i % count;
                    if (avg == 0 && i != 0)
                    {
                        CurrentHeight += Convert.ToInt32(GetStringSize(ph.BagCodes[i], f, g).Height);
                        p1 = new Point();
                        p1.X = OrignalPoint.X + 2;
                        p1.Y = OrignalPoint.Y + CurrentHeight;
                        DrawString(p1, ph.BagCodes[i] + "、", f, ref g);

                    }
                    else
                    {
                        p1 = new Point();
                        p1.X = OrignalPoint.X + 2 + width * avg;
                        p1.Y = OrignalPoint.Y + CurrentHeight;
                        DrawString(p1, ph.BagCodes[i] + "、", f, ref g);
                    }

                }
            }
        }

        private static void DrawTruck(PrintAttachment ph, ref Graphics g)
        {
            if (ph.Trucks != null && ph.Trucks.Count > 0)
            {
                Point p = new Point();
                p.X = OrignalPoint.X;
                p.Y = OrignalPoint.Y + CurrentHeight;

                Font f = new Font("微软雅黑", 9, FontStyle.Regular, GraphicsUnit.Point);
                p.X = OrignalPoint.X; ;
                DrawString(p, "笼车：", f, ref g);

                Point p1;
                for (int i = 0; i < ph.Trucks.Count; i++)
                {
                    p1 = new Point();
                    p1.X = OrignalPoint.X + 2;
                    p1.Y = OrignalPoint.Y + CurrentHeight;
                    DrawString(p1, ph.Trucks[i], f, ref g);

                    CurrentHeight += Convert.ToInt32(GetStringSize(ph.Trucks[i], ColumnHeaderFont, g).Height);
                }
            }
        }

        private static void DrawFooter(PrintAttachment ph, ref Graphics g)
        {
            Point p = new Point();
            p.X = OrignalPoint.X;
            p.Y = OrignalPoint.Y + CurrentHeight + 40;

            Font f = new Font("微软雅黑", 9, FontStyle.Regular, GraphicsUnit.Point);
            p.X = OrignalPoint.X; ;
            DrawString(p, "签名：", f, ref g);

            CurrentHeight += 80;

            p = new Point();
            p.X = OrignalPoint.X + 30;
            p.Y = OrignalPoint.Y + CurrentHeight + 40;
            DrawString(p, ph.CompanyName, f, ref g);
        }

        #endregion

        #region --- DrawPath ---

        /// <summary>
        /// 绘制矩形的方法
        /// </summary>
        /// <param name="p">矩形的顶点</param>
        /// <param name="width">矩形的宽度</param>
        /// <param name="height">矩形的高度</param>
        /// <param name="g">画笔</param>
        private static void DrawRectangle(Point p, int width, int height, ref Graphics g)
        {
            Pen pen = new Pen(new SolidBrush(Color.Black), 1);
            g.DrawRectangle(pen, new Rectangle(p, new Size(width, height)));
            g.Flush();
        }

        /// <summary>
        /// 以特定颜色填充矩形的方法
        /// </summary>
        /// <param name="p">矩形的顶点</param>
        /// <param name="width">矩形的宽度</param>
        /// <param name="height">矩形的高度</param>
        /// <param name="fillColor">填充色</param>
        /// <param name="g">画笔</param>
        private static void FillRectangle(Point p, int width, int height, Color fillColor, ref Graphics g)
        {
            Pen pen = new Pen(new SolidBrush(Color.Black), 1);
            g.DrawRectangle(pen, new Rectangle(p, new Size(width, height)));
            g.FillRectangle(new SolidBrush(fillColor), new Rectangle(p.X + 1, p.Y + 1, width - 1, height - 1));
            g.Flush();
        }


        /// <summary>
        /// 绘制数据表格中内容文本
        /// </summary>
        private static void DrawTableContent(Point p, string content, ref Graphics g)
        {
            SolidBrush brush = new SolidBrush(Color.Black);
            g.DrawString(content, ContentFont, brush, p);
            g.Flush();
        }


        /// <summary>
        /// 绘制字符串
        /// </summary>
        private static void DrawString(Point p, string content, Font font, ref Graphics g)
        {
            SolidBrush brush = new SolidBrush(Color.Black);
            g.DrawString(content, font, brush, p);
            g.Flush();
        }

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="p1">直线开始点</param>
        /// <param name="p2">直线结束点</param>
        /// <param name="width">画笔大小</param>
        /// <param name="g">画笔</param>
        private static void DrawLine(Point p1, Point p2, int width, ref Graphics g)
        {
            Pen pen = new Pen(new SolidBrush(Color.Black), width);
            g.DrawLine(pen, p1, p2);
            g.Flush();
        }

        /// <summary>
        /// 绘制字符串
        /// </summary>
        private static void DrawString(Point p, string content, Font font, Color color, ref Graphics g)
        {
            SolidBrush brush = new SolidBrush(color);
            g.DrawString(content, font, brush, p);
            g.Flush();
        }

        /// <summary>
        /// 绘制实线的方法
        /// </summary>
        /// <param name="p1">线段起点</param>
        /// <param name="p2">线段重点</param>
        /// <param name="width">绘制线段的粗细</param>
        /// <param name="g">画笔</param>
        private static void DrawSolidLine(Point p1, Point p2, int width, ref Graphics g)
        {
            Pen pen = new Pen(new SolidBrush(Color.Black), width);
            g.DrawLine(pen, p1, p2);
            g.Flush();
        }

        /// <summary>
        /// 绘制虚线的方法
        /// </summary>
        /// <param name="p1">线段起始点</param>
        /// <param name="p2">线段结束点</param>
        /// <param name="width">绘制线段的粗细</param>
        /// <param name="g">画笔</param>
        private static void DrawDashLine(Point p1, Point p2, int width, ref Graphics g)
        {
            Pen pen = new Pen(Color.Black, width);
            pen.DashStyle = DashStyle.Dot;
            g.DrawLine(pen, p1, p2);
            g.Flush();
        }

        /// <summary>
        /// 获取一段文本在特定字体下的大小
        /// </summary>
        /// <param name="s">文本</param>
        /// <param name="f">字体</param>
        /// <returns>尺寸</returns>
        private static SizeF GetStringSize(string s, Font f, Graphics g)
        {
            return g.MeasureString(s, f);
        }

        /// <summary>
        ///  获取一段文本在特定字体下的宽度
        /// </summary>
        /// <param name="s">文本</param>
        /// <param name="f">字体</param>
        /// <returns></returns>
        private static int GetStringWidth(string s, Font f, Graphics g)
        {
            SizeF size = g.MeasureString(s, f);
            return Convert.ToInt32(size.Width);
        }

        #endregion
    }
}
