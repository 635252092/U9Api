namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.MO.MO;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.Lot;
    using UFIDA.U9.Complete.RcvRptBP;
    using UFSoft.UBF.Business;
    using U9Api.CustSV.Model.Response;
    using UFIDA.U9.CBO.Enums;
    using System.Linq;
    using U9Api.CustSV.Base;

    /// <summary>
    /// CreateRcvRptDocCustSV partial 
    /// </summary>	
    public partial class CreateRcvRptDocCustSV
    {
        internal BaseStrategy Select()
        {
            return new CreateRcvRptDocCustSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateRcvRptDocCustSVImpementStrategy : BaseStrategy
    {
        public CreateRcvRptDocCustSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateRcvRptDocCustSV bpObj = (CreateRcvRptDocCustSV)obj;
            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateRcvRptDocRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateRcvRptDocRequest>(bpObj.JsonRequest);
                if (reqHeader == null)
                {
                    throw new Exception("request == null");
                }
            }
            catch (Exception ex)
            {
                return JsonUtil.GetFailResponse("JSON格式错误;" + ex.Message);
            }
            string res = string.Empty;
            StringBuilder debugInfo = new StringBuilder();
            StringBuilder errorInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));
            //原始单号
            string sourceDocNo = string.Empty;
            using (UFSoft.UBF.Transactions.UBFTransactionScope scope = new UFSoft.UBF.Transactions.UBFTransactionScope(UFSoft.UBF.Transactions.TransactionOption.RequiresNew))
            {
                try
                {
                    UFIDA.U9.Complete.RcvRptBP.MOListToRcvRpt proxy = new UFIDA.U9.Complete.RcvRptBP.MOListToRcvRpt();
                    proxy.MOInfoDTO = new List<UFIDA.U9.Complete.RcvRptBP.MOInfoDTO>();
                    List<UFIDA.U9.Complete.RcvRptBP.MOInfoDTO> listMOInfoDTO = new List<MOInfoDTO>();
                    foreach (var reqLine in reqHeader.CreateRcvRptDocLines)
                    {
                        //查找行订单
                        MO mo = MO.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqLine.SrcDocNo));

                        if (mo == null)
                        {
                            return JsonUtil.GetFailResponse(String.Format("创建成品入库单，未根据来源单号{0}找到生产订单", reqLine.SrcDocNo), debugInfo);
                        }
                        if (mo.DocState != UFIDA.U9.MO.Enums.MOStateEnum.Released)
                        {
                            //debugInfo.AppendLine(mo.DocNo + ":订单不是开工状态");
                            //errorInfo.AppendLine(mo.DocNo + ":订单不是开工状态");
                            //continue; 
                            return JsonUtil.GetFailResponse(mo.DocNo + ":订单不是开工状态");
                        }
                        DateTime businessDate = DateTime.Parse(reqHeader.BusinessDate);
                        if (mo.ActualStartDate > businessDate)
                        {
                            return JsonUtil.GetFailResponse($"申报单完工日期/入库日期{reqHeader.BusinessDate}不能早于生产单据{mo.DocNo}实际开工日期{mo.ActualStartDate.ToString("yyyy-MM-dd HH:mm:ss")}");
                        }
                        UFIDA.U9.Complete.RcvRptBP.MOInfoDTO dto = new UFIDA.U9.Complete.RcvRptBP.MOInfoDTO();
                        dto.MOID = mo.Key;
                        dto.RcvQty = reqLine.RcvQty;
                        Warehouse warehouse = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);
                        if (warehouse == null)
                        {
                            return JsonUtil.GetFailResponse(reqLine.WHCode + U9Contant.NoFindWH, debugInfo);
                        }
                        dto.SCVWh = warehouse.Key;


                        dto.CompleteDate = businessDate;
                        dto.CompleteWh = warehouse.Key;
                        dto.SCVWh = warehouse.Key;
                        dto.CompleteQty = reqLine.RcvQty;

                        listMOInfoDTO.Add(dto);
                    }
                    if (listMOInfoDTO == null || listMOInfoDTO.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("请检查订单;" + errorInfo.ToString(), debugInfo);
                    }
                    proxy.MOInfoDTO = listMOInfoDTO;
                    RcvRptResultDTO rcvRptResultDTO = proxy.Do();
                    if (rcvRptResultDTO == null || rcvRptResultDTO.DocNos == null || rcvRptResultDTO.DocNos.Count == 0)
                    {
                        throw U9Exception.GetException(U9Contant.U9Fail_NoResponse, debugInfo);
                    }
                    sourceDocNo = rcvRptResultDTO.DocNos[0];
                    List<CommonApproveResponse> listRes = new List<CommonApproveResponse>();
                    foreach (var docNo in rcvRptResultDTO.DocNos)
                    {
                        debugInfo.AppendLine(docNo);
                        UFIDA.U9.Complete.RCVRpt.RcvRptDoc rcvRptDoc = UFIDA.U9.Complete.RCVRpt.RcvRptDoc.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, docNo));

                        if (rcvRptDoc == null)
                        {
                            return JsonUtil.GetFailResponse("rcvRptDoc == null");
                        }

                        using (ISession session = Session.Open())
                        {
                            if (!string.IsNullOrEmpty(reqHeader.RcvDocNo))
                            {
                                rcvRptDoc.DocNo = reqHeader.RcvDocNo;
                            }
                            rcvRptDoc.Remark = reqHeader.Remark;
                            rcvRptDoc.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
                            rcvRptDoc.ActualRcvTime = rcvRptDoc.BusinessDate;
                            rcvRptDoc.DocDate = rcvRptDoc.BusinessDate;
                            //修改批号
                            HashSet<CreateRcvRptDocRequest.CreateRcvRptDocLine> hs = new HashSet<CreateRcvRptDocRequest.CreateRcvRptDocLine>();
                            foreach (var rcvLine in rcvRptDoc.RcvRptDocLines)
                            {
                                CreateRcvRptDocRequest.CreateRcvRptDocLine curLine = reqHeader.CreateRcvRptDocLines.Where(a => a.ItemCode == rcvLine.Item.Code
                                && !hs.Contains(a)
                                ).FirstOrDefault();

                                if (curLine != null && !string.IsNullOrEmpty(curLine.LotCode)
                                     && CommonUtil.IsNeedLot(curLine.ItemCode, Context.LoginOrg.ID)
                                    )
                                {
                                    hs.Add(curLine);
                                    rcvLine.RcvLotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.M_CompleteApplyDoc.Value);
                                    rcvLine.RcvLotNo = curLine.LotCode;
                                    debugInfo.AppendLine("批号赋值end");
                                }
                                rcvLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                            }
                            session.Commit();
                            listRes.Add(new CommonApproveResponse() { DocNo = rcvRptDoc.DocNo });
                        }
                        //修改批号的有效期
                        string sql = string.Format(@"UPDATE line SET RcvLotDisableDate=NULL from  Complete_RcvRptDocLine line 
INNER JOIN Complete_RcvRptDoc header on line.RcvRptDoc=header.ID
where header.Org = {0} AND header.DocNo = '{1}'", Context.LoginOrg.ID, rcvRptDoc.DocNo);
                        CommonUtil.RunUpdateSQL(sql);

                        if (reqHeader.IsAutoApprove)
                        {
                            RcvRptDocApproveSV sv2 = new RcvRptDocApproveSV();
                            CommonApproveRequest shipApproveRequest = new CommonApproveRequest();
                            shipApproveRequest.DocNo = rcvRptDoc.DocNo;
                            sv2.JsonRequest = JsonUtil.GetJsonString(shipApproveRequest);
                            res = sv2.Do();
                        }
                        else
                        {
                            res = JsonUtil.GetSuccessResponse(rcvRptDoc.DocNo, debugInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.WriteDebugInfoLog(debugInfo.ToString());
                    scope.Rollback();

                    if (!string.IsNullOrEmpty(sourceDocNo))
                    {
                        try
                        {
                            LogUtil.WriteDebugInfoLog($"{sourceDocNo}:删除start");
                            DeleteCustSV deleteCustSV = new DeleteCustSV();
                            DeleteCustRequest deleteCustRequest = new DeleteCustRequest();
                            deleteCustRequest.WmsDocNo = sourceDocNo;
                            deleteCustRequest.DocTypeCode = Model.Enum.DocTypeCustEnum.RcvRptDoc.ToString();

                            deleteCustSV.JsonRequest = JsonUtil.GetJsonString(deleteCustRequest);
                            deleteCustSV.Do();
                            LogUtil.WriteDebugInfoLog($"{sourceDocNo}:删除end");
                        }
                        catch (Exception ex2)
                        {
                            string custMsg = sourceDocNo + "：删除失败，请联系U9管理员手动删除;";
                            string exMsg = custMsg + Base.U9Exception.GetInnerExceptionMsg(ex2);
                            return JsonUtil.GetFailResponse(exMsg);
                        }
                    }
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