namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using UFIDA.U9.PM.Rcv;
    using UFIDA.U9.PM.PO;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.Base;
    using UFIDA.U9.PM.DTOs;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFSoft.UBF.Business;
    using System.Linq;
    using UFIDA.U9.Lot;
    using UFIDA.U9.CBO.Enums;
    using U9Api.CustSV.Base;

    /// <summary>
    /// CreateRcvFromPOSV partial 
    /// </summary>	
    public partial class CreateRcvFromPOSV
    {
        internal BaseStrategy Select()
        {
            return new CreateRcvFromPOSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateRcvFromPOSVImpementStrategy : BaseStrategy
    {
        public CreateRcvFromPOSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateRcvFromPOSV bpObj = (CreateRcvFromPOSV)obj;
            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateRcvFromPORequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateRcvFromPORequest>(bpObj.JsonRequest);
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
                List<RcvFromPODTO> list = new List<RcvFromPODTO>();
                POToRcvSV pOToRcvSVProxy = new POToRcvSV();
                pOToRcvSVProxy.IsWholeAllot = false;
                pOToRcvSVProxy.OptionalSplitTerm = new List<string>();
                pOToRcvSVProxy.TimeInterval = 0;
                List<POToRcvDTO> listPOToRcvDTO = new List<POToRcvDTO>();
                //Dictionary<long, CreateRcvFromPORequest.CreateRcvFromPOLine> dic = new Dictionary<long, CreateRcvFromPORequest.CreateRcvFromPOLine>();
                foreach (var reqLine in reqHeader.CreateRcvFromPOLines)
                {
                    //查找行订单
                    POLine poLine = POLine.Finder.Find(string.Format("PurchaseOrder.Org={0} and PurchaseOrder.DocNo='{1}' and DocLineNo='{2}'", Context.LoginOrg.ID, reqLine.SrcDocNo, reqLine.SrcLineNo));
                    if (poLine == null)
                    {
                        return JsonUtil.GetFailResponse(String.Format("创建收货单时，未根据来源单号{0}找到来源采购订单行", reqLine.SrcDocNo), debugInfo);
                    }
                    foreach (var poShipLine in poLine.POShiplines)
                    {
                        if (reqLine.SrcSubLineNo != poShipLine.SubLineNo)
                        {
                            continue;
                        }
                        if (poShipLine.Status != PODOCStatusEnum.Approved)
                        {
                            debugInfo.AppendLine("行不是审核状态");
                            errorInfo.AppendLine("行不是审核状态");
                            continue;
                        }
                        debugInfo.AppendLine("遍历行");

                        POToRcvDTO pOToRcvDTOData = new POToRcvDTO();
                        pOToRcvDTOData.SrcDocLineKey = poShipLine.Key;
                        UFIDA.U9.CBO.DTOs.UOMInfoDTO uom1 = new UFIDA.U9.CBO.DTOs.UOMInfoDTO(poShipLine.TradeUOM.Key, poShipLine.TradeUOM.Key, 0);
                        pOToRcvDTOData.RcvQty = new UFIDA.U9.CBO.DTOs.DoubleQuantity(reqLine.RcvQty, Decimal.Zero, uom1, uom1);
                        pOToRcvDTOData.RcvBussinessDate = DateTime.Parse(reqHeader.BusinessDate);
                        pOToRcvDTOData.ArrivedTime = pOToRcvDTOData.RcvBussinessDate;
                        listPOToRcvDTO.Add(pOToRcvDTOData);
                        //dic.Add(poShipLine.ID, reqLine);
                    }
                }
                if (listPOToRcvDTO == null || listPOToRcvDTO.Count == 0)
                {
                    return JsonUtil.GetFailResponse(errorInfo.ToString(), debugInfo);
                }
                pOToRcvSVProxy.POToRcvDTO = listPOToRcvDTO;
                //pOToRcvSVProxy.RcvDocType = UFIDA.U9.PM.Pub.RcvDocType.FindByCode( Context.LoginOrg,reqHeader.RcvDocType).Key;

                RcvFromPODTO rcvFromPODTO = pOToRcvSVProxy.Do();
                if (rcvFromPODTO == null || rcvFromPODTO.RcvDTOs == null || rcvFromPODTO.RcvDTOs.Count == 0)
                {
                    return JsonUtil.GetFailResponse("rcvFromPODTO == null || rcvFromPODTO.RcvDTOs == null || rcvFromPODTO.RcvDTOs.Count == 0", debugInfo);
                }
                else if (rcvFromPODTO.RcvDTOs.Count > 1)
                {
                    return JsonUtil.GetFailResponse(U9Contant.RcvCountOver, debugInfo);
                }

                RcvHeadDTOData rcvHeadDTOData = (RcvHeadDTOData)rcvFromPODTO.RcvDTOs[0];

                HashSet<CreateRcvFromPORequest.CreateRcvFromPOLine> hs2 = new HashSet<CreateRcvFromPORequest.CreateRcvFromPOLine>();
                  
                    foreach (var current2 in rcvHeadDTOData.RcvLineDTOs)
                {
                    //U9Api.CustSV.Model.Request.CreateRcvFromPORequest.CreateRcvFromPOLine reqLine = dic[current2.RcvLine.SrcDoc.SrcDocSubLine.EntityID];

                    CreateRcvFromPORequest.CreateRcvFromPOLine reqLine = reqHeader.CreateRcvFromPOLines.Where(a => a.ItemCode == current2.RcvLine.ItemInfo.ItemCode
                    && a.SrcLineNo == current2.RcvLine.SrcDoc.SrcDocLineNo
                    && a.SrcSubLineNo == current2.RcvLine.SrcDoc.SrcDocSubLineNo
                    && !hs2.Contains(a)
                    ).FirstOrDefault();

                    if (reqLine == null)
                    {
                        return JsonUtil.GetFailResponse("reqLine == null", debugInfo);
                    }
                    hs2.Add(reqLine);
                    Warehouse warehouse = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);

                    if (warehouse == null)
                    {
                        return JsonUtil.GetFailResponse(reqLine.WHCode+U9Contant.NoFindWH, debugInfo);
                    }
                    current2.RcvLine.Wh = warehouse.Key.ID;
                    UFIDA.U9.CBO.HR.Operator.Operators wHMan = warehouse.Manager;
                    if (wHMan == null)
                    {
                        return JsonUtil.GetFailResponse(U9Contant.NoFindWHMan, debugInfo);

                    }
                    current2.RcvLine.WhMan = wHMan.Key.ID;
                    UFIDA.U9.CBO.HR.Department.Department rcvDept = warehouse.Department;
                    if (rcvDept == null)
                    {
                        return JsonUtil.GetFailResponse(U9Contant.NoFindWHDept, debugInfo);
                    }
                    current2.RcvLine.RcvDept = rcvDept.Key.ID;
                }
                UFIDA.U9.PM.Rcv.Proxy.CreateReceivementProxy createReceivementProxy = new UFIDA.U9.PM.Rcv.Proxy.CreateReceivementProxy();
                //RcvHeadDTO l = new RcvHeadDTO();
                //l.FromEntityData(rcvHeadDTOData);
                List<RcvHeadDTOData> listRcvHeadDTO = new List<RcvHeadDTOData>();
                rcvHeadDTOData.RcvHead.Memo = reqHeader.Remark;
                listRcvHeadDTO.Add(rcvHeadDTOData);
                createReceivementProxy.RcvHeadDTOs = listRcvHeadDTO;
                List<BusinessEntity.EntityKey> list2 = createReceivementProxy.Do();
                if (list2 == null || list2.Count == 0)
                {
                    return JsonUtil.GetFailResponse("创建失败");
                }
                foreach (var item in list2)
                {
                    Receivement receivement = Receivement.Finder.FindByID(item.ID);
                    if (receivement == null)
                    {
                        return JsonUtil.GetFailResponse("r == null");
                    }
                    using (ISession session = Session.Open())
                    {
                        //修改批号
                        HashSet<CreateRcvFromPORequest.CreateRcvFromPOLine> hs = new HashSet<CreateRcvFromPORequest.CreateRcvFromPOLine>();
                        foreach (var rcvLine in receivement.RcvLines)
                        {
                            rcvLine.ConfirmDate = receivement.BusinessDate;
                            rcvLine.EyeballedTime = receivement.BusinessDate;
                            rcvLine.ArrivedTime = receivement.BusinessDate;
                            CreateRcvFromPORequest.CreateRcvFromPOLine curLine = reqHeader.CreateRcvFromPOLines.Where(a => a.ItemCode == rcvLine.ItemInfo.ItemCode 
                            && a.SrcLineNo == rcvLine.SrcDoc.SrcDocLineNo
                            &&a.SrcSubLineNo==rcvLine.SrcDoc.SrcDocSubLineNo
                            &&!hs.Contains(a)
                            ).FirstOrDefault();
                            if (curLine != null && !string.IsNullOrEmpty(curLine.LotCode) && CommonUtil.IsNeedLot(rcvLine.ItemInfo.ItemCode, Context.LoginOrg.ID))
                            {
                                hs.Add(curLine);
                                LogUtil.WriteDebugInfoLog(rcvLine.ItemInfo.ItemCode + " " + curLine.LotCode);
                                LotMaster lotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.P_PurTransferIn.Value);
                                lotMaster.DescFlexSegments.PrivateDescSeg1 = curLine.TotalLotQty;
                                rcvLine.InvLot = lotMaster;
                                rcvLine.InvLotCode = curLine.LotCode;
                            }
                            //rcvLine.QCQCConclusion =  UFIDA.U9.CBO.QC.Enums.QCResultEnum.Eligibility;
                            rcvLine.QCConclusion = UFIDA.U9.CBO.SCM.Enums.QCConclusionEnum.Qualified;
                            rcvLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                        }
                        if (!string.IsNullOrEmpty(reqHeader.RcvDocNo))
                        {
                            receivement.DocNo = reqHeader.RcvDocNo;
                        }

                        res.Append(receivement.DocNo);
                        receivement.Memo = reqHeader.Remark;

                        session.Commit();
                    }
                    if (reqHeader.IsAutoApprove)
                    {
                        RcvApproveSV sv2 = new RcvApproveSV();
                        RcvApproveRequest shipApproveRequest = new RcvApproveRequest();
                        shipApproveRequest.DocNo = receivement.DocNo;
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