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
    using UFIDA.U9.PM.Rtn;
    using U9Api.CustSV.Base;


    /// <summary>
    /// CreateRcvFromRtnSV partial 
    /// </summary>	
    public partial class CreateRcvFromRtnSV 
	{	
		internal BaseStrategy Select()
		{
			return new CreateRcvFromRtnSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class CreateRcvFromRtnSVImpementStrategy : BaseStrategy
	{
		public CreateRcvFromRtnSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			CreateRcvFromRtnSV bpObj = (CreateRcvFromRtnSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateRcvFromRtnRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateRcvFromRtnRequest>(bpObj.JsonRequest);
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
               StringBuilder errorInfo = new StringBuilder();
          debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));
            using (UFSoft.UBF.Transactions.UBFTransactionScope scope = new UFSoft.UBF.Transactions.UBFTransactionScope(UFSoft.UBF.Transactions.TransactionOption.RequiresNew))
            {
                try
            {
                List<RcvFromPODTO> list = new List<RcvFromPODTO>();
                RTNToRtnRcvSV sv1 = new RTNToRtnRcvSV();
                UFIDA.U9.PM.Pub.RcvDocType docType = UFIDA.U9.PM.Pub.RcvDocType.FindByCode(Context.LoginOrg,reqHeader.DocTypeCode);
                if (docType == null)
                {
                    return JsonUtil.GetFailResponse(reqHeader.DocTypeCode+U9Contant.NoFindDocType, debugInfo);
                }
                sv1.RcvDocType = docType;
                sv1.GroupParams = new List<string>();
                sv1.RtnKeys = new List<RtnLine.EntityKey>();
                sv1.RtnToRcvDTOs = new List<RtnToRcvDTO>();
               
                List<RtnToRcvDTO> listRtnToRcvDTO = new List<RtnToRcvDTO>();
                foreach (var reqLine in reqHeader.CreateRcvFromRtnLines)
                {
                    //查找行订单
                    RtnLine rtnLine = RtnLine.Finder.Find(string.Format("Rtn.Org={0} and Rtn.DocNo='{1}' and RtnLineNO={2}", Context.LoginOrg.ID, reqLine.SrcDocNo, reqLine.SrcLineNo));
                
                    if (rtnLine == null)
                    {
                        return JsonUtil.GetFailResponse(string.Format("单号：{0} 行号:{1} 没有找到对应的【采购退货申请单行】", reqLine.SrcDocNo, reqLine.SrcLineNo), debugInfo);
                    }
                    if (rtnLine.Status != RtnStatusEnum.Approved)
                    {
                        debugInfo.AppendLine("行不是审核状态");
                        errorInfo.AppendLine("行不是审核状态");
                        continue;
                    }
                    debugInfo.AppendLine("遍历行");

                    RtnToRcvDTO rtnToRcvDTOData = new RtnToRcvDTO();
                    rtnToRcvDTOData.RtnLineKey=rtnLine.Key;
                    rtnToRcvDTOData.RcvBusinessDate = DateTime.Parse(reqHeader.BusinessDate);

                    UFIDA.U9.CBO.DTOs.UOMInfoDTO uom1 = new UFIDA.U9.CBO.DTOs.UOMInfoDTO(rtnLine.RtnUom.Key, rtnLine.RtnUom.Key, 0);

                    if (rtnLine.RtnOrderQtyRU > 0 && rtnLine.RtnARQtyRU > 0)
                    {
                        return JsonUtil.GetFailResponse("WMS不支持【同时退扣退补】操作", debugInfo);
                    }
                    if (rtnLine.RtnOrderQtyRU > 0)
                    {
                        rtnToRcvDTOData.RtnFillQty = new UFIDA.U9.CBO.DTOs.DoubleQuantity(reqLine.ItemQty, Decimal.Zero, uom1, uom1);
                        rtnToRcvDTOData.RtnDeductQty = new UFIDA.U9.CBO.DTOs.DoubleQuantity();
                    }
                    else if (rtnLine.RtnARQtyRU > 0)
                    {
                        rtnToRcvDTOData.RtnDeductQty = new UFIDA.U9.CBO.DTOs.DoubleQuantity(reqLine.ItemQty, Decimal.Zero, uom1, uom1);
                        rtnToRcvDTOData.RtnFillQty = new UFIDA.U9.CBO.DTOs.DoubleQuantity();
                    }

                    listRtnToRcvDTO.Add(rtnToRcvDTOData);
                }
                if (listRtnToRcvDTO == null || listRtnToRcvDTO.Count == 0)
                {
                    return JsonUtil.GetFailResponse("请检查订单行;" + errorInfo.ToString(), debugInfo);
                }
                sv1.RtnToRcvDTOs = listRtnToRcvDTO;
                //pOToRcvSVProxy.RcvDocType = UFIDA.U9.PM.Pub.RcvDocType.FindByCode( Context.LoginOrg,reqHeader.RcvDocType).Key;

                RcvFromRTN rcvFromPODTO = sv1.Do();
                if (rcvFromPODTO == null || rcvFromPODTO.Rcvs == null)
                {
                        throw U9Exception.GetException(U9Contant.U9Fail_NoResponse, debugInfo);
                    }
                System.Collections.ArrayList arrayList = (System.Collections.ArrayList)rcvFromPODTO.Rcvs ;
                List<RcvHeadDTOData> listRcvHeadDTOData = new List<RcvHeadDTOData>();
                foreach (object current4 in arrayList)
                {
                    listRcvHeadDTOData.Add(current4 as RcvHeadDTOData);
                }

                    if (listRcvHeadDTOData == null || listRcvHeadDTOData.Count == 0)
                    {
                        throw U9Exception.GetException(U9Contant.U9Fail_NoResponse, debugInfo);
                    }
                    else if (listRcvHeadDTOData.Count > 1)
                    {
                        return JsonUtil.GetFailResponse("创建的收货单数量超过1个，请检查来源订单的供应商是否一致", debugInfo);
                    }
                RcvHeadDTOData rcvHeadDTOData = listRcvHeadDTOData[0];
                //Receivement r2 = Receivement.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, rcvHeadDTOData.RcvHead.DocNo));
                //if (r2 == null)
                //{
                //    debugInfo.AppendLine("r2 == null");
                //}

                rcvHeadDTOData.RcvHead.DocNo = reqHeader.RcvDocNo;
                HashSet<CreateRcvFromRtnRequest.CreateRcvFromRtnLine> hs = new HashSet<CreateRcvFromRtnRequest.CreateRcvFromRtnLine>();
                foreach (var current2 in rcvHeadDTOData.RcvLineDTOs)
                {
                    CreateRcvFromRtnRequest.CreateRcvFromRtnLine reqLine = reqHeader.CreateRcvFromRtnLines.Where(a => a.SrcDocNo == current2.RcvLine.SrcDoc.SrcDocNo 
                    && a.SrcLineNo == current2.RcvLine.SrcDoc.SrcDocLineNo
                    &&!hs.Contains(a)
                    ).FirstOrDefault();

                    if (reqLine == null)
                    {
                        return JsonUtil.GetFailResponse("reqLine == null", debugInfo);
                    }
                    hs.Add(reqLine);
                    Warehouse warehouse = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);

                    if (warehouse == null)
                    {
                        return JsonUtil.GetFailResponse(reqLine.WHCode+ Base.U9Contant.NoFindWH, debugInfo);
                    }
                    current2.RcvLine.Wh = warehouse.Key.ID;
                    UFIDA.U9.CBO.HR.Operator.Operators wHMan = warehouse.Manager;
                    if (wHMan == null)
                    {
                        return JsonUtil.GetFailResponse(Base.U9Contant.NoFindWHMan, debugInfo);
                    }
                    current2.RcvLine.WhMan = wHMan.Key.ID;
                    UFIDA.U9.CBO.HR.Department.Department rcvDept = warehouse.Department;
                    if (rcvDept == null)
                    {
                        return JsonUtil.GetFailResponse(Base.U9Contant.NoFindWHDept, debugInfo);
                    }
                    current2.RcvLine.RcvDept = rcvDept.Key.ID;


                    current2.RcvLine.ConfirmDate = DateTime.Parse(reqHeader.BusinessDate);
                    current2.RcvLine.EyeballedTime = DateTime.Parse(reqHeader.BusinessDate);
                    current2.RcvLine.ArrivedTime = DateTime.Parse(reqHeader.BusinessDate);
                    debugInfo.AppendLine("current2.RcvLine.StorageTyp:"+current2.RcvLine.StorageType);
                    //current2.RcvLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Reject.Value;
                    debugInfo.AppendLine("current2.RcvLine.InvLot:" + current2.RcvLine.InvLot);
                    if (current2.RcvLine.InvLot < 1)
                    {
                        if (reqLine != null && !string.IsNullOrEmpty(reqLine.LotCode)
                             && CommonUtil.IsNeedLot(reqLine.ItemCode, Context.LoginOrg.ID))
                        {
                            LogUtil.WriteDebugInfoLog(current2.RcvLine.ItemInfo.ItemCode + " " + reqLine.LotCode);
                            LotMaster lotMaster = CommonUtil.GetLot(reqLine.LotCode, reqLine.ItemCode, reqLine.WHCode, LotSnStatusEnum.P_Receive.Value, true);
                            if (lotMaster == null)
                            {
                                return JsonUtil.GetFailResponse("没有找到批号：" + reqLine.LotCode);
                            }
                            current2.RcvLine.InvLot = lotMaster.ID;
                            current2.RcvLine.InvLotCode = reqLine.LotCode;
                        } 
                    }
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
                Receivement r = Receivement.Finder.FindByID(list2[0].ID);
                if (r == null)
                {
                    return JsonUtil.GetFailResponse("r == null");
                }
                //using (ISession session = Session.Open())
                //{
                //    //修改批号
                //    foreach (var rcvLine in r.RcvLines)
                //    {
                //        rcvLine.ConfirmDate = r.BusinessDate;
                //        rcvLine.EyeballedTime = r.BusinessDate;
                //        rcvLine.ArrivedTime = r.BusinessDate;
                //        rcvLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                //        CreateRcvFromRtnRequest.CreateRcvFromRtnLine curLine = reqHeader.CreateRcvFromRtnLines.Where(a => a.SrcDocNo == rcvLine.SrcDoc.SrcDocNo && a.SrcLineNo == rcvLine.SrcDoc.SrcDocLineNo).FirstOrDefault();
                //        LogUtil.WriteDebugInfoLog(rcvLine.ItemInfo.ItemCode + " " + rcvLine.SrcDoc.SrcDocLineNo + " " + curLine.LotCode);
                //        if (curLine != null && !string.IsNullOrEmpty(curLine.LotCode))
                //        {
                //            LogUtil.WriteDebugInfoLog(rcvLine.ItemInfo.ItemCode + " " + curLine.LotCode);
                //            LotMaster lotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.P_Receive.Value, true);
                //            if (lotMaster == null)
                //            {
                //                return JsonUtil.GetFailResponse("没有找到批号：" + curLine.LotCode);
                //            }
                //            rcvLine.InvLot = lotMaster;
                //            rcvLine.InvLotCode = curLine.LotCode;
                //        }
                //    }
                //    if (!string.IsNullOrEmpty(reqHeader.RcvDocNo))
                //    {
                //        r.DocNo = reqHeader.RcvDocNo;
                //    }

                //    res.Append(r.DocNo);
                //    r.Memo = reqHeader.Remark;
                //    session.Commit();
                //}
                if (reqHeader.IsAutoApprove)
                {
                    RcvApproveSV sv2 = new RcvApproveSV();
                    RcvApproveRequest shipApproveRequest = new RcvApproveRequest();
                    shipApproveRequest.DocNo = r.DocNo;
                    sv2.JsonRequest = JsonUtil.GetJsonString(shipApproveRequest);
                        res.Clear();
                        res.Append(sv2.Do());
                        scope.Commit();
                        return res.ToString();
                    }
                    //return r.DocNo;
                    //return JsonUtil.GetSuccessResponse(r.DocNo,debugInfo);

                    scope.Commit();
                    return JsonUtil.GetSuccessResponse(r.DocNo, r.Status.Name, debugInfo);
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