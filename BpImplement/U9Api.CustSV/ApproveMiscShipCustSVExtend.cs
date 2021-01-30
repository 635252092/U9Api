namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Model.Response;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.ISV.MiscShipISV;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.PL;
    using UFSoft.UBF.Util.Context;

	/// <summary>
	/// ApproveMiscShipCustSV partial 
	/// </summary>	
	public partial class ApproveMiscShipCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new ApproveMiscShipCustSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class ApproveMiscShipCustSVImpementStrategy : BaseStrategy
	{
		public ApproveMiscShipCustSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			ApproveMiscShipCustSV bpObj = (ApproveMiscShipCustSV)obj;

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
			CommonCommitMiscShipSV sv1 = new CommonCommitMiscShipSV();
			sv1.MiscShipmentKeyList = new List<UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO>();
			CommonApproveMiscShipSV client = new CommonApproveMiscShipSV();
			client.MiscShipmentKeyList = new List<UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO>();

			UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO dto = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
			UFIDA.U9.InvDoc.MiscShip.MiscShipment miscShipment = UFIDA.U9.InvDoc.MiscShip.MiscShipment.Finder.Find(string.Format("Org={0} and DocNo='{1}'",Context.LoginOrg.ID,reqHeader.DocNo));
			if (miscShipment == null)
			{
				throw Base.U9Exception.GetException(reqHeader.DocNo + Base.U9Contant.NoFindDoc, debugInfo);
			}
			dto.Code = miscShipment.DocNo;
			sv1.MiscShipmentKeyList.Add(dto);
			client.MiscShipmentKeyList.Add(dto);
			try
			{
				sv1.Do();
				client.Do();
			}
			catch (Exception ex)
			{
				LogUtil.WriteDebugInfoLog(debugInfo.ToString());
				throw Base.U9Exception.GetInnerException(ex);
			}
			
			miscShipment = UFIDA.U9.InvDoc.MiscShip.MiscShipment.Finder.Find("DocNo=@DocNo", new OqlParam(reqHeader.DocNo));
			CommonApproveResponse response = new CommonApproveResponse();
			response.DocNo = miscShipment.DocNo;
			response.CurrentStatus = miscShipment.Status.Name;
			return JsonUtil.GetSuccessResponse(response, debugInfo);
		}
	}

	#endregion
	
	
}