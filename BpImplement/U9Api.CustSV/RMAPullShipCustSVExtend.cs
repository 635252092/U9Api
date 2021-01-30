namespace U9Api.CustSV
{
	using System;
    using System.Collections.Generic;
    using System.Text;
	using U9Api.CustSV.Model.Request;
	using U9Api.CustSV.Utils;
	using UFIDA.U9.Base;
    using UFIDA.U9.CBO.DTOs;
    using UFIDA.U9.SM.RMA;
	using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Business;
	using System.Linq;
    using UFIDA.U9.Lot;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.CBO.Enums;
	using U9Api.CustSV.Base;

	/// <summary>
	/// RMAPullShipCustSV partial 
	/// </summary>	
	public partial class RMAPullShipCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new RMAPullShipCustSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class RMAPullShipCustSVImpementStrategy : BaseStrategy
	{
		public RMAPullShipCustSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			RMAPullShipCustSV bpObj = (RMAPullShipCustSV)obj;

			if (string.IsNullOrEmpty(bpObj.JsonRequest))
			{
				return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
			}
			RMAPullShipRequest reqHeader = null;
			try
			{
				reqHeader = JsonUtil.GetJsonObject<RMAPullShipRequest>(bpObj.JsonRequest);
			}
			catch (Exception ex)
			{
				return JsonUtil.GetFailResponse("JSON格式错误");
			}
			StringBuilder res = new StringBuilder();
			StringBuilder debugInfo = new StringBuilder();
				StringBuilder errorInfo = new StringBuilder();
			debugInfo.AppendLine("strat...");
			debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));


			try
			{
				//审核RMA
				if (reqHeader.IsAutoApproveRMA)
				{
					RMA rMA = RMA.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.RMAPullShipLines[0].SrcDocNo));
					if (rMA == null)
					{
						return JsonUtil.GetFailResponse("rMA == null", debugInfo);
					}

					UFIDA.U9.ISV.SM.AuditRMASRV auditRMASRV = new UFIDA.U9.ISV.SM.AuditRMASRV();
					auditRMASRV.ContextDTO = new UFIDA.U9.CBO.Pub.Controller.ContextDTO();
					UFIDA.U9.CBO.Pub.Controller.ContextDTO contextDTO = new UFIDA.U9.CBO.Pub.Controller.ContextDTO();

					contextDTO.CultureName = Context.LoginLanguageCode;
					contextDTO.EntCode = reqHeader.EntCode;
					contextDTO.OrgCode = Context.LoginOrg.Code;
					contextDTO.OrgID = Context.LoginOrg.ID;

					UFIDA.U9.Base.UserRole.User user = UFIDA.U9.Base.UserRole.User.Finder.FindByID(Context.LoginUserID);
					
					if (user == null)
					{
						return JsonUtil.GetFailResponse("user == null",debugInfo);
					}
					contextDTO.UserCode = user.Code;
					contextDTO.UserID = user.ID;
					auditRMASRV.ContextDTO = contextDTO;
					auditRMASRV.RMAKeys = new List<UFIDA.U9.ISV.SM.DocKeyDTO>();

				
					UFIDA.U9.ISV.SM.DocKeyDTO docKeyDTO = new UFIDA.U9.ISV.SM.DocKeyDTO();
					docKeyDTO.DocID = rMA.ID;
					auditRMASRV.RMAKeys.Add(docKeyDTO);
					auditRMASRV.Do();
				}

				RmaPushRefillDocBP rmaPushRefillDocBP = new RmaPushRefillDocBP();
				rmaPushRefillDocBP.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
				rmaPushRefillDocBP.Rmalines = new List<RmaPushRefillDocParaDTO>();
				rmaPushRefillDocBP.SplitTermsForShip = new List<string>();
				rmaPushRefillDocBP.SplitTermsForSO = new List<string>();

				foreach (var reqLine in reqHeader.RMAPullShipLines)
				{
					RMALine current = RMALine.Finder.Find(string.Format("Org={0} and RMA.DocNo='{1}' and DocLineNo={2}", Context.LoginOrg.ID, reqLine.SrcDocNo, reqLine.SrcLineNo));

					if (current == null)
					{
						return JsonUtil.GetFailResponse("rMALine == null", debugInfo);
					}
					if (current.Status != RMAStatusEnum.Posted)
					{
						debugInfo.AppendLine("行不是审核状态");
						errorInfo.AppendLine("行不是审核状态");
						continue;
					}

					RmaPushRefillDocParaDTO rmaTransformParaDTOData = new RmaPushRefillDocParaDTO();
					rmaTransformParaDTOData.RefillType =
						 UFIDA.U9.CBO.SCM.Enums.ReFillTypeEnum.SourceBill;

					rmaTransformParaDTOData.TransformPara = new RmaTransformParaDTO();

					RmaTransformParaDTO rmaTransformParaDTO = new RmaTransformParaDTO();
					rmaTransformParaDTO.ArriveTime = rmaPushRefillDocBP.BusinessDate;
					rmaTransformParaDTO.RmaLine = current.Key;
					rmaTransformParaDTO.TransformDate = rmaTransformParaDTO.ArriveTime;

					DoubleQuantity doubleQuantity = new DoubleQuantity(reqLine.ItemQty, Decimal.Zero, current.UomInfoTu1, current.UomInfoTu1);

					rmaTransformParaDTO.TransformQty = doubleQuantity;

					rmaTransformParaDTOData.TransformPara = rmaTransformParaDTO;

					rmaPushRefillDocBP.Rmalines.Add(rmaTransformParaDTOData);
				}
				if (rmaPushRefillDocBP.Rmalines == null || rmaPushRefillDocBP.Rmalines.Count == 0)
				{
					return JsonUtil.GetFailResponse("rmaPushRefillDocBP.Rmalines == null || rmaPushRefillDocBP.Rmalines.Count == 0;"+ errorInfo.ToString(), debugInfo);
				}
				List<RmaPushRefillDocResultDTO> rmaPushRefillDocResultDTOs = rmaPushRefillDocBP.Do();
				if (rmaPushRefillDocResultDTOs == null || rmaPushRefillDocResultDTOs.Count == 0)
				{
					return JsonUtil.GetFailResponse("没有找到【退回处理单行】请检查入参", debugInfo);
				}
				foreach (var rmaPushRefillDocResultDTO in rmaPushRefillDocResultDTOs)
				{
					UFIDA.U9.SM.Ship.Ship ship = UFIDA.U9.SM.Ship.Ship.Finder.FindByID(rmaPushRefillDocResultDTO.DocKey);
					if (ship == null)
					{
						return JsonUtil.GetFailResponse("生成发货单失败", debugInfo);
					}
					using (ISession session = Session.Open())
					{
						ship.DocNo = reqHeader.WmsDocNo;
						ship.ShipMemo = reqHeader.Remark;
						ship.ShipConfirmDate = ship.BusinessDate;
						//联系人
						if (!string.IsNullOrEmpty(reqHeader.RcvContact))
						{
							UFIDA.U9.SM.Ship.ShipContact rcvContact = UFIDA.U9.SM.Ship.ShipContact.Finder.Find(string.Format("Org={0} and Contact.Name='{1}'", Context.LoginOrg.ID, reqHeader.RcvContact));
							if (rcvContact == null)
							{
								rcvContact = UFIDA.U9.SM.Ship.ShipContact.Create(ship);
								rcvContact.Org = Context.LoginOrg;
								rcvContact.Contact = new UFIDA.U9.CBO.SCM.PropertyTypes.LinkManInfo(UFIDA.U9.CBO.SCM.Enums.OwnerTypeEnum.OrderBy, reqHeader.RcvContact, "", "", "", "");
							}
							ship.Recipients = rcvContact;
						}
						HashSet<RMAPullShipRequest.RMAPullShipLine> hs = new HashSet<RMAPullShipRequest.RMAPullShipLine>();
						foreach (var item in ship.ShipLines)
						{
							RMAPullShipRequest.RMAPullShipLine curLine = reqHeader.RMAPullShipLines.Where(a => 
							a.ItemCode == item.ItemInfo.ItemCode
							&& a.SrcLineNo == item.SrcDocLineNo
							&& !hs.Contains(a)
							).FirstOrDefault();

							if (curLine != null)
							{
								hs.Add(curLine);

								if (!string.IsNullOrEmpty(curLine.LotCode)
									&& CommonUtil.IsNeedLot(curLine.ItemCode, Context.LoginOrg.ID)
									)
								{
									LotMaster lotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.S_Ship.Value, false);
									if (lotMaster == null)
									{
										throw Base.U9Exception.GetException(curLine.LotCode+U9Contant.NoFindLot, debugInfo);
									}
									item.LotInfo.LotMaster = lotMaster;
									item.LotInfo.LotCode = curLine.LotCode;
								}
								if (item.WH == null)
								{
									Warehouse warehouse = Warehouse.FindByCode(Context.LoginOrg, curLine.WHCode);
									if (warehouse == null)
									{
										throw Base.U9Exception.GetException(curLine.WHCode+U9Contant.NoFindWH, debugInfo);
									}
									item.WH = warehouse;
								}
							}
							item.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
						}
						session.Commit();
					}
					if (reqHeader.IsAutoApprove)
					{
						ShipApproveSV sv2 = new ShipApproveSV();
						ShipApproveRequest shipApproveRequest = new ShipApproveRequest();
						shipApproveRequest.DocNo = ship.DocNo;
						shipApproveRequest.OrgID = ship.Org.ID;
						sv2.JsonRequest = JsonUtil.GetJsonString(shipApproveRequest);
						return sv2.Do();
					}
				}

				return JsonUtil.GetSuccessResponse(res.ToString(), debugInfo);
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