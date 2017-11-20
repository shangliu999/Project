using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.Common.SMS
{
    public static class SMSTools
    {

        public static bool SendSMS(string uid, string key, string moblie, string content)
        {
            bool rtn = false;

            string url = string.Format("http://utf8.api.smschinese.cn/?Uid={0}&key={1}&smsMob={2}&smsText={3}", uid, key, moblie, content);

            string targeturl = url.Trim().ToString();
            try
            {
                HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                hr.Method = "GET";
                hr.Timeout = 30 * 60 * 1000;
                WebResponse hs = hr.GetResponse();
                Stream sr = hs.GetResponseStream();
                StreamReader ser = new StreamReader(sr, Encoding.Default);

                rtn = true;
            }
            catch
            {
            }

            return rtn;
        }
    }
}
