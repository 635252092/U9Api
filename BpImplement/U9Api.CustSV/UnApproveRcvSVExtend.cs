namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using U9Api.CustSV.Model.Enum;
	using U9Api.CustSV.Model.Request;
	using U9Api.CustSV.Utils;
	using UFIDA.U9.Base;
	using UFIDA.U9.Base.Doc;
	using UFIDA.U9.CBO.Pub.Controller;
	using UFIDA.U9.ISV.MiscShipISV;
	using UFIDA.U9.ISV.TransferOutISV;
	using UFSoft.UBF.AopFrame;
	using UFSoft.UBF.Business;

	/// <summary>
	/// UnApproveRcvSV partial 
	/// </summary>	
	public partial class UnApproveRcvSV 
	{	
		internal BaseStrategy Select()
		{
			return new UnApproveRcvSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class UnApproveRcvSVImpementStrategy : BaseStrategy
	{
		public UnApproveRcvSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			UnApproveRcvSV bpObj = (UnApproveRcvSV)obj;

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
				UFIDA.U9.PM.Rcv.Receivement receivement = UFIDA.U9.PM.Rcv.Receivement.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.WmsDocNo));

				if (receivement == null)
				{
					throw Base.U9Exception.GetException(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
				}
				if (receivement.Status == UFIDA.U9.PM.Rcv.RcvStatusEnum.Closed)
				{
					#region 弃审
					//UFIDA.U9.ISV.RCV.ApproveRCVSRV approveRCVSRV = new UFIDA.U9.ISV.RCV.ApproveRCVSRV();
					UFIDA.U9.ISV.RCV.Proxy.ApproveRCVSRVProxy approveRCVSRV = new UFIDA.U9.ISV.RCV.Proxy.ApproveRCVSRVProxy();
					approveRCVSRV.ActType = UFIDA.U9.PM.Enums.ActivateTypeEnum.UnApprovedAct.Value;
					approveRCVSRV.RCVLogicKeyINFOs = new List<UFIDA.U9.ISV.RCV.DTO.RCVLogicKeyINFOData>();
					UFIDA.U9.ISV.RCV.DTO.RCVLogicKeyINFOData rCVLogicKeyINFO = new UFIDA.U9.ISV.RCV.DTO.RCVLogicKeyINFOData();
					rCVLogicKeyINFO.RcvID = receivement.Key.ID;
					approveRCVSRV.RCVLogicKeyINFOs.Add(rCVLogicKeyINFO);
					approveRCVSRV.Do();
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