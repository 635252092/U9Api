using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.PL;

namespace U9Api.CustSV.Utils
{
    class SVPublicExtend
    {
        public static long GetSOBAccountPeriod(DateTime DateInfo)
        {
            long SOBID = 0;
            UFIDA.U9.Base.SOB.SOBAccountingPeriod SobAccList = GetSOBAccountPeriodEntity(DateInfo, "");
            if (SobAccList != null) SOBID = SobAccList.ID;
            return SOBID;
        }
        /// <summary>
        /// 获取期间号码
        /// </summary>
        /// <param name="DateInfo"></param>
        /// <param name="SobCode"></param>
        /// <returns></returns>
        public static string GetSOBAccountPeriodByCode(DateTime DateInfo, string SobCode)
        {
            string PeriodCode = DateInfo.ToString("yyyy-MM");
            UFIDA.U9.Base.SOB.SOBAccountingPeriod SobAccList = GetSOBAccountPeriodEntity(DateInfo, SobCode);
            if (SobAccList != null) PeriodCode = SobAccList.DisplayName;
            return PeriodCode;
        }
        private static UFIDA.U9.Base.SOB.SOBAccountingPeriod GetSOBAccountPeriodEntity(DateTime DateInfo, string SOBCode)
        {
            StringBuilder _qStr = new StringBuilder(128);
            _qStr.Append("SetofBooks.Org=" + UFIDA.U9.Base.Context.LoginOrg.ID);
            _qStr.Append(" and @DateInfo between AccountPeriod.FromDate and AccountPeriod.ToDate");
            if (string.IsNullOrEmpty(SOBCode))
                _qStr.Append(" and SetofBooks.SOBType=" + UFIDA.U9.Base.SOB.SOBTypeEnum.PrimarySOB.Value);
            else
                _qStr.Append(" and SetofBooks.Code='" + SOBCode + "'");
            UFIDA.U9.Base.SOB.SOBAccountingPeriod SobAccList = UFIDA.U9.Base.SOB.SOBAccountingPeriod.Finder.Find(_qStr.ToString(), new OqlParam(DateInfo));
            return SobAccList;
        }
    }
}
