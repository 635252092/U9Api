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
    using UFIDA.U9.ISV.MiscRcvISV;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.PL;
    using UFSoft.UBF.Util.Context;

	/// <summary>
	/// ApproveMiscRcvTransCustSV partial 
	/// </summary>	
	public partial class ApproveMiscRcvTransCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new ApproveMiscRcvTransCustSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class ApproveMiscRcvTransCustSVImpementStrategy : BaseStrategy
	{
		public ApproveMiscRcvTransCustSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			ApproveMiscRcvTransCustSV bpObj = (ApproveMiscRcvTransCustSV)obj;
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

			CommonCommitMiscRcv sv1 = new CommonCommitMiscRcv();
			sv1.MiscRcvKeys = new List<UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO>();

			CommonApproveMiscRcv client = new CommonApproveMiscRcv();
			client.MiscRcvKeys = new List<UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO>();

			UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO dto = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
			UFIDA.U9.InvDoc.MiscRcv.MiscRcvTrans miscRcv = UFIDA.U9.InvDoc.MiscRcv.MiscRcvTrans.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));
			if (miscRcv == null)
			{
				throw Base.U9Exception.GetException(reqHeader.DocNo+U9Contant.NoFindDoc, debugInfo);
			}
			dto.Code = miscRcv.DocNo;

			sv1.MiscRcvKeys.Add(dto);
			client.MiscRcvKeys.Add(dto);
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
		
			CommonApproveResponse response = new CommonApproveResponse();
			response.DocNo = miscRcv.DocNo;
			response.CurrentStatus = miscRcv.Status.Name;
			return JsonUtil.GetSuccessResponse(response, debugInfo);
		}
	}

	#endregion
	
	
}