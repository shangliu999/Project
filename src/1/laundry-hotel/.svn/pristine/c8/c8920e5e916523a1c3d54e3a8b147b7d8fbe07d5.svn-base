using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Utilities.PrintBase
{
    public class PrintQueue
    {
        public List<PrintAttachment> PrintAttachments
        {
            get;
            set;
        }

        private PrintDocument printDoc;

        /// <summary>
        /// 是否保存生成的xps
        /// </summary>
        public bool isSave = false;

        private int printPageCount;
        private int currentPageCount;

        public PrintQueue()
        {
            this.PrintAttachments = new List<PrintAttachment>();
            this.printDoc = new PrintDocument();
            this.printDoc.PrintPage += new PrintPageEventHandler(printDoc_PrintPage);
            this.CurrentAttachmentIndex = 0;
            this.printPageCount = 0;
            this.currentPageCount = 0;
        }

        void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            isSave = true;
            if (!this.EOF)
            {
                this.currentPageCount++;
                if (this.currentPageCount > this.printPageCount - 1)
                    e.HasMorePages = false;
                else
                    e.HasMorePages = true;

                PrintAttachment att = this.PrintAttachments[this.CurrentAttachmentIndex];
                if (!att.EOF)
                {
                    Graphics g = e.Graphics;
                    PrintTools.DrawBill(att, g);
                    att.NextPage();
                    if (att.EOF)
                    {
                        this.NextAttachment();
                    }
                }
                else
                {
                    this.NextAttachment();
                    if (this.PrintAttachments[this.CurrentAttachmentIndex] == null)
                    {
                        e.HasMorePages = false;
                    }
                }


            }
            else
            {
                e.HasMorePages = false;
                //e.Cancel = true;
            }
        }

        public void Add(PrintAttachment att)
        {
            this.PrintAttachments.Add(att);
            this.printPageCount += att.PageCount;
        }


        public void Print()
        {
            if (this.PrintAttachments.Count > 0)
            {
                this.printDoc.Print();
                this.currentPageCount = 0;
            }
        }


        internal int CurrentAttachmentIndex
        {
            get;
            set;
        }

        internal int AttachmentCount
        {
            get
            {
                return this.PrintAttachments.Count;
            }
        }

        internal bool EOF
        {
            get;
            private set;
        }

        private void NextAttachment()
        {
            this.EOF = this.CurrentAttachmentIndex >= this.AttachmentCount - 1;
            if (this.EOF)
            {
                return;
            }
            else
            {
                this.CurrentAttachmentIndex++;
            }
        }
    }
}
