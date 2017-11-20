using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETexsys.WashingCabinet.Utilities
{
    public class ComUitilities
    {
        /// <summary>
        /// 开门
        /// </summary>
        /// <returns></returns>
        public bool OpenDoor()
        {
            bool rtn = false;

            if (App.sp.IsOpen)
            {
                string cmd = "8A 01 01 11 9B";

                byte[] send = ToHexByte(cmd);

                string result = WritePort(App.sp, send, 0, send.Length);

                if (result == "8A-01-01-00-8A")
                {
                    rtn = true;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 获取当前门的状态
        /// </summary>
        /// <returns>0 无法获取 1 门关  2 门开</returns>
        public int GetDoorState()
        {
            int rtn = 0;
            if (App.sp.IsOpen)
            {
                string cmd = "80 01 01 33 B3";

                byte[] send = ToHexByte(cmd);

                string result = WritePort(App.sp, send, 0, send.Length);

                if (result == "80-01-01-11-91")
                {
                    rtn = 1;
                }
                else if (result == "80-01-01-00-80")
                {
                    rtn = 2;
                }
            }
            return rtn;
        }

        private byte[] ToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
            {
                hexString += " ";
            }
            byte[] b = new byte[hexString.Length / 2];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return b;
        }

        /// <summary>
        /// 向串口写入数据
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="send"></param>
        /// <param name="offSet"></param>
        /// <param name="count"></param>
        private string WritePort(SerialPort sp, byte[] send, int offSet, int count)
        {
            string result = "";

            if (sp.IsOpen)
            {
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();

                sp.Write(send, offSet, count);

                //sp.DiscardOutBuffer();
                //sp.DiscardInBuffer();

                //Thread.Sleep(100);

                while (sp.BytesToRead == 0)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(100);

                int readlength = sp.BytesToRead;
                byte[] response = new byte[readlength];
                sp.Read(response, 0, readlength);

                result = BitConverter.ToString(response);

                Console.WriteLine(result);
            }

            return result;
        }
    }
}
