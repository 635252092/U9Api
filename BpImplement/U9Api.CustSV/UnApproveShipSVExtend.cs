namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// UnApproveShipSV partial 
	/// </summary>	
	public partial class UnApproveShipSV 
	{	
		internal BaseStrategy Select()
		{
			return new UnApproveShipSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class UnApproveShipSVImpementStrategy : BaseStrategy
	{
		public UnApproveShipSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			UnApproveShipSV bpObj = (UnApproveShipSV)obj;

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
				UFIDA.U9.SM.Ship.Ship ship = UFIDA.U9.SM.Ship.Ship.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.WmsDocNo));

				if (ship == null)
				{
					throw Base.U9Exception.GetException(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
				}
				if (ship.Status == UFIDA.U9.SM.Ship.ShipStateEnum.Approved)
				{
					#region 弃审
					UFIDA.U9.SM.Ship.ShipmentApprove shipmentApprove = new UFIDA.U9.SM.Ship.ShipmentApprove();
					shipmentApprove.IsUnApprove = true;
					shipmentApprove.ShipmentKey = ship.Key;
					shipmentApprove.SysVersion = ship.SysVersion;
					shipmentApprove.Do();
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