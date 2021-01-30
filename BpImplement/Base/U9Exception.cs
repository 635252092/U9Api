using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using U9Api.CustSV.Utils;

namespace U9Api.CustSV.Base
{
   public class U9Exception
    {
       public string U9ERROR = "U9ERROR";
        public static Exception GetInnerException(Exception ex) {

            Exception res = ex.InnerException?? ex;
            LogUtil.WriteExceptionLog(res.ToString());
            //return res;
            return new Exception(res.Message);
        }
        public static string GetInnerExceptionMsg(Exception ex)
        {

            Exception res = ex.InnerException ?? ex;
            LogUtil.WriteExceptionLog(res.ToString());
            string msg = res.Message;
            if (!string.IsNullOrEmpty(msg))
            {
                if (msg.Contains("。 <a href=\"#\""))
                {
                    msg = msg.Substring(0, msg.IndexOf("。 <a href=\"#\""));
                }
                msg.Trim();
            }
            return msg;
        }
        public static Exception GetException(string msg, StringBuilder sb=null)
        {
            if (sb != null)
                LogUtil.WriteDebugInfoLog(sb.ToString());
            return new Exception(msg);
        }
    }
}
