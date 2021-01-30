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


	/// <summary>
	/// RcvRptDocApproveSV partial 
	/// </summary>	
	public partial class RcvRptDocApproveSV 
	{	
		internal BaseStrategy Select()
		{
			return new RcvRptDocApproveSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class RcvRptDocApproveSVImpementStrategy : BaseStrategy
	{
		public RcvRptDocApproveSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			RcvRptDocApproveSV bpObj = (RcvRptDocApproveSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CommonApproveRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CommonApproveRequest>(bpObj.JsonRequest);
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
                UFIDA.U9.Complete.RCVRpt.RcvRptDoc rcvRptDoc = UFIDA.U9.Complete.RCVRpt.RcvRptDoc.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));

                if (rcvRptDoc == null)
                {
                    throw Base.U9Exception.GetException("rcvRptDoc == null");
                }
                CommonApproveResponse response = null;
                UFIDA.U9.Complete.RcvRptBP.ApproveRcvRpt sv = new UFIDA.U9.Complete.RcvRptBP.ApproveRcvRpt();
                if (rcvRptDoc.DocState == UFIDA.U9.Complete.Enums.RcvRptDocStateEnum.Opened
                    || rcvRptDoc.DocState == UFIDA.U9.Complete.Enums.RcvRptDocStateEnum.Approving)
                {
                    DoService(reqHeader, ref rcvRptDoc, ref response, sv, true);
                }
                if (response != null)
                {
                    return JsonUtil.GetSuccessResponse(JsonUtil.GetJsonString(response), debugInfo);
                }
                response = new CommonApproveResponse();
                response.CurrentStatus = rcvRptDoc.DocState.Name;
                response.DocNo = rcvRptDoc.DocNo;
                return JsonUtil.GetFailResponse("数据没有改变", debugInfo);
            }
            catch (Exception ex)
            {
                LogUtil.WriteDebugInfoLog(debugInfo.ToString());
                throw Base.U9Exception.GetInnerException(ex);
            }
        }

        private static void DoService(CommonApproveRequest reqHeader, ref UFIDA.U9.Complete.RCVRpt.RcvRptDoc ship, ref CommonApproveResponse response, UFIDA.U9.Complete.RcvRptBP.ApproveRcvRpt s, bool isSubmit)
        {
            s.CurruntSysVersion = ship.SysVersion;
            s.IsAutoApproved = true;
            s.RcvRptDocKey = ship.Key;
           UFIDA.U9.Complete.RcvRptBP.RcvRptInfoDTO dto = s.Do();


           ship = UFIDA.U9.Complete.RCVRpt.RcvRptDoc.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));
            response = new CommonApproveResponse();
            response.DocNo = ship.DocNo;
            response.CurrentStatus = ship.DocState.Name;
        }
	}

	#endregion
	
	
}