namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.InvDoc.TransferOut;
    using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// UpdateTransferOutSV partial 
	/// </summary>	
	public partial class UpdateTransferOutSV 
	{	
		internal BaseStrategy Select()
		{
			return new UpdateTransferOutSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class UpdateTransferOutSVImpementStrategy : BaseStrategy
	{
		public UpdateTransferOutSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			UpdateTransferOutSV bpObj = (UpdateTransferOutSV)obj;

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


			TransferOutData doc1 = TransferOut.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.DocNo)).ToEntityData();
DateTime businessDate = DateTime.Parse(reqHeader.BusinessDate);

			UFIDA.U9.Base.SOB.SOBAccountingPeriod sOBAccountingPeriod = 
CommonUtil.GetSOBAccountPeriodEntity(businessDate, Context.LoginOrg.ID);
			CommonUtil.RunUpdateSQL($"UPDATE InvDoc_TransferOut SET SOBAccountPeriod={sOBAccountingPeriod.ID},BusinessDate='{reqHeader.BusinessDate}' WHERE ID={doc1.ID} AND Org={doc1.Org}");
			foreach (var current in doc1.TransOutAccountPeriods)
			{
				CommonUtil.RunUpdateSQL($"UPDATE InvDoc_TransOutAccPeriod SET SOBAccountPeriod={sOBAccountingPeriod.ID}  WHERE ID={current.ID} AND Org={doc1.Org}");
			}
			foreach (var transOutLine in doc1.TransOutLines)
			{
				foreach (var transOutSubLine in transOutLine.TransOutSubLines)
				{
					CommonUtil.RunUpdateSQL($"UPDATE InvDoc_TransOutSubLine SET PlanRcvDate='{reqHeader.BusinessDate}',PlanSendDate='{reqHeader.BusinessDate}' WHERE Org={doc1.Org} AND ID={transOutSubLine.ID}");
				}
				foreach (var transOutBin in transOutLine.TransOutBins)
				{
					CommonUtil.RunUpdateSQL($"UPDATE InvDoc_TransOutBin SET TimeForPickOrTally='{reqHeader.BusinessDate}' WHERE ID = {transOutBin.ID}  AND Org={doc1.Org} ");
				}
			}
			return JsonUtil.GetSuccessResponse("",debugInfo);
		}		
	}

	#endregion
	
	
}