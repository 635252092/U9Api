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
    /// ShipApproveSV partial 
    /// </summary>	
    public partial class ShipApproveSV 
	{	
		internal BaseStrategy Select()
		{
			return new ShipApproveSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
    internal partial class ShipApproveSVImpementStrategy : BaseStrategy
    {
        public ShipApproveSVImpementStrategy() { }

        public override object Do(object obj)
        {
            ShipApproveSV bpObj = (ShipApproveSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            ShipApproveRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<ShipApproveRequest>(bpObj.JsonRequest);
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
                UFIDA.U9.SM.Ship.Ship ship = UFIDA.U9.SM.Ship.Ship.Finder.Find(string.Format("Org={0} and DocNo='{1}'", reqHeader.OrgID, reqHeader.DocNo));
                if (ship == null)
                {
                    //return JsonUtil.GetFailResponse("ship == null");
                    throw U9Exception.GetException(reqHeader.DocNo+U9Contant.NoFindDoc,debugInfo);
                }
                CommonApproveResponse response = null;
                UFIDA.U9.ISV.SM.AuditShipSV sv = new UFIDA.U9.ISV.SM.AuditShipSV();
                sv.IsSubmit = false;
                sv.ShipKeys = new List<UFIDA.U9.ISV.SM.DocKeyDTO>();
                UFIDA.U9.ISV.SM.DocKeyDTO dto = new UFIDA.U9.ISV.SM.DocKeyDTO();
                dto.DocID = ship.ID;
                dto.DocNO = ship.DocNo;
                sv.ShipKeys.Add(dto);
                sv.Do();
                ship = UFIDA.U9.SM.Ship.Ship.Finder.Find(string.Format("Org={0} and DocNo='{1}'", reqHeader.OrgID, reqHeader.DocNo));
                response = new CommonApproveResponse();
                response.DocNo = ship.DocNo;
                response.CurrentStatus = ship.Status.Name;

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