namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using U9Api.CustSV.Base;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.SM.SO;
    using System.Linq;
    using UFIDA.U9.ISV.SM;
    using UFSoft.UBF.Business;
    using UFIDA.U9.Lot;
    using UFIDA.U9.Base;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.PM.Rcv;
    using UFIDA.U9.PM.DTOs;
    using UFIDA.U9.CBO.DTOs;


    /// <summary>
    /// RcvToRefillRcvCustSV partial 
    /// </summary>	
    public partial class RcvToRefillRcvCustSV
    {
        internal BaseStrategy Select()
        {
            return new RcvToRefillRcvCustSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class RcvToRefillRcvCustSVImpementStrategy : BaseStrategy
    {
        public RcvToRefillRcvCustSVImpementStrategy() { }

        public override object Do(object obj)
        {
            RcvToRefillRcvCustSV bpObj = (RcvToRefillRcvCustSV)obj;
            return null;
        }
    }

    #endregion


}