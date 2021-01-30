namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.SM.SO;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.Base;
    using UFIDA.U9.SM.ShipPlan;
    using System.Linq;
    using UFIDA.U9.Lot;
    using UFSoft.UBF.Business;

    /// <summary>
    /// PullShipWithLotSV partial 
    /// </summary>	
    public partial class PullShipWithLotSV
    {
        internal BaseStrategy Select()
        {
            return new PullShipWithLotSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class PullShipWithLotSVImpementStrategy : BaseStrategy
    {
        public PullShipWithLotSVImpementStrategy() { }

        public override object Do(object obj)
        {
            PullShipWithLotSV bpObj = (PullShipWithLotSV)obj;

            //if (string.IsNullOrEmpty(bpObj.JsonRequest))
            //{
            //    return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            //}
            //PullShipWithLotRequest reqHeader = JsonUtil.GetJsonObject<PullShipWithLotRequest>(bpObj.JsonRequest);

            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();

            UFIDA.U9.Base.UserRole.User user = UFIDA.U9.Base.UserRole.User.Finder.FindByID(Context.LoginUserID);
            debugInfo.AppendLine(Context.LoginUser);
            debugInfo.AppendLine(Context.LoginUserID);

            if (user == null)
            {
                return JsonUtil.GetFailResponse("user == null", debugInfo);
            }
            res.AppendLine(user.Code+":"+user.ID+":"+user.Name);
            return JsonUtil.GetSuccessResponse(res.ToString(), debugInfo);
        }
    }

    #endregion


}