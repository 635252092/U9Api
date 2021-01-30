namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;
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
	/// DeleteCustSV partial 
	/// </summary>	
	public partial class DeleteCustSV
	{
		internal BaseStrategy Select()
		{
			return new DeleteCustSVImpementStrategy();
		}
	}

	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class DeleteCustSVImpementStrategy : BaseStrategy
	{
		public DeleteCustSVImpementStrategy() { }

		public override object Do(object obj)
		{
			DeleteCustSV bpObj = (DeleteCustSV)obj;

			if (string.IsNullOrEmpty(bpObj.JsonRequest))
			{
				return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
			}
			DeleteCustRequest reqHeader = null;
			try
			{
				reqHeader = JsonUtil.GetJsonObject<DeleteCustRequest>(bpObj.JsonRequest);
				if (reqHeader == null)
				{
					throw new Exception("request == null");
				}
			}
			catch (Exception ex)
			{
				return JsonUtil.GetFailResponse("JSON格式错误;" + ex.Message);
			}
			StringBuilder res = new StringBuilder();
			StringBuilder debugInfo = new StringBuilder();
			debugInfo.AppendLine("strat...");
			debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));
			using (UFSoft.UBF.Transactions.UBFTransactionScope scope = new UFSoft.UBF.Transactions.UBFTransactionScope(UFSoft.UBF.Transactions.TransactionOption.RequiresNew))
			{
				try
			{
				DocTypeCustEnum docTypeCustEnum = (DocTypeCustEnum)System.Enum.Parse(typeof(DocTypeCustEnum), reqHeader.DocTypeCode, true);
				bool isDeleted = true;
				switch (docTypeCustEnum)
				{
					case DocTypeCustEnum.MaterialDeliveryDoc:
					case DocTypeCustEnum.MaterialIn:
						UFIDA.U9.IssueNew.MaterialDeliveryDocBE.MaterialDeliveryDoc materialDeliveryDoc = UFIDA.U9.IssueNew.MaterialDeliveryDocBE.MaterialDeliveryDoc.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.WmsDocNo));
						if (materialDeliveryDoc == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						if (materialDeliveryDoc.DocState == UFIDA.U9.IssueNew.Enums.DocStateEnum.Approved)
						{
							#region 弃审
							UFIDA.U9.IssueNew.MaterialDeliveryBP.MaterialDeliveryUnApprove sv1 = new UFIDA.U9.IssueNew.MaterialDeliveryBP.MaterialDeliveryUnApprove();
							sv1.MaterialDeliveryDoc = materialDeliveryDoc.Key;
							sv1.CurrentSysVersion = materialDeliveryDoc.SysVersion;
							sv1.IsAutoApp = true;
							sv1.Do();
							Thread.Sleep(500);
							#endregion
						}

						#region 删除
						using (ISession session = Session.Open())
						{
							materialDeliveryDoc = UFIDA.U9.IssueNew.MaterialDeliveryDocBE.MaterialDeliveryDoc.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.WmsDocNo));

							UFIDA.U9.IssueNew.MaterialDeliveryDocBE.MaterialDeliveryDocLine.EntityList entityList = UFIDA.U9.IssueNew.MaterialDeliveryDocBE.MaterialDeliveryDocLine.Finder.FindAll(string.Format("MaterialDeliveryDoc.Org={0} and MaterialDeliveryDoc.DocNo='{1}'",Context.LoginOrg.ID,reqHeader.WmsDocNo));

							for (int i = entityList.Count - 1; i >= 0; i--)
							{
								if (materialDeliveryDoc.m_DeletedMaterialDeliveryDocLines == null)
								{
									materialDeliveryDoc.m_DeletedMaterialDeliveryDocLines = new List<UFIDA.U9.IssueNew.MaterialDeliveryDocBE.MaterialDeliveryDocLine>();
								}
								materialDeliveryDoc.m_DeletedMaterialDeliveryDocLines.Add(entityList[i]);
								entityList[i].Remove();
							}
							materialDeliveryDoc = UFIDA.U9.IssueNew.MaterialDeliveryDocBE.MaterialDeliveryDoc.Finder.FindByID(materialDeliveryDoc.ID);
							if (materialDeliveryDoc != null)
							{
								materialDeliveryDoc.Remove();
								session.Commit();
							}
						}
						#endregion

						break;
					case DocTypeCustEnum.MiscShip:
						UFIDA.U9.InvDoc.MiscShip.MiscShipment miscShip = UFIDA.U9.InvDoc.MiscShip.MiscShipment.Finder.Find(GetFindParam(reqHeader.WmsDocNo));
						if (miscShip == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						if (miscShip.Status == UFIDA.U9.InvDoc.Enums.INVDocStatus.Approved)
						{
							#region 弃审
							UFIDA.U9.ISV.MiscShipISV.CommonUnApporveMiscShipSV commonUnApporveMiscShipSV = new UFIDA.U9.ISV.MiscShipISV.CommonUnApporveMiscShipSV();
							commonUnApporveMiscShipSV.MiscShipmentKeyList = new List<CommonArchiveDataDTO>();

							CommonArchiveDataDTO dtoCommonUnApporveMiscShipSV = new CommonArchiveDataDTO();
							dtoCommonUnApporveMiscShipSV.ID = miscShip.ID;
							commonUnApporveMiscShipSV.MiscShipmentKeyList.Add(dtoCommonUnApporveMiscShipSV);
							commonUnApporveMiscShipSV.Do();
							Thread.Sleep(500);
							#endregion
						}
						#region 删除
						miscShip = UFIDA.U9.InvDoc.MiscShip.MiscShipment.Finder.Find(GetFindParam(reqHeader.WmsDocNo));

						UFIDA.U9.ISV.MiscShipISV.CommonDeleteMiscShip client = new UFIDA.U9.ISV.MiscShipISV.CommonDeleteMiscShip();
						client.MiscShipmentKeyList = new List<CommonArchiveDataDTO>();
						CommonArchiveDataDTO dtoCommonDeleteMiscShip = new CommonArchiveDataDTO();
						dtoCommonDeleteMiscShip.ID = miscShip.ID;
						client.MiscShipmentKeyList.Add(dtoCommonDeleteMiscShip);
						client.Do();
						#endregion

						break;
					case DocTypeCustEnum.MiscRcvTrans:
						UFIDA.U9.InvDoc.MiscRcv.MiscRcvTrans miscRcvTrans = UFIDA.U9.InvDoc.MiscRcv.MiscRcvTrans.Finder.Find(GetFindParam(reqHeader.WmsDocNo));
						if (miscRcvTrans == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						if (miscRcvTrans.Status == UFIDA.U9.InvDoc.Enums.INVDocStatus.Approved)
						{
							#region 弃审
							UFIDA.U9.ISV.MiscRcvISV.CommonUnApproveMiscRcv commonUnApproveMiscRcv = new UFIDA.U9.ISV.MiscRcvISV.CommonUnApproveMiscRcv();
							commonUnApproveMiscRcv.MiscRcvKeys = new List<CommonArchiveDataDTO>();
							CommonArchiveDataDTO dtoCommonUnApproveMiscRcv = new CommonArchiveDataDTO();
							dtoCommonUnApproveMiscRcv.ID = miscRcvTrans.ID;
							commonUnApproveMiscRcv.MiscRcvKeys.Add(dtoCommonUnApproveMiscRcv);
							commonUnApproveMiscRcv.Do();
							Thread.Sleep(500);
							#endregion
						}
						#region 删除
						miscRcvTrans = UFIDA.U9.InvDoc.MiscRcv.MiscRcvTrans.Finder.Find(GetFindParam(reqHeader.WmsDocNo));
						UFIDA.U9.ISV.MiscRcvISV.CommonDeleteMiscRcv Contin = new UFIDA.U9.ISV.MiscRcvISV.CommonDeleteMiscRcv();
						Contin.MiscRcvKeyList = new List<CommonArchiveDataDTO>();
						CommonArchiveDataDTO dtoCommonDeleteMiscRcv = new CommonArchiveDataDTO();
						dtoCommonDeleteMiscRcv.ID = miscRcvTrans.ID;
						Contin.MiscRcvKeyList.Add(dtoCommonDeleteMiscRcv);
						Contin.Do();
						#endregion
						break;
					case DocTypeCustEnum.RCVOnlyDel:
						UFIDA.U9.PM.Rcv.Receivement receivementRtnRCV = UFIDA.U9.PM.Rcv.Receivement.Finder.Find(GetFindParam(reqHeader.WmsDocNo));

						if (receivementRtnRCV == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						UFIDA.U9.PM.Rcv.ReceivementData receivementRtnRCVData =  receivementRtnRCV.ToEntityData();
						UFIDA.U9.ISV.RCV.Proxy.DeleteRCVByDocNOSRVProxy deleteRCVSRVProxy = new UFIDA.U9.ISV.RCV.Proxy.DeleteRCVByDocNOSRVProxy();
						deleteRCVSRVProxy.RcvDocNos = new List<string>();
						deleteRCVSRVProxy.RcvDocNos.Add(receivementRtnRCVData.DocNo);
						deleteRCVSRVProxy.TargetOrgCode = Context.LoginOrg.Code;
						deleteRCVSRVProxy.TargetOrgName = Context.LoginOrg.Name;
						deleteRCVSRVProxy.Do();

						break;
					case DocTypeCustEnum.RCV:
						UFIDA.U9.PM.Rcv.Receivement receivement = UFIDA.U9.PM.Rcv.Receivement.Finder.Find(GetFindParam(reqHeader.WmsDocNo));

						if (receivement == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						UFIDA.U9.PM.Rcv.ReceivementData receivementData = receivement.ToEntityData();
						if (receivementData.Status == UFIDA.U9.PM.Rcv.RcvStatusEnum.Closed.Value)
						{
							#region 弃审
							UnApproveRcvSV unApproveRcvSV = new UnApproveRcvSV();
							CommonUnApproveRequest unApproveRcvRequest = new CommonUnApproveRequest();
							unApproveRcvRequest.WmsDocNo = receivementData.DocNo;
							unApproveRcvSV.JsonRequest = JsonUtil.GetJsonString(unApproveRcvRequest);
							unApproveRcvSV.Do();
							Thread.Sleep(500);
							#endregion
						}

						#region 删除
						UFIDA.U9.ISV.RCV.Proxy.DeleteRCVByDocNOSRVProxy deleteRCVSRVProxy2 = new UFIDA.U9.ISV.RCV.Proxy.DeleteRCVByDocNOSRVProxy();
						deleteRCVSRVProxy2.RcvDocNos = new List<string>();
						deleteRCVSRVProxy2.RcvDocNos.Add(receivementData.DocNo);
						deleteRCVSRVProxy2.TargetOrgCode = Context.LoginOrg.Code;
						deleteRCVSRVProxy2.TargetOrgName = Context.LoginOrg.Name;
						deleteRCVSRVProxy2.Do();
						#endregion

						break;
					case DocTypeCustEnum.Ship:
						UFIDA.U9.SM.Ship.Ship ship = UFIDA.U9.SM.Ship.Ship.Finder.Find(GetFindParam(reqHeader.WmsDocNo));
						if (ship == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						UFIDA.U9.SM.Ship.ShipData shipData = ship.ToEntityData();
						#region 弃审
						if (shipData.Status == UFIDA.U9.SM.Ship.ShipStateEnum.Approved.Value)
						{
							UnApproveShipSV unApproveRcvSV = new UnApproveShipSV();
							CommonUnApproveRequest unApproveShipRequest = new CommonUnApproveRequest();
							unApproveShipRequest.WmsDocNo = shipData.DocNo;
							unApproveRcvSV.JsonRequest = JsonUtil.GetJsonString(unApproveShipRequest);
							unApproveRcvSV.Do();
							Thread.Sleep(500);
						}
						#endregion
						#region 删除
						
						UFIDA.U9.ISV.SM.Proxy.DeleteShipProxy  deleteShip = new UFIDA.U9.ISV.SM.Proxy.DeleteShipProxy();
						deleteShip.ShipDocNos = new List<string>();
						deleteShip.ShipDocNos.Add(reqHeader.WmsDocNo);
						deleteShip.Do();

						#endregion
						break;
					case DocTypeCustEnum.TransferOut:
						UFIDA.U9.InvDoc.TransferOut.TransferOut transferOut = UFIDA.U9.InvDoc.TransferOut.TransferOut.Finder.Find(GetFindParam(reqHeader.WmsDocNo));

						if (transferOut == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						if (transferOut.Status == UFIDA.U9.InvDoc.TransferOut.Status.Approved)
						{
							#region 弃审
							//using (ISession session = Session.Open())
							//{
							//	UFIDA.U9.InvDoc.TransferOut.UnApprovedEvent ev = new UFIDA.U9.InvDoc.TransferOut.UnApprovedEvent();
							//	transferOut.StateMachineInstance.ApprovedState_ApproveToOpen(ev);
							//	session.Modify(transferOut);
							//	session.Commit();
							//}
							//using (ISession session = UFSoft.UBF.Business.Session.Open())
							//{
							//	transferOut.Status = UFIDA.U9.InvDoc.TransferOut.Status.Opening;
							//	transferOut.ApprovedBy = "";
							//	transferOut.ApprovedOn = DateTime.MinValue;
							//	transferOut.CancelApprovedBy = Context.LoginUser;
							//	transferOut.CancelApprovedOn = UFSoft.UBF.Util.Context.PlatformContext.Current.OperationDate;
							//	transferOut.CurrAction = UFIDA.U9.InvDoc.TransferOut.TransferOutActionEnum.UIUpdate;
							//	session.Commit();
							//}
							UnApproveTransferOutSV unApproveTransferOut = new UnApproveTransferOutSV();
							CommonUnApproveRequest unApproveTransferOutRequest = new CommonUnApproveRequest();
							unApproveTransferOutRequest.WmsDocNo = reqHeader.WmsDocNo;
							unApproveTransferOut.JsonRequest = JsonUtil.GetJsonString(unApproveTransferOutRequest);
							unApproveTransferOut.Do();
							Thread.Sleep(500);
							#endregion
						}
						#region 删除
						//transferOut = UFIDA.U9.InvDoc.TransferOut.TransferOut.Finder.Find(GetFindParam(reqHeader.WmsDocNo));

						UFIDA.U9.ISV.TransferOutISV.Proxy.CommonDeleteTransferOutSVProxy commonDeleteTransferOutSV = new UFIDA.U9.ISV.TransferOutISV.Proxy.CommonDeleteTransferOutSVProxy();
						commonDeleteTransferOutSV.CommonDTOList = new List<CommonArchiveDataDTOData>();
						CommonArchiveDataDTOData dto = new CommonArchiveDataDTOData();
						dto.Code = reqHeader.WmsDocNo;
						commonDeleteTransferOutSV.CommonDTOList.Add(dto);
						commonDeleteTransferOutSV.Do();
						#endregion
						break;
					case DocTypeCustEnum.TransferIn:
						UFIDA.U9.InvDoc.TransferIn.TransferIn transferIn = UFIDA.U9.InvDoc.TransferIn.TransferIn.Finder.Find(GetFindParam(reqHeader.WmsDocNo));
						if (transferIn == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo + Base.U9Contant.NoFindDoc, debugInfo);
						}
						UFIDA.U9.InvDoc.TransferIn.TransferInData transferInData  =transferIn.ToEntityData();
						if (transferInData.Status == UFIDA.U9.InvDoc.TransferIn.TransInStatus.Approved.Value)
						{
							#region 弃审
							//UFIDA.U9.InvDoc.TransferIn.UnApprovedTransferInBP unApprovedTransferInBP = new UFIDA.U9.InvDoc.TransferIn.UnApprovedTransferInBP();
							//unApprovedTransferInBP.DocKeyList = new List<UFIDA.U9.InvDoc.TransferIn.TransferIn.EntityKey>();
							//unApprovedTransferInBP.DocKeyList.Add(transferIn.Key);
							//unApprovedTransferInBP.Do();

							UFIDA.U9.ISV.TransferInISV.Proxy.TransferInBatchUnApproveSRVProxy transferInBatchUnApproveSRV = new UFIDA.U9.ISV.TransferInISV.Proxy.TransferInBatchUnApproveSRVProxy();
							transferInBatchUnApproveSRV.DocList = new List<CommonArchiveDataDTOData>();
							CommonArchiveDataDTOData dtoTransferInBatchUnApproveSRV = new CommonArchiveDataDTOData();
							dtoTransferInBatchUnApproveSRV.Code = transferInData.DocNo;
							transferInBatchUnApproveSRV.DocList.Add(dtoTransferInBatchUnApproveSRV);
							transferInBatchUnApproveSRV.Do();
							Thread.Sleep(500);
							#endregion
						}

						#region 删除
						//transferIn = UFIDA.U9.InvDoc.TransferIn.TransferIn.Finder.Find(GetFindParam(reqHeader.WmsDocNo));

						UFIDA.U9.ISV.TransferInISV.Proxy.CommonDeleteTransferInSVProxy commonDeleteTransferInSV = new UFIDA.U9.ISV.TransferInISV.Proxy.CommonDeleteTransferInSVProxy();
						commonDeleteTransferInSV.BeDelTransferInDocInfoList = new List<CommonArchiveDataDTOData>();
						CommonArchiveDataDTOData dtoTransferIn = new CommonArchiveDataDTOData();
						dtoTransferIn.Code = reqHeader.WmsDocNo;
						commonDeleteTransferInSV.BeDelTransferInDocInfoList.Add(dtoTransferIn);
						commonDeleteTransferInSV.Do();
						#endregion
						break;
					case DocTypeCustEnum.RcvRptDoc:
						UFIDA.U9.Complete.RCVRpt.RcvRptDoc rcvRptDoc = UFIDA.U9.Complete.RCVRpt.RcvRptDoc.Finder.Find(GetFindParam(reqHeader.WmsDocNo));
						if (rcvRptDoc == null)
						{
							return JsonUtil.GetFailResponse(reqHeader.WmsDocNo+Base.U9Contant.NoFindDoc, debugInfo);
						}
						if (rcvRptDoc.DocState == UFIDA.U9.Complete.Enums.RcvRptDocStateEnum.Approved)
						{
							#region 弃审
							UFIDA.U9.Complete.RcvRptBP.ApproveRcvRpt approveRcvRpt = new UFIDA.U9.Complete.RcvRptBP.ApproveRcvRpt();
							approveRcvRpt.CurruntSysVersion = rcvRptDoc.SysVersion;
							approveRcvRpt.IsAutoApproved = false;
							approveRcvRpt.RcvRptDocKey = rcvRptDoc.Key;
							approveRcvRpt.Do();
							Thread.Sleep(500);
							#endregion
						}
						#region 删除
						rcvRptDoc = UFIDA.U9.Complete.RCVRpt.RcvRptDoc.Finder.Find(GetFindParam(reqHeader.WmsDocNo));

						UFIDA.U9.Complete.RcvRptBP.DeleteRcvRptRpt deleteRcvRptRpt = new UFIDA.U9.Complete.RcvRptBP.DeleteRcvRptRpt();
						deleteRcvRptRpt.RcvRptKey = rcvRptDoc.Key;
						deleteRcvRptRpt.Do();
						#endregion
						break;
					default:
						isDeleted = false;
						break;
				}
				if (!isDeleted)
				{
						scope.Rollback();
						return JsonUtil.GetFailResponse("没有找到此单据类型的删除方法", debugInfo);
				}

					scope.Commit();
					return JsonUtil.GetSuccessResponse(reqHeader.WmsDocNo + "删除成功", debugInfo);
			}
				catch (Exception ex)
				{
					LogUtil.WriteDebugInfoLog(debugInfo.ToString());
					//throw Base.U9Exception.GetInnerException(ex);
					scope.Rollback();
					return JsonUtil.GetFailResponse(ex, debugInfo);//返回，不执行 scope.Commit();就会回滚
				}
			}
		}

		public static string GetFindParam(string docNo)
		{
			return string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, docNo);
		}
	}

	#endregion


}