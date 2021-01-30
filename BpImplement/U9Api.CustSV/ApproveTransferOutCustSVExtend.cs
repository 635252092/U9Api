namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Model.Response;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.ISV.TransferOutISV;
    using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// ApproveTransferOutCustSV partial 
	/// </summary>	
	public partial class ApproveTransferOutCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new ApproveTransferOutCustSVImpementStrategy();	
		}		
	}

	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class ApproveTransferOutCustSVImpementStrategy : BaseStrategy
	{
		public ApproveTransferOutCustSVImpementStrategy() { }

		public override object Do(object obj)
		{
			ApproveTransferOutCustSV bpObj = (ApproveTransferOutCustSV)obj;


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
			UFIDA.U9.InvDoc.TransferOut.TransferOut ship2 = UFIDA.U9.InvDoc.TransferOut.TransferOut.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));
			debugInfo.AppendLine(ship2.BusinessDate.ToString("yyyy-MM-dd"));
			try
			{
				UFIDA.U9.ISV.TransferOutISV.Proxy.TransferOutBatchApproveSRVProxy client = new UFIDA.U9.ISV.TransferOutISV.Proxy.TransferOutBatchApproveSRVProxy();
				client.ApprovedBy = Context.LoginUser;
				if (!string.IsNullOrEmpty(reqHeader.BusinessDate))
					client.ApprovedOn = DateTime.Parse(reqHeader.BusinessDate);
				client.DocList = new List<UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData>();
				UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData dto = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
				dto.Code = reqHeader.DocNo;
				client.DocList.Add(dto);
				client.TargetOrgCode = Context.LoginOrg.Code;
				UFSoft.UBF.Util.Context.PlatformContext.Current.OperationDate = client.ApprovedOn;
				client.Do();


				//using (UFSoft.UBF.Business.Session session = UFSoft.UBF.Business.Session.Open())
				//{
				//	UFIDA.U9.InvDoc.TransferOut.TransferOut transferOut;
				//	transferOut = UFIDA.U9.InvDoc.TransferOut.TransferOut.Finder.Find(string.Concat(new object[]
				//		{
				//				" Org = ",
				//				Context.LoginOrg.ID,
				//				" and DocNo = '",
				//				reqHeader.DocNo,
				//				"'"
				//		}), null);
				//	transferOut.Status = UFIDA.U9.InvDoc.TransferOut.Status.Approved;
				//	if (!string.IsNullOrEmpty(reqHeader.BusinessDate))
				//		transferOut.ApprovedOn = DateTime.Parse(reqHeader.BusinessDate);
				//	transferOut.ApprovedBy = Context.LoginUser;
				//	transferOut.CurrAction = UFIDA.U9.InvDoc.TransferOut.TransferOutActionEnum.UIUpdate;
				//	UFIDA.U9.Base.SOB.SOBAccountingPeriod sOBAccountingPeriod = CommonUtil.GetSOBAccountPeriodEntity(transferOut.ApprovedOn, Context.LoginOrg.ID);
				//	transferOut.SOBAccountPeriod = sOBAccountingPeriod;
				//	foreach (var item in transferOut.TransOutAccountPeriods)
				//	{
				//		item.SOBAccountPeriod = sOBAccountingPeriod;
				//	}
				//	transferOut.BusinessDate = transferOut.ApprovedOn;
				//	foreach (var transOutLine in transferOut.TransOutLines) {
				//		foreach (var transOutSubLine in transOutLine.TransOutSubLines)
				//		{
				//			transOutSubLine.PlanRcvDate = transferOut.ApprovedOn;
				//			transOutSubLine.PlanSendDate = transferOut.ApprovedOn;
				//		}
				//		foreach (var transOutBin in transOutLine.TransOutBins)
				//		{
				//			transOutBin.TimeForPickOrTally = transferOut.ApprovedOn;
				//		}
				//	}
				//	session.Commit();
				//}
			}
			catch (Exception ex)
			{
				LogUtil.WriteDebugInfoLog(debugInfo.ToString());
				throw Base.U9Exception.GetInnerException(ex);
			}


			UFIDA.U9.InvDoc.TransferOut.TransferOut ship = UFIDA.U9.InvDoc.TransferOut.TransferOut.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo));
			CommonApproveResponse response = new CommonApproveResponse();
			response.DocNo = ship.DocNo;
			response.CurrentStatus = ship.Status.Name;
			return JsonUtil.GetSuccessResponse(response, debugInfo);
		}
	}

	#endregion
	
	
}