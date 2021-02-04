namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Base;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Model.Response;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.IssueNew.MaterialDeliveryDocBE;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.PL;
    using UFSoft.UBF.Util.Context;

	/// <summary>
	/// ApproveMaterialDeliveryDocSV partial 
	/// </summary>	
	public partial class ApproveMaterialDeliveryDocSV 
	{	
		internal BaseStrategy Select()
		{
			return new ApproveMaterialDeliveryDocSVImpementStrategy();	
		}		
	}

	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class ApproveMaterialDeliveryDocSVImpementStrategy : BaseStrategy
	{
		public ApproveMaterialDeliveryDocSVImpementStrategy() { }

		public override object Do(object obj)
		{
			ApproveMaterialDeliveryDocSV bpObj = (ApproveMaterialDeliveryDocSV)obj;
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

			MaterialDeliveryDoc materialDeliveryDoc = MaterialDeliveryDoc.Finder.Find("DocNo=@DocNo", new OqlParam(reqHeader.DocNo));
			if (materialDeliveryDoc == null)
			{
				throw Base.U9Exception.GetException(reqHeader.DocNo + U9Contant.NoFindDoc, debugInfo);
			}
			if (materialDeliveryDoc.DocState != UFIDA.U9.IssueNew.Enums.DocStateEnum.Approved)
			{
				int invokeCount = 1;
				if (materialDeliveryDoc.DocState == UFIDA.U9.IssueNew.Enums.DocStateEnum.Opened)
				{
					invokeCount = 2;
				}
				UFIDA.U9.ISV.MO.MaterialDeliveryDocApproveSV proxy = new UFIDA.U9.ISV.MO.MaterialDeliveryDocApproveSV();
				for (int i = 0; i < invokeCount; i++)
				{
					proxy.IsAutoApp = true;
					proxy.CurrentSysVersion = materialDeliveryDoc.SysVersion;
					proxy.MaterialDeliveryID = materialDeliveryDoc.ID;
					try
					{
						proxy.Do();
					}
					catch (Exception ex)
					{
						LogUtil.WriteDebugInfoLog(debugInfo.ToString());
						throw U9Exception.GetInnerException(ex);
					}
				}
			}
			CommonApproveResponse response = new CommonApproveResponse();
			response.DocNo = materialDeliveryDoc.DocNo;
			response.CurrentStatus = materialDeliveryDoc.DocState.Name;
			return JsonUtil.GetSuccessResponse(response, debugInfo);
		}
	}

	#endregion
	
	
}