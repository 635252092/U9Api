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
    using UFIDA.U9.InvDoc.TransferOut;
    using UFIDA.U9.Lot;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Business;
    using UFSoft.UBF.Util.Context;

    /// <summary>
    /// CreateTransferOutCustSV partial 
    /// </summary>	
    public partial class CreateTransferOutCustSV
    {
        internal BaseStrategy Select()
        {
            return new CreateTransferOutCustSVImpementStrategy();
        }
    }

    #region  implement strategy	
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateTransferOutCustSVImpementStrategy : BaseStrategy
    {
        public CreateTransferOutCustSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateTransferOutCustSV bpObj = (CreateTransferOutCustSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateTransferOutRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateTransferOutRequest>(bpObj.JsonRequest);
            }
            catch (Exception ex)
            {
                return JsonUtil.GetFailResponse("JSON格式错误");
            }

            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

            //Context. = DateTime.Now;

            try
            {
                #region 创建调出单

                UFIDA.U9.ISV.TransferOutISV.IC_TransferOutDTO _docHead = new UFIDA.U9.ISV.TransferOutISV.IC_TransferOutDTO();
                _docHead.TransOutDocType = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                _docHead.TransOutDocType.Code = reqHeader.DocTypeCode;
                //_docHead.TransOutDocType.Code = "TransOut002";

                //_docHead.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
                DateTime businessDate = DateTime.Parse(reqHeader.BusinessDate);
                if (businessDate > DateTime.Now)
                {
                    _docHead.BusinessDate = DateTime.Now;
                }
                else
                {
                    _docHead.BusinessDate = businessDate;
                }

                _docHead.ExpectArriveDate = businessDate; 
                _docHead.LatestArriveDate = businessDate;
                _docHead.LatestShippingDate = businessDate;
                _docHead.PlanShippingDate = businessDate;

                _docHead.Org = new CommonArchiveDataDTO();
                _docHead.Org.ID = Context.LoginOrg.ID;
                _docHead.Status = Status.Opening;
                _docHead.TransferDirection = UFIDA.U9.InvDoc.Enums.TransferDirectionEnum.Usual;
                //_docHead.SOBAccountPeriod = SVPublicExtend.GetSOBAccountPeriod(_docHead.BusinessDate);
                _docHead.CreatedBy = Context.LoginUser;
                _docHead.CreatedOn = _docHead.BusinessDate;
                _docHead.TransOutLines = new List<UFIDA.U9.ISV.TransferOutISV.IC_TransOutLineDTO>();
                _docHead.IsAutoPushTask = true;
                _docHead.Memo = reqHeader.Remark;
                _docHead.DocNo = reqHeader.WmsDocNo;
                foreach (var reqLine in reqHeader.TransferOutLines)
                {
                    #region 调出明细
                    UFIDA.U9.ISV.TransferOutISV.IC_TransOutLineDTO dtoLine = new UFIDA.U9.ISV.TransferOutISV.IC_TransOutLineDTO();
              
                    dtoLine.CreatedBy = Context.LoginUser;
                    dtoLine.CreatedOn = businessDate;
                    
                    dtoLine.TransOutOwnerOrg = new CommonArchiveDataDTO();
                    dtoLine.TransOutOwnerOrg.Code = Context.LoginOrg.Code;
                    dtoLine.TransOutWh = new CommonArchiveDataDTO();
                    dtoLine.TransOutWh.Code = reqLine.FromWHCode;
                    dtoLine.Item_Info = new UFIDA.U9.CBO.SCM.Item.ItemInfo();
                    dtoLine.Item_Info.ItemCode = reqLine.ItemCode;

                    dtoLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;

                    dtoLine.StoreUOMQty = reqLine.ItemQty;
                    dtoLine.CostUOMQty = reqLine.ItemQty;


                    #endregion

                    #region 调入信息
                    dtoLine.TransOutSubLines = new List<UFIDA.U9.ISV.TransferOutISV.IC_TransOutSubLineDTO>();

                    UFIDA.U9.ISV.TransferOutISV.IC_TransOutSubLineDTO dtoSubLine = new UFIDA.U9.ISV.TransferOutISV.IC_TransOutSubLineDTO();
              
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
                    }

                    //批号赋值
                    if (!string.IsNullOrEmpty(reqLine.LotCode) && CommonUtil.IsNeedLot(reqLine.ItemCode, Context.LoginOrg.ID))
                    {
                        //调出批号
                        LotMaster lotOut = CommonUtil.GetLot(reqLine.LotCode, reqLine.ItemCode, reqLine.FromWHCode, LotSnStatusEnum.I_TransferOut.Value, false);
                        if (lotOut == null)
                        {
                            return JsonUtil.GetFailResponse(reqLine.LotCode + U9Contant.NoFindLot, debugInfo);
                        }
                        else
                        {
                            dtoLine.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfo();
                            dtoLine.LotInfo.LotCode = reqLine.LotCode;
                            dtoLine.LotInfo.LotMaster = new UFIDA.U9.Base.PropertyTypes.BizEntityKey();
                            dtoLine.LotInfo.LotMaster.EntityID = lotOut.ID;
                        }

                        //调入批号
                        LotMaster lotIn;
                        if (Context.LoginOrg.Code.Equals(reqLine.InOrg))
                        {
                            lotIn = lotOut;
                        }
                        else
                        {
                            lotIn = CommonUtil.GetLot(reqLine.LotCode, reqLine.ItemCode, reqLine.ToWHCode, LotSnStatusEnum.I_TransferIn.Value, true, reqLine.InOrg);
                        }
                        if (lotIn != null)
                        {
                            dtoSubLine.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfo();
                            dtoSubLine.LotInfo.LotCode = reqLine.LotCode;
                            dtoSubLine.LotInfo.LotMaster = new UFIDA.U9.Base.PropertyTypes.BizEntityKey();
                            dtoSubLine.LotInfo.LotMaster.EntityID = lotIn.ID;
                        }
                    }

                    dtoSubLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                    dtoSubLine.TransInOrg = new CommonArchiveDataDTO();
                    //dtoSubLine.TransInOrg.Code = Context.LoginOrg.Code;
                    dtoSubLine.TransInOrg.Code = reqLine.InOrg;
                    dtoSubLine.TransInWh = new CommonArchiveDataDTO();
                    dtoSubLine.TransInWh.Code = reqLine.ToWHCode;
                    dtoSubLine.TransInOwnerOrg = new CommonArchiveDataDTO();
                    dtoSubLine.TransInOwnerOrg.Code = reqLine.InOrg;
                    dtoSubLine.StorageType = dtoLine.StorageType;
                    dtoSubLine.TransOutSUQty = reqLine.ItemQty;
                    dtoSubLine.CostUOMQty = reqLine.ItemQty;
                    dtoSubLine.SUToCURate = 1;

                    dtoSubLine.Project = dtoLine.Project;
                    dtoSubLine.Task = dtoLine.Task;
                    dtoSubLine.Seiban = dtoLine.Seiban;
                    dtoSubLine.PlanRcvDate = businessDate;
                    dtoSubLine.CreatedOn = businessDate;
                    dtoLine.TransOutSubLines.Add(dtoSubLine);
                    #endregion
                    //库位字表
                    dtoLine.TransOutBins = new List<UFIDA.U9.ISV.TransferOutISV.IC_TransOutBinDTO>();
                    UFIDA.U9.ISV.TransferOutISV.IC_TransOutBinDTO transOutBin = new UFIDA.U9.ISV.TransferOutISV.IC_TransOutBinDTO();
                    //transOutBin.BinInfo = new UFIDA.U9.CBO.SCM.Bin.BinInfo();
                    //transOutBin.BinInfo.Bin = new UFIDA.U9.CBO.SCM.Bin.Bin();
                    //transOutBin.BinInfo.

                    transOutBin.CreatedOn = businessDate;
                    transOutBin.StoreUOMQty = reqLine.ItemQty;
                    transOutBin.CreatedBy = Context.LoginUser;
                    transOutBin.CreatedOn = _docHead.CreatedOn;
                    //transOutBin.OperationType = UFIDA.U9.CBO.SCM.Enums.BusinessOperatorTypeEnum.NoChange;
                    //transOutBin.SetValue("TimeForPickOrTally", _docHead.BusinessDate);
                    dtoLine.TransOutBins.Add(transOutBin);

                    _docHead.TransOutLines.Add(dtoLine);
                }

                List<UFIDA.U9.ISV.TransferOutISV.IC_TransferOutDTOData> _TransInDocList = new List<UFIDA.U9.ISV.TransferOutISV.IC_TransferOutDTOData>();
                _TransInDocList.Add(_docHead.ToEntityData());
                #endregion
                UFIDA.U9.ISV.TransferOutISV.Proxy.CreateTransferOutSRVProxy _proxy = new UFIDA.U9.ISV.TransferOutISV.Proxy.CreateTransferOutSRVProxy();
               
                _proxy.TransferOutDOCList = _TransInDocList;
                List<CommonArchiveDataDTOData> listRes = _proxy.Do();

                if (listRes == null || listRes.Count == 0)
                {
                    return JsonUtil.GetFailResponse("listRes == null || listRes.Count == 0", debugInfo);
                }
                CommonArchiveDataDTOData curDto = listRes[0];

                if (businessDate > DateTime.Now)
                {
                    using (UFSoft.UBF.Business.Session session = UFSoft.UBF.Business.Session.Open())
                    {

                        UFIDA.U9.InvDoc.TransferOut.TransferOut transferOut = UFIDA.U9.InvDoc.TransferOut.TransferOut.Finder.Find(string.Concat(new object[]
                            {
                                " Org = ",
                                Context.LoginOrg.ID,
                                " and DocNo = '",
                                curDto.Code,
                                "'"
                            }), null);

                        transferOut.Status = UFIDA.U9.InvDoc.TransferOut.Status.Approving;

                        transferOut.CurrAction = UFIDA.U9.InvDoc.TransferOut.TransferOutActionEnum.UIUpdate;
                        UFIDA.U9.Base.SOB.SOBAccountingPeriod sOBAccountingPeriod = CommonUtil.GetSOBAccountPeriodEntity(businessDate, Context.LoginOrg.ID);
                        transferOut.SOBAccountPeriod = sOBAccountingPeriod;
                        foreach (var item in transferOut.TransOutAccountPeriods)
                        {
                            item.SOBAccountPeriod = sOBAccountingPeriod;
                        }
                        transferOut.BusinessDate = businessDate;
                        foreach (var transOutLine in transferOut.TransOutLines)
                        {
                            foreach (var transOutSubLine in transOutLine.TransOutSubLines)
                            {
                                transOutSubLine.PlanRcvDate = businessDate;
                                transOutSubLine.PlanSendDate = businessDate;
                            }
                            foreach (var transOutBin in transOutLine.TransOutBins)
                            {
                                transOutBin.TimeForPickOrTally = businessDate;
                            }
                        }
                        session.Commit();
                    }
                }

                if (reqHeader.IsAutoApprove)
                {
                    U9Api.CustSV.ApproveTransferOutCustSV sv2 = new ApproveTransferOutCustSV();
                    CommonApproveRequest req = new CommonApproveRequest();
                    req.DocNo = curDto.Code;
                    req.BusinessDate = reqHeader.BusinessDate;
                    sv2.JsonRequest = JsonUtil.GetJsonString(req);
                    return sv2.Do();
                }

                return JsonUtil.GetSuccessResponse(curDto.Code, "", debugInfo);
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