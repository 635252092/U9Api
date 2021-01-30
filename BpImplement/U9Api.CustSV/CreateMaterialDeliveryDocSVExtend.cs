namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.Base;
    using UFIDA.U9.CBO.Pub.Controller;
    using UFIDA.U9.ISV.MO;
    using UFSoft.UBF.AopFrame;
    using UFIDA.U9.MO.MO;
    using UFIDA.U9.IssueNew.MaterialDeliveryBP;
    using UFIDA.U9.IssueNew.IssueApplyBE;
    using UFIDA.U9.IssueNew.MaterialDeliveryDocBE;
    using UFSoft.UBF.Business;
    using System.Linq;
    using UFIDA.U9.CBO.Enums;
    using U9Api.CustSV.Base;

    /// <summary>
    /// CreateMaterialDeliveryDocSV partial 
    /// </summary>	
    public partial class CreateMaterialDeliveryDocSV
    {
        internal BaseStrategy Select()
        {
            return new CreateMaterialDeliveryDocSVImpementStrategy();
        }
    }

    #region  implement strategy	
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateMaterialDeliveryDocSVImpementStrategy : BaseStrategy
    {
        public CreateMaterialDeliveryDocSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateMaterialDeliveryDocSV bpObj = (CreateMaterialDeliveryDocSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateMaterialDeliveryDocRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateMaterialDeliveryDocRequest>(bpObj.JsonRequest);
                if (reqHeader == null)
                {
                    throw new Exception("request == null");
                }
            }
            catch (Exception ex)
            {
                return JsonUtil.GetFailResponse("JSON格式错误;" + ex.Message);
            }
            string res = "";
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

            using (UFSoft.UBF.Transactions.UBFTransactionScope scope = new UFSoft.UBF.Transactions.UBFTransactionScope(UFSoft.UBF.Transactions.TransactionOption.RequiresNew))
            {
                try
                {
                    List<MaterialInfo> list = new List<MaterialInfo>();
                    foreach (var reqLine in reqHeader.MOPickLines)
                    {
                        UFIDA.U9.MO.MO.MOPickList _srcPlist = MOPickList.Finder.Find(string.Format("MO.Org={0} and MO.DocNo='{1}' and DocLineNO={2}", Context.LoginOrg.ID, reqLine.SrcDocNo, reqLine.SrcLineNo));

                        if (_srcPlist == null)
                        {
                            return JsonUtil.GetFailResponse("未能获取到订单:" + reqLine.SrcDocNo + " 行号:" + reqLine.SrcDocNo + " 备料信息！", debugInfo);
                        }

                        if (_srcPlist.MO.TotalStartQty == 0)
                        {
                            return JsonUtil.GetFailResponse("工单:" + reqLine.SrcDocNo + " 未开工,不能创建领料单", debugInfo);
                        }

                        MaterialInfo materialInfoData = new MaterialInfo();
                        materialInfoData.MOPick = _srcPlist.Key;
                        materialInfoData.IssuedQty = reqLine.ItemQty;
                        //materialInfoData.DemandQty = _srcPlist.ActualReqQty;
                        materialInfoData.DemandQty = reqLine.ItemQty;
                        materialInfoData.SupplyWh = UFIDA.U9.CBO.SCM.Warehouse.Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode).Key;
                        materialInfoData.DemandOrg = Context.LoginOrg.Key;
                        list.Add(materialInfoData);
                    }
                    DrawMaterialOutAndIn drawMaterialOutAndInProxy = new DrawMaterialOutAndIn();
                    drawMaterialOutAndInProxy.SplitCondition = new UFIDA.U9.IssueNew.IssueBP.BatchIssueApplySplitCond();
                    UFIDA.U9.Issue.MaterialDeliveryDocTypeBE.MaterialDeliveryDocType docType = UFIDA.U9.Issue.MaterialDeliveryDocTypeBE.MaterialDeliveryDocType.Finder.Find(string.Format("Org={0} and Code='{1}'", Context.LoginOrg.ID, "1"));
                    if (docType == null)
                    {
                        return JsonUtil.GetFailResponse("MaterialDeliveryDocType" + U9Contant.NoFindDocType, debugInfo);
                    }
                    drawMaterialOutAndInProxy.MaterialDeliveryDocType = docType.Key;
                    drawMaterialOutAndInProxy.BatchMaterialOutDTOList = new List<MaterialInfo>();
                    drawMaterialOutAndInProxy.BatchMaterialOutDTOList = list;
                    drawMaterialOutAndInProxy.IsPush = true;
                    drawMaterialOutAndInProxy.IsIssue = true;
                    drawMaterialOutAndInProxy.DocDate = DateTime.Parse(reqHeader.BusinessDate);
                    List<MaterialDeliveryDoc.EntityKey> listIssueKeyDTO = drawMaterialOutAndInProxy.Do();

                    //sv.Do();
                    if (listIssueKeyDTO == null || listIssueKeyDTO.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("listIssueKeyDTO == null || listIssueKeyDTO.Count == 0", debugInfo);
                    }
                    else if (listIssueKeyDTO.Count > 1)
                    {
                        throw new Exception("不支持同时出库多张生产订单！");
                    }
                    MaterialDeliveryDoc doc = listIssueKeyDTO[0].GetEntity();

                    using (ISession session = Session.Open())
                    {
                        doc.DocNo = reqHeader.WmsDocNo;
                        doc.Memo = reqHeader.Remark;
                        doc.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
                        doc.ConfirmDate = doc.BusinessDate;
                        HashSet<CreateMaterialDeliveryDocRequest.MOPickLine> hs = new HashSet<CreateMaterialDeliveryDocRequest.MOPickLine>();
                        foreach (var docLine in doc.MaterialDeliveryDocLines)
                        {
                            CreateMaterialDeliveryDocRequest.MOPickLine curLine = reqHeader.MOPickLines.Where(a => a.SrcDocNo == docLine.SourceDoc.SrcDocNo
                            && a.SrcLineNo == Convert.ToInt32(docLine.SourceDoc.SrcDocLineNo)
                            && !hs.Contains(a)
                            ).FirstOrDefault();
                            if (curLine == null)
                            {
                                continue;
                            }

                            hs.Add(curLine);
                            docLine.SupplyWh = UFIDA.U9.CBO.SCM.Warehouse.Warehouse.FindByCode(Context.LoginOrg, curLine.WHCode);
                            if (!string.IsNullOrEmpty(curLine.LotCode) 
                                && CommonUtil.IsNeedLot(curLine.ItemCode, doc.Org.ID))
                            {
                                docLine.LotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.M_ManufactureOut.Value);
                                docLine.LotNo = curLine.LotCode;
                            }
                            docLine.StorageType = StorageTypeEnum.Useable;
                        }
                        session.Commit();
                    }
                    if (reqHeader.IsAutoApprove)
                    {
                        U9Api.CustSV.ApproveMaterialDeliveryDocSV sv3 = new
                          ApproveMaterialDeliveryDocSV();
                        CommonApproveRequest req = new CommonApproveRequest();
                        req.DocNo = doc.DocNo;
                        sv3.JsonRequest = JsonUtil.GetJsonString(req);

                        res = sv3.Do();
                    }
                    else
                    {
                        res = JsonUtil.GetSuccessResponse(doc.DocNo, doc.DocState.Name, debugInfo);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.WriteDebugInfoLog(debugInfo.ToString());
                    scope.Rollback();
                    //throw Base.U9Exception.GetInnerException(ex);
                    return JsonUtil.GetFailResponse(Base.U9Exception.GetInnerException(ex).Message, debugInfo);
                }
                scope.Commit();
                return res;
            }
        }
    }

    #endregion


}