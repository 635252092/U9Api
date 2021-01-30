namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;
	using U9Api.CustSV.Model.Request;
	using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFSoft.UBF.Business;
	using U9Api.CustSV.Base;

	/// <summary>
	/// UnApproveTransferOutSV partial 
	/// </summary>	
	public partial class UnApproveTransferOutSV 
	{	
		internal BaseStrategy Select()
		{
			return new UnApproveTransferOutSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class UnApproveTransferOutSVImpementStrategy : BaseStrategy
	{
		public UnApproveTransferOutSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			UnApproveTransferOutSV bpObj = (UnApproveTransferOutSV)obj;

			if (string.IsNullOrEmpty(bpObj.JsonRequest))
			{
				return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
			}
			CommonUnApproveRequest reqHeader = null;
			try
			{
				reqHeader = JsonUtil.GetJsonObject<CommonUnApproveRequest>(bpObj.JsonRequest);
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
				UFIDA.U9.InvDoc.TransferOut.TransferOut transferOut = UFIDA.U9.InvDoc.TransferOut.TransferOut.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.WmsDocNo));

				if (transferOut == null)
				{
					throw U9Exception.GetException(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
				}
				if (transferOut.Status == UFIDA.U9.InvDoc.TransferOut.Status.Approved)
				{
					#region 弃审

					using (ISession session = UFSoft.UBF.Business.Session.Open())
					{
						transferOut.Status = UFIDA.U9.InvDoc.TransferOut.Status.Opening;
						transferOut.ApprovedBy = "";
						transferOut.ApprovedOn = DateTime.MinValue;
						transferOut.CancelApprovedBy = Context.LoginUser;
						transferOut.CancelApprovedOn = UFSoft.UBF.Util.Context.PlatformContext.Current.OperationDate;
						//UFSoft.UBF.Util.Context.PlatformContext.Current.OperationDate = DateTime.Now;
						transferOut.CurrAction = UFIDA.U9.InvDoc.TransferOut.TransferOutActionEnum.UIUpdate;
						session.Commit();
					}
					#endregion
				}

				return JsonUtil.GetSuccessResponse(reqHeader.WmsDocNo + "弃审成功", debugInfo);
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