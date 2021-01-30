namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using U9Api.CustSV.Base;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.SM.SO;
    using System.Linq;
    using UFIDA.U9.ISV.SM;
    using UFSoft.UBF.Business;
    using UFIDA.U9.Lot;
    using UFIDA.U9.Base;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.PM.Rcv;
    using UFIDA.U9.PM.DTOs;
    using UFIDA.U9.CBO.DTOs;
    using UFIDA.U9.CBO.Enums;

    /// <summary>
    /// CreateRcvFromRMRSV partial 
    /// </summary>	
    public partial class CreateRcvFromRMRSV
    {
        internal BaseStrategy Select()
        {
            return new CreateRcvFromRMRSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateRcvFromRMRSVImpementStrategy : BaseStrategy
    {
        public CreateRcvFromRMRSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateRcvFromRMRSV bpObj = (CreateRcvFromRMRSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateRcvFromRMRRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateRcvFromRMRRequest>(bpObj.JsonRequest);
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
                //Dictionary<long, CreateRcvFromRMRRequest.CreateRcvFromRMRLine> dic = new Dictionary<long, CreateRcvFromRMRRequest.CreateRcvFromRMRLine>();
                List<UFIDA.U9.SM.RMR.RMRLineAndQtyDTO> listRMRLineAndQtyDTO = new List<UFIDA.U9.SM.RMR.RMRLineAndQtyDTO>();

                foreach (var reqLine in reqHeader.CreateRcvFromRMRLines)
                {
                    UFIDA.U9.SM.RMR.RMR rmr = UFIDA.U9.SM.RMR.RMR.Finder.Find(string.Format("Org={0} and RMRHead.DocNo='{1}' and RMRNo={2}", Context.LoginOrg.ID, reqHeader.SrcDocNo, reqLine.SrcLineNo));

                    if (rmr == null)
                    {
                        return JsonUtil.GetFailResponse(String.Format("创建【销售退货收货】时，未根据来源单号{0}找到来源【退回申请】", reqHeader.SrcDocNo), debugInfo);
                    }
                    if (rmr.Status != UFIDA.U9.SM.RMR.RMRStatusEnum.Approved)
                    {
                        debugInfo.AppendLine("行不是审核状态");
                        errorInfo.AppendLine("行不是审核状态");
                        continue;
                    }

                    foreach (var current in rmr.RMRLines)
                    {
                        if (reqLine.SrcSubLineNo != current.DocLineNo)
                        {
                            continue;
                        }
                        UFIDA.U9.SM.RMR.RMRLineAndQtyDTO rMRLineAndQtyDTOData = new UFIDA.U9.SM.RMR.RMRLineAndQtyDTO();
                        rMRLineAndQtyDTOData.RMRLineID = current.ID;
                        rMRLineAndQtyDTOData.TransformDate = DateTime.Parse(reqHeader.BusinessDate);
                        rMRLineAndQtyDTOData.ArriveTime = DateTime.Parse(reqHeader.BusinessDate);
                        //U9Api.CustSV.Model.Request.CreateRcvFromRMRRequest.CreateRcvFromRMRLine curLine = reqHeader.CreateRcvFromRMRLines.Where(a => a.SrcLineNo == current.DocLineNo).FirstOrDefault();
                        //if (curLine == null)
                        //{
                        //    return JsonUtil.GetFailResponse(String.Format("创建【销售退货收货】时，未根据来源单号{0},行号{1}找到来源【退回申请行】", reqHeader.SrcDocNo, current.DocLineNo), debugInfo);
                        //}

                        //dic.Add(current.ID, reqLine);
                        DoubleQuantity doubleQuantity = new DoubleQuantity(reqLine.RcvQty, Decimal.Zero, current.UomInfoDTO1, current.UomInfoDTO1);


                        rMRLineAndQtyDTOData.RcvQty = doubleQuantity;
                        listRMRLineAndQtyDTO.Add(rMRLineAndQtyDTOData);
                    }
                }

                if (listRMRLineAndQtyDTO == null || listRMRLineAndQtyDTO.Count == 0)
                {
                    return JsonUtil.GetFailResponse("没有找到对应的【退回申请单子行】，请检查入参;"+ errorInfo.ToString(), debugInfo);
                }
                UFIDA.U9.SM.RMR.GetRcvDTOsByRMR getRcvDTOsByRMRProxy = new UFIDA.U9.SM.RMR.GetRcvDTOsByRMR();
                getRcvDTOsByRMRProxy.RMRLineAndQtys = listRMRLineAndQtyDTO;
                getRcvDTOsByRMRProxy.SplitRules = new List<string>();
                UFIDA.U9.SM.RMR.RcvFromRMRDTO rcvFromRMRDTO = getRcvDTOsByRMRProxy.Do();

                List<RcvHeadDTOData> listRcvHeadDTOData = (List<RcvHeadDTOData>)rcvFromRMRDTO.RcvHeadDatas;
                if (listRcvHeadDTOData == null || listRcvHeadDTOData.Count == 0)
                {
                    return JsonUtil.GetFailResponse("listRcvHeadDTOData == null || listRcvHeadDTOData.Count == 0", debugInfo);
                }
                else if (listRcvHeadDTOData.Count > 1)
                {
                    return JsonUtil.GetFailResponse(U9Contant.RcvCountOver, debugInfo);
                }
                RcvHeadDTOData rcvHeadDTOData = listRcvHeadDTOData[0];
                Dictionary<string, Warehouse> dicWH = new Dictionary<string, Warehouse>();

                HashSet<CreateRcvFromRMRRequest.CreateRcvFromRMRLine> hs2 = new HashSet<CreateRcvFromRMRRequest.CreateRcvFromRMRLine>();

                foreach (var current2 in rcvHeadDTOData.RcvLineDTOs)
                {
                    CreateRcvFromRMRRequest.CreateRcvFromRMRLine reqLine = reqHeader.CreateRcvFromRMRLines.Where(a => a.ItemCode == current2.RcvLine.ItemInfo.ItemCode
                             && a.SrcLineNo == current2.RcvLine.SrcDoc.SrcDocLineNo
                             && a.SrcSubLineNo == current2.RcvLine.SrcDoc.SrcDocSubLineNo
                             && !hs2.Contains(a)
                             ).FirstOrDefault();

                    //U9Api.CustSV.Model.Request.CreateRcvFromRMRRequest.CreateRcvFromRMRLine reqLine = dic[current2.RcvLine.SrcDoc.SrcDocSubLine.EntityID];
                    if (reqLine == null)
                    {
                        return JsonUtil.GetFailResponse("reqLine == null", debugInfo);
                    }
                    hs2.Add(reqLine);
                    Warehouse warehouse = null;
                    if (dicWH.Keys.Contains(reqLine.WHCode))
                    {
                        dicWH.TryGetValue(reqLine.WHCode, out warehouse);
                    }
                    else
                    {
                        warehouse = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);
                        dicWH.Add(reqLine.WHCode, warehouse);
                    }
                    if (warehouse == null)
                    {
                        return JsonUtil.GetFailResponse(reqLine.WHCode+U9Contant.NoFindWH , debugInfo);
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
                    Receivement r = Receivement.Finder.FindByID(item.ID);
                    if (r == null)
                    {
                        return JsonUtil.GetFailResponse("r == null");
                    }
                    using (ISession session = Session.Open())
                    {
                        if (!string.IsNullOrEmpty(reqHeader.RcvDocNo))
                        {
                            r.DocNo = reqHeader.RcvDocNo;
                        }
                        res.Append(r.DocNo);
                        r.Memo = reqHeader.Remark;
                        //修改批号
                        HashSet<CreateRcvFromRMRRequest.CreateRcvFromRMRLine> hs = new HashSet<CreateRcvFromRMRRequest.CreateRcvFromRMRLine>();
                        foreach (var rcvLine in r.RcvLines)
                        {
                            rcvLine.ConfirmDate = r.BusinessDate;
                            CreateRcvFromRMRRequest.CreateRcvFromRMRLine curLine = reqHeader.CreateRcvFromRMRLines.Where(a => a.ItemCode == rcvLine.ItemInfo.ItemCode 
                            && a.SrcLineNo == rcvLine.SrcDoc.SrcDocLineNo 
                            && a.SrcSubLineNo == rcvLine.SrcDoc.SrcDocSubLineNo
                            &&!hs.Contains(a)
                            ).FirstOrDefault();
                            rcvLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                            if (curLine != null && !string.IsNullOrEmpty(curLine.LotCode)
                               && CommonUtil.IsNeedLot(rcvLine.ItemInfo.ItemCode, Context.LoginOrg.ID))
                            {
                                hs.Add(curLine);
                                rcvLine.InvLot = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.S_ReturnInbound.Value);
                                rcvLine.InvLotCode = curLine.LotCode;
                                debugInfo.AppendLine("批号赋值end");
                            }

                        }
                        session.Commit();
                    }
                    if (reqHeader.IsAutoApprove)
                    {
                        RcvApproveSV sv2 = new RcvApproveSV();
                        RcvApproveRequest shipApproveRequest = new RcvApproveRequest();
                        shipApproveRequest.DocNo = r.DocNo;
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