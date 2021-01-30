using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using U9Api.CustSV.Base;
namespace U9Api.CustSV.Utils
{
    public class DateUtil
    {
        public static DateTime GetDateObject(string str)
        {
            return DateTime.Parse(str);
        }
    }
}
