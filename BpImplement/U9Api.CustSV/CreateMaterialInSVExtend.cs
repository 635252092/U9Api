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
    using System.Linq;
    using UFSoft.UBF.Business;
    using UFIDA.U9.CBO.Enums;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using U9Api.CustSV.Base;

    /// <summary>
    /// CreateMaterialInSV partial 
    /// </summary>	
    public partial class CreateMaterialInSV
    {
        internal BaseStrategy Select()
        {
            return new CreateMaterialInSVImpementStrategy();
        }
    }

    #region  implement strategy	
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateMaterialInSVImpementStrategy : BaseStrategy
    {
        public CreateMaterialInSVImpementStrategy() { }
        private static string GetIDList4PickList(List<string> picklist)
        {
            string text = string.Empty;
            for (int i = 0; i < picklist.Count; i++)
            {
                if (i == picklist.Count - 1)
                {
                    text += picklist[i];
                }
                else
                {
                    text = text + picklist[i] + ",";
                }
            }
            return text;
        }
        public override object Do(object obj)
        {
            CreateMaterialInSV bpObj = (CreateMaterialInSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateMaterialInRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateMaterialInRequest>(bpObj.JsonRequest);
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
                    GetConfirmInfo sv1 = new GetConfirmInfo();
                    sv1.ConditionInfo = new List<UFIDA.U9.IssueNew.MaterialDeliveryBP.ConditionInfo>();
                    UFIDA.U9.IssueNew.MaterialDeliveryBP.ConditionInfo conditionInfo = new UFIDA.U9.IssueNew.MaterialDeliveryBP.ConditionInfo();
                    conditionInfo.SourceType = 3;
                    conditionInfo.DemandOrgs = Context.LoginOrg.ID;
                    List<string> listIds = new List<string>();
                    foreach (var reqLine in reqHeader.MaterialDeliveryDocLines)
                    {
                        //UFIDA.U9.MO.MO.MOPickList _srcPlist = MOPickList.Finder.Find(string.Format("MO.Org={0} and MO.DocNo='{1}' and DocLineNO={2}", Context.LoginOrg.ID, reqLine.SrcDocNo, reqLine.SrcLineNo));
                        MaterialDeliveryDocLine materialDeliveryDocLine = MaterialDeliveryDocLine.Finder.Find(string.Format("MaterialDeliveryDoc.Org={0} and MaterialDeliveryDoc.DocNo='{1}' and LotNo='{2}'", Context.LoginOrg.ID, reqLine.SrcDocNo, reqLine.LotCode));

                        if (materialDeliveryDocLine == null)
                        {
                            return JsonUtil.GetFailResponse("未能获取到订单:" + reqLine.SrcDocNo + " 行号:" + reqLine.SrcDocNo + " 备料信息！", debugInfo);
                        }

                        listIds.Add(materialDeliveryDocLine.MOPick.ID.ToString());
                    }
                    conditionInfo.PickIDs = GetIDList4PickList(listIds);
                    sv1.ConditionInfo.Add(conditionInfo);
                    List<ConfirmDocLineInfo> listConfirmDocLineInfo = sv1.Do();
                    List<ConfirmDocLineInfo> listForSV = new List<ConfirmDocLineInfo>();
                    if (listConfirmDocLineInfo == null || listConfirmDocLineInfo.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("listConfirmDocLineInfo == null || listConfirmDocLineInfo.Count == 0", debugInfo);
                    }

                    foreach (var reqLine in reqHeader.MaterialDeliveryDocLines)
                    {
                        foreach (var item in listConfirmDocLineInfo)
                        {
                            MaterialDeliveryDocLine materialConfirmDocLine = item.MaterialDeliveryDocLine.GetEntity();
                            if (materialConfirmDocLine.MaterialDeliveryDoc.DocNo == reqLine.SrcDocNo
                                && materialConfirmDocLine.LotNo == reqLine.LotCode
                                && materialConfirmDocLine.ItemInfo.Code == reqLine.ItemCode)
                            {
                                item.RecedeQty = reqLine.ItemQty;
                                Warehouse warehouse = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);
                                if (warehouse == null)
                                {
                                    return JsonUtil.GetFailResponse(reqLine.WHCode + Base.U9Contant.NoFindWH, debugInfo);
                                }
                                item.Wh = warehouse.Key;
                                item.Wh_Code = warehouse.Code;
                                item.Wh_Name = warehouse.Name;

                                reqLine.MoDocNo = materialConfirmDocLine.SourceDoc.SrcDocNo;
                                listForSV.Add(item);
                                break;
                            }
                        }
                    }
                    if (listForSV == null || listForSV.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("listForSV == null || listForSV.Count == 0", debugInfo);
                    }
                    BatchCreateRecedeDoc sv2 = new BatchCreateRecedeDoc();

                    sv2.SplitCondition = new UFIDA.U9.IssueNew.IssueBP.BatchIssueApplySplitCond();
                    UFIDA.U9.Issue.MaterialDeliveryDocTypeBE.MaterialDeliveryDocType doctype = UFIDA.U9.Issue.MaterialDeliveryDocTypeBE.MaterialDeliveryDocType.Finder.Find(string.Format("Org={0} and Code='{1}'", Context.LoginOrg.ID, reqHeader.DocTypeCode));

                    if (doctype == null)
                    {
                        return JsonUtil.GetFailResponse(reqHeader.DocTypeCode + U9Contant.NoFindDocType, debugInfo);
                    }
                    sv2.MaterialDeliveryDocType = doctype.Key;
                    sv2.DocDate = DateTime.Parse(reqHeader.BusinessDate);
                    sv2.ConfirmDocLineInfo = listForSV;
                    List<MaterialDeliveryDoc.EntityKey> listMaterialDeliveryDocKey = sv2.Do();

                    if (listMaterialDeliveryDocKey == null || listMaterialDeliveryDocKey.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("listIssueKeyDTO == null || listIssueKeyDTO.Count == 0", debugInfo);
                    }
                    else if (listMaterialDeliveryDocKey.Count > 1)
                    {
                        throw new Exception("不支持同时入库多张生产订单！");
                    }
                    MaterialDeliveryDoc doc = listMaterialDeliveryDocKey[0].GetEntity();
                    using (ISession session = Session.Open())
                    {
                        doc.DocNo = reqHeader.WmsDocNo;
                        doc.Memo = reqHeader.Remark;
                        doc.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
                        doc.ConfirmDate = doc.BusinessDate;
                        HashSet<CreateMaterialInRequest.MaterialDeliveryDocLine> hs = new HashSet<CreateMaterialInRequest.MaterialDeliveryDocLine>();

                        foreach (var docLine in doc.MaterialDeliveryDocLines)
                        {
                            CreateMaterialInRequest.MaterialDeliveryDocLine curLine = reqHeader.MaterialDeliveryDocLines.Where(a =>
                            a.MoDocNo == docLine.SourceDoc.SrcDocNo
                            && a.ItemCode == docLine.ItemInfo.Code
                            && !hs.Contains(a)
                            ).FirstOrDefault();
                            if (curLine == null)
                            {
                                continue;
                            }

                            hs.Add(curLine);

                            if (!string.IsNullOrEmpty(curLine.LotCode)
                                    && CommonUtil.IsNeedLot(curLine.ItemCode, doc.Org.ID))
                            {
                                UFIDA.U9.Lot.LotMaster lotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.M_ReturnItem.Value, false);
                                if (lotMaster == null)
                                {
                                    throw U9Exception.GetException(curLine.LotCode + U9Contant.NoFindLot, debugInfo);
                                }
                                docLine.LotMaster = lotMaster;
                                docLine.LotNo = curLine.LotCode;
                            }
                            docLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                        }
                        session.Commit();
                        res.AppendLine(doc.DocNo);
                    }

                    if (reqHeader.IsAutoApprove)
                    {
                        U9Api.CustSV.ApproveMaterialDeliveryDocSV sv3 = new
                          ApproveMaterialDeliveryDocSV();
                        CommonApproveRequest req = new CommonApproveRequest();
                        req.DocNo = doc.DocNo;
                        sv3.JsonRequest = JsonUtil.GetJsonString(req);
                        res.Clear();
                        res.Append(sv3.Do());
                        scope.Commit();
                        return res.ToString();
                    }
                    scope.Commit();
                    return JsonUtil.GetSuccessResponse(doc.DocNo, doc.DocState.Name, debugInfo);
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