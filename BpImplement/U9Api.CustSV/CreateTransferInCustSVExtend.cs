namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Base;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.CBO.Enums;
    using UFIDA.U9.CBO.Pub.Controller;
    using UFIDA.U9.CBO.SCM.ProjectTask;
    using UFIDA.U9.ISV.TransferInISV;
    using UFIDA.U9.Lot;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Business;
    using UFSoft.UBF.PL.Engine;
    using UFSoft.UBF.Util.Context;

	/// <summary>
	/// CreateTransferInCustSV partial 
	/// </summary>	
	public partial class CreateTransferInCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new CreateTransferInCustSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class CreateTransferInCustSVImpementStrategy : BaseStrategy
	{
		public CreateTransferInCustSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			CreateTransferInCustSV bpObj = (CreateTransferInCustSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateTransferInRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateTransferInRequest>(bpObj.JsonRequest);
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
                    CommonCreateTransferInSV client = new CommonCreateTransferInSV();
                    client.TransferInDTOList = new List<IC_TransferInDTO>();

                    //头
                    IC_TransferInDTO dtoHeader = new IC_TransferInDTO();
                    dtoHeader.TransInDocType = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                    //dtoHeader.TransInDocType.Code = "TransIn002";//单据类型
                    dtoHeader.TransInDocType.Code = reqHeader.DocTypeCode;
                    //dtoHeader.TransferType = UFIDA.U9.InvDoc.Enums.TransferTypeEnum.BetweenWh;
                    dtoHeader.Org = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                    dtoHeader.Org.Code = Context.LoginOrg.Code;
                    dtoHeader.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
                    dtoHeader.Memo = reqHeader.Remark;
                    dtoHeader.TransInLines = new List<IC_TransInLineDTO>();
                    dtoHeader.SysState = ObjectState.Inserted;
                    dtoHeader.DocNo = reqHeader.WmsDocNo;
                    dtoHeader.CreatedBy = Context.LoginUser;

                    foreach (var reqLine in reqHeader.TransferInLineLines)
                    {

                        //行
                        IC_TransInLineDTO dtoLine = new IC_TransInLineDTO();
                        dtoLine.ItemInfo = new UFIDA.U9.CBO.SCM.Item.ItemInfo();
                        dtoLine.ItemInfo.ItemCode = reqLine.ItemCode;//料品                
                        dtoLine.StoreUOMQty = reqLine.ItemQty;//调入数量
                        dtoLine.CostUOMQty = reqLine.ItemQty;  //成本数量   

                        dtoLine.TransInWh = new CommonArchiveDataDTO();
                        dtoLine.TransInWh.Code = reqLine.ToWHCode; ;//存储地点
                        dtoLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                        //Bom_line.LotInfo.LotMaster.EntityID = 1001007189629053;
                        dtoLine.SysState = ObjectState.Inserted;
                        dtoLine.TransInSubLines = new List<IC_TransInSubLineDTO>();

                        //子行
                        IC_TransInSubLineDTO dtoSubLine = new IC_TransInSubLineDTO();
                        if (!string.IsNullOrEmpty(reqLine.ProjectCode))
                        {
                            debugInfo.AppendLine(reqLine.ProjectCode);
                            Project project = Project.FindByCode(reqLine.ProjectCode);
                            if (project == null)
                            {
                                return JsonUtil.GetFailResponse("project == null", debugInfo);
                            }
                            dtoLine.Project = new CommonArchiveDataDTO();
                            dtoLine.Project.ID = project.ID;

                            dtoSubLine.Project = new CommonArchiveDataDTO();
                            dtoSubLine.Project.ID = project.ID;
                        }
                        if (!string.IsNullOrEmpty(reqLine.TaskCode))
                        {
                            dtoLine.Task = new CommonArchiveDataDTO();
                            dtoLine.Task.Code = reqLine.TaskCode;

                            dtoSubLine.Task = new CommonArchiveDataDTO();
                            dtoSubLine.Task.Code = reqLine.TaskCode;
                        }

                        dtoSubLine.TransOutOrg = new CommonArchiveDataDTO();
                        dtoSubLine.TransOutOrg.Code = reqLine.OutOrg;
                        dtoSubLine.TransOutOwnerOrg = new CommonArchiveDataDTO();
                        dtoSubLine.TransOutOwnerOrg.Code = reqLine.OutOrg;
                        dtoSubLine.TransOutWh = new CommonArchiveDataDTO();
                        dtoSubLine.TransOutWh.Code = reqLine.FromWHCode;

                        //批号赋值
                        if (!string.IsNullOrEmpty(reqLine.LotCode)
                             && CommonUtil.IsNeedLot(reqLine.ItemCode, reqLine.OutOrg)
                            )
                        {
                            //调出批号
                            LotMaster lotOut = CommonUtil.GetLot(reqLine.LotCode, reqLine.ItemCode, reqLine.FromWHCode, LotSnStatusEnum.I_TransferOut.Value, false, reqLine.OutOrg);
                            if (lotOut == null)
                            {
                                return JsonUtil.GetFailResponse(reqLine.LotCode + U9Contant.NoFindLot, debugInfo);
                            }
                            else
                            {
                                dtoSubLine.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfo();
                                dtoSubLine.LotInfo.LotCode = reqLine.LotCode;
                                dtoSubLine.LotInfo.LotMaster = new UFIDA.U9.Base.PropertyTypes.BizEntityKey();
                                dtoSubLine.LotInfo.LotMaster.EntityID = lotOut.ID;
                            }

                            //调入批号
                            LotMaster lotIn;
                            if (Context.LoginOrg.Code.Equals(reqLine.OutOrg))
                            {
                                lotIn = lotOut;
                            }
                            else
                            {
                                lotIn = CommonUtil.GetLot(reqLine.LotCode, reqLine.ItemCode, reqLine.ToWHCode, LotSnStatusEnum.I_TransferIn.Value);
                            }
                            if (lotIn != null)
                            {
                                dtoLine.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfo();
                                dtoLine.LotInfo.LotCode = reqLine.LotCode;
                                dtoLine.LotInfo.LotMaster = new UFIDA.U9.Base.PropertyTypes.BizEntityKey();
                                dtoLine.LotInfo.LotMaster.EntityID = lotIn.ID;
                            }
                        }
                        dtoSubLine.SysState = ObjectState.Inserted;
                        dtoSubLine.StorageType = dtoLine.StorageType;
                        dtoLine.TransInSubLines.Add(dtoSubLine);//加载子行
                        dtoHeader.TransInLines.Add(dtoLine);//加载行         
                    }
                    client.TransferInDTOList.Add(dtoHeader);
                    List<CommonArchiveDataDTO> listRes = client.Do();
                    if (listRes == null || listRes.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("listRes == null || listRes.Count == 0", debugInfo);
                    }
                    UFIDA.U9.InvDoc.TransferIn.TransferIn transferIn = UFIDA.U9.InvDoc.TransferIn.TransferIn.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, listRes[0].Code));
                    if (transferIn == null)
                    {
                        return JsonUtil.GetFailResponse("transferIn == null", debugInfo);
                    }
                    using (ISession session = Session.Open())
                    {
                        transferIn.DocNo = reqHeader.WmsDocNo;
                        transferIn.Memo = reqHeader.Remark;
                        transferIn.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);

                        session.Commit();
                    }
                    //CommonArchiveDataDTO curDto = listRes[0];
                    if (reqHeader.IsAutoApprove)
                    {
                        U9Api.CustSV.ApproveTransferInCustSV sv2 = new
                          ApproveTransferInCustSV();
                        CommonApproveRequest req = new CommonApproveRequest();
                        req.DocNo = transferIn.DocNo;
                        req.BusinessDate = reqHeader.BusinessDate;
                        sv2.JsonRequest = JsonUtil.GetJsonString(req);
                        res.Clear();
                        res.Append(sv2.Do());
                        scope.Commit();
                        return res.ToString();
                    }

                    scope.Commit();
                    return JsonUtil.GetSuccessResponse(transferIn.DocNo, "", debugInfo);
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
	}

	#endregion
	
	
}