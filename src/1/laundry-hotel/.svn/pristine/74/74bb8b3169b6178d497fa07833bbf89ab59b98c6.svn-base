using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Utilities
{
    public class ComUitilities
    {
        public SerialPort Init(string com)
        {
            SerialPort sp = null;
            try
            {
                sp = new SerialPort();
                sp.PortName = com;
                sp.ReadBufferSize = 1024;
                sp.WriteBufferSize = 1024;
                sp.BaudRate = 9600;
                sp.DataBits = 8;
                sp.StopBits = System.IO.Ports.StopBits.One;
                sp.Parity = System.IO.Ports.Parity.None;
                sp.WriteTimeout = -1;
                sp.ReadTimeout = -1;
                sp.Handshake = Handshake.None;
                sp.RtsEnable = false;
                sp.DtrEnable = false;
            }
            catch { }
            return sp;
        }

        public bool TurnAntenna(SerialPort sp)
        {
            bool rtn = false;

            if (sp != null)
            {
                try
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                    }

                    byte[] send = new byte[] { 0x01 };

                    WritePort(sp, send, 0, send.Length);

                    Thread.Sleep(2000);

                    send = new byte[] { 0xF1 };

                    WritePort(sp, send, 0, send.Length);

                    sp.Close();

                    rtn = true;
                }
                catch { }
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
