using U9Api.CustSV;
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
    using UFIDA.U9.CBO.DTOs;
    using UFIDA.U9.CBO.Enums;

    /// <summary>
    /// PullShipSV partial 
    /// </summary>	
    public partial class PullShipSV
    {
        internal BaseStrategy Select()
        {
            return new PullShipSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class PullShipSVImpementStrategy : BaseStrategy
    {
        public PullShipSVImpementStrategy() { }
     

        public override object Do(object obj)
        {

            PullShipSV bpObj = (PullShipSV)obj;
            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            PullShipBatchRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<PullShipBatchRequest>(bpObj.JsonRequest);
            }
            catch (Exception ex)
            {
                return JsonUtil.GetFailResponse("JSON格式错误");
            }

            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

            try
            {
                SOPushSMBP proxy = new SOPushSMBP();
                //UFIDA.U9.ISV.SM.SMPullSrcDocSV proxy = new UFIDA.U9.ISV.SM.SMPullSrcDocSV();
                proxy.SplitRules = new List<string>();
                proxy.TransFormDTOs = new List<TransformDTO>();
                List<TransformDTO> listTransformDTO = new List<TransformDTO>();

                foreach (var reqLine in reqHeader.PullShipLines)
                {
                    //查找行订单
                    UFIDA.U9.Base.Organization.Organization org = UFIDA.U9.Base.Organization.Organization.FindByCode(reqLine.SrcOrg);
                    if (org == null)
                    {
                        return JsonUtil.GetFailResponse("org == null");
                    }

                    UFIDA.U9.SM.SO.SOLine soLine = UFIDA.U9.SM.SO.SOLine.Finder.Find(String.Format("SO.DocNo='{0}' and DocLineNo='{1}' and Org={2}", reqLine.SrcDocNo.Trim(), reqLine.SrcLineNo, org.ID));

                    if (soLine == null || soLine.ID < 1)
                    {
                        return JsonUtil.GetFailResponse(String.Format("创建出货单时，未根据来源单号{0}找到来源销售订单行", reqLine.SrcDocNo), debugInfo);
                    }
                    debugInfo.AppendLine("找到单号...");
                    decimal curShipQty = 0, maxShipQty = 0, restQty = reqLine.ShipQty;
                    foreach (SOShipline soShipline in soLine.SOShiplines)
                    {
                        if (reqLine.SrcSubLineNo != soShipline.DocSubLineNo)
                        {
                            continue;
                        }
                        if (soShipline.Status != SODocStatusEnum.Approved)
                        {
                            continue;
                        }
                        debugInfo.AppendLine("遍历行doing..");

                        maxShipQty = soShipline.ShipPlanQtyTU - soShipline.SOShipLineSumInfo.SumShipQtyTU;
                        curShipQty = Math.Min(maxShipQty, reqLine.ShipQty);//出货数量
                        restQty -= curShipQty;

                        DoubleQuantity doubleQuantityData = new DoubleQuantity(curShipQty, Decimal.Zero, new UOMInfoDTO(soShipline.TU.Key, soShipline.TBU.Key, soShipline.TUToTBURate), new UOMInfoDTO(soShipline.TU.Key, soShipline.TBU.Key, soShipline.TUToTBURate));
                        TransformDTO transformDTOData = new TransformDTO();
                        transformDTOData.SrcDocLinesKey = soShipline.Key;
                        transformDTOData.ShipmentQty = doubleQuantityData;

                        Warehouse wh = null;
                        if (soShipline.WH != null)
                        {
                            wh = soShipline.WH;//仓库
                        }
                        else
                        {
                            debugInfo.AppendLine(reqLine.WHCode);
                            wh = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);
                            if (wh == null)
                            {
                                return JsonUtil.GetFailResponse("wh==null", debugInfo);
                            }
                        }

                        transformDTOData.Warehouse = wh.Key;
                        listTransformDTO.Add(transformDTOData);
                    }
                    if (restQty < 0)
                    {
                        return JsonUtil.GetFailResponse("restQty < 0,发货数量超过计划数量", debugInfo);
                    }
                }
                if (listTransformDTO == null || listTransformDTO.Count == 0)
                {
                    return JsonUtil.GetFailResponse("listTransformDTO == null || listTransformDTO.Count == 0", debugInfo);
                }

                proxy.SMDate = DateTime.Parse(reqHeader.BusinessDate);
                proxy.TimeSplot = 0;
                proxy.TransFormDTOs = listTransformDTO;
                List<SOToShipMentDTO> listSOToShipMentDTO = proxy.Do();

                if (listSOToShipMentDTO == null || listSOToShipMentDTO.Count == 0)
                {
                    return JsonUtil.GetFailResponse("listSOToShipMentDTO == null || listSOToShipMentDTO.Count == 0", debugInfo);
                }
                U9Api.CustSV.Model.Response.CommonApproveResponse response = null;
                foreach (var sOToShipMentDTO in listSOToShipMentDTO)
                {
                    if (sOToShipMentDTO.ShipmentIDList == null || sOToShipMentDTO.ShipmentIDList.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("listSOToShipMentDTO == null || listSOToShipMentDTO.Count == 0", debugInfo);
                    }

                    foreach (var id in sOToShipMentDTO.ShipmentIDList)
                    {
                        UFIDA.U9.SM.Ship.Ship ship = UFIDA.U9.SM.Ship.Ship.Finder.FindByID(id);
                        if (ship != null)
                        {
                            using (ISession session = Session.Open())
                            {
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
                                //出货确认日期
                                ship.ShipConfirmDate = ship.BusinessDate;
                                if (!string.IsNullOrEmpty(reqHeader.DocNo))
                                {
                                    ship.DocNo = reqHeader.DocNo;
                                }
                                ship.ShipMemo = reqHeader.Remark;
                                foreach (var item in ship.ShipLines)
                                {
                                    PullShipBatchRequest.PullShipBatchLine curLine = reqHeader.PullShipLines.Where(a => a.ItemCode == item.ItemInfo.ItemCode && a.SrcLineNo == item.SrcDocLineNo && a.SrcSubLineNo == item.SrcDocSubLineNo).FirstOrDefault();
                                    if (curLine != null && !string.IsNullOrEmpty(curLine.LotCode))
                                    {
                                        LotMaster lotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.S_Ship.Value, false);
                                        if (lotMaster == null)
                                        {
                                            return JsonUtil.GetFailResponse("lotMaster == null", debugInfo);
                                        }
                                        item.LotInfo.LotMaster = lotMaster;
                                        item.LotInfo.LotCode = curLine.LotCode;
                                        item.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                                    }
                                }
                                session.Commit();
                            }
                            res.AppendLine(ship.DocNo);
                            response = new Model.Response.CommonApproveResponse();
                            response.DocNo = ship.DocNo;
                            response.CurrentStatus = ship.Status.Name;
                        }
                        if (reqHeader.IsAutoApprove)
                        {
                            ShipApproveSV sv2 = new ShipApproveSV();
                            ShipApproveRequest shipApproveRequest = new ShipApproveRequest();
                            shipApproveRequest.DocNo = ship.DocNo;
                            sv2.JsonRequest = JsonUtil.GetJsonString(shipApproveRequest);
                            return sv2.Do();
                        }
                    }
                }

                if (response != null)
                {
                    return JsonUtil.GetSuccessResponse(JsonUtil.GetJsonString(response), debugInfo);
                }
            }
            catch (Exception ex)
            {
                LogUtil.WriteDebugInfoLog(debugInfo.ToString());
                throw U9Exception.GetInnerException(ex);
            }
            return JsonUtil.GetFailResponse("发货单创建失败", debugInfo);

        }
    }
    #endregion
}