namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using UFIDA.U9.Base;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Model.Response;
    using U9Api.CustSV.Base;

    /// <summary>
    /// RcvApproveSV partial 
    /// </summary>	
    public partial class RcvApproveSV 
	{	
		internal BaseStrategy Select()
		{
			return new RcvApproveSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
    internal partial class RcvApproveSVImpementStrategy : BaseStrategy
    {
        public RcvApproveSVImpementStrategy() { }

        public override object Do(object obj)
        {
            RcvApproveSV bpObj = (RcvApproveSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            RcvApproveRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<RcvApproveRequest>(bpObj.JsonRequest);
            }
            catch (Exception ex)
            {
                return JsonUtil.GetFailResponse("JSON格式错误");
            }
            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

            try
            {
                UFIDA.U9.PM.Rcv.Receivement rcv = UFIDA.U9.PM.Rcv.Receivement.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));
                if (rcv == null)
                {
                    throw Base.U9Exception.GetException(reqHeader.DocNo+U9Contant.NoFindDoc,debugInfo);
                }
                UFIDA.U9.PM.Rcv.ReceivementData receivementData = rcv.ToEntityData();
                CommonApproveResponse response = null;
                //UFIDA.U9.PM.Rcv.RcvApprovedSV sv = new UFIDA.U9.PM.Rcv.RcvApprovedSV();
                UFIDA.U9.PM.Rcv.Proxy.RcvApprovedSVProxy sv = new UFIDA.U9.PM.Rcv.Proxy.RcvApprovedSVProxy();

                //sv.ActType = UFIDA.U9.PM.Enums.ActivateTypeEnum.ApprovingAct.Value;
                //sv.DocHead = receivementData.ID;
                //sv.Do();

                sv.ActType = UFIDA.U9.PM.Enums.ActivateTypeEnum.ApprovedAct.Value;
                sv.DocHead = receivementData.ID;
                sv.Do();
                rcv = UFIDA.U9.PM.Rcv.Receivement.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));
                response = new CommonApproveResponse();
                response.DocNo = rcv.DocNo;
                response.CurrentStatus = rcv.Status.Name;

                return JsonUtil.GetSuccessResponse(JsonUtil.GetJsonString(response), debugInfo);
            }
            catch (Exception ex)
            {
                LogUtil.WriteDebugInfoLog(debugInfo.ToString());
                throw Base.U9Exception.GetInnerException(ex);
            }
        }
    }

	#endregion
	
	
}