namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Model.Response;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.ISV.TransferInISV;
    using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// ApproveTransferInCustSV partial 
	/// </summary>	
	public partial class ApproveTransferInCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new ApproveTransferInCustSVImpementStrategy();	
		}		
	}

	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class ApproveTransferInCustSVImpementStrategy : BaseStrategy
	{
		public ApproveTransferInCustSVImpementStrategy() { }

		public override object Do(object obj)
		{
			ApproveTransferInCustSV bpObj = (ApproveTransferInCustSV)obj;

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

			TransferInBatchApproveSRV client = new TransferInBatchApproveSRV();
			client.ApprovedBy = Context.LoginUser;
			if (!string.IsNullOrEmpty(reqHeader.BusinessDate))
				client.ApprovedOn = DateTime.Parse(reqHeader.BusinessDate);
			client.DocList = new List<UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO>();
			UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO dto = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
			dto.Code = reqHeader.DocNo;
			client.DocList.Add(dto);

			try
			{
				client.Do();
			}
			catch (Exception ex)
			{
				LogUtil.WriteDebugInfoLog(debugInfo.ToString());
				throw Base.U9Exception.GetInnerException(ex);
			}


			UFIDA.U9.InvDoc.TransferIn.TransferIn ship = UFIDA.U9.InvDoc.TransferIn.TransferIn.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));
			CommonApproveResponse response = new CommonApproveResponse();
			response.DocNo = ship.DocNo;
			response.CurrentStatus = ship.Status.Name;
			return JsonUtil.GetSuccessResponse(response, debugInfo);
		}
	}

	#endregion
	
	
}