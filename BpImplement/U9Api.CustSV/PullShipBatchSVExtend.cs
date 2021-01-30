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
    using UFIDA.U9.SM.Ship;
    using UFIDA.U9.CBO.Enums;
    
    

    /// <summary>
    /// PullShipBatchSV partial 
    /// </summary>	
    public partial class PullShipBatchSV
    {
        internal BaseStrategy Select()
        {
            return new PullShipBatchSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class PullShipBatchSVImpementStrategy : BaseStrategy
    {
        public PullShipBatchSVImpementStrategy() { }
        private TransformByOrgDTO FindByOrgID(long lngOrgID, List<TransformByOrgDTO> soLinesDTO)
        {
            TransformByOrgDTO result;
            foreach (TransformByOrgDTO current in soLinesDTO)
            {
                if (current.SrcOrgKey.ID == lngOrgID)
                {
                    result = current;
                    return result;
                }
            }
            TransformByOrgDTO transformByOrgDTOData = new TransformByOrgDTO();
            transformByOrgDTOData.TransData = new List<UFIDA.U9.SM.Ship.TransformDTO>();
            soLinesDTO.Add(transformByOrgDTOData);
            result = transformByOrgDTOData;
            return result;
        }
        public override object Do(object obj)
        {
            PullShipBatchSV bpObj = (PullShipBatchSV)obj;
            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            //PullShipBatchRequest reqHeader = JsonUtil.GetJsonObject<PullShipBatchRequest>(bpObj.JsonRequest);

            PullShipBatchRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<PullShipBatchRequest>(bpObj.JsonRequest);
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


            UFIDA.U9.SM.Ship.SMPullSO sMPullSOProxy = new UFIDA.U9.SM.Ship.SMPullSO();
            List<UFIDA.U9.SM.Ship.TransformDTO> list = new List<UFIDA.U9.SM.Ship.TransformDTO>();
            sMPullSOProxy.DivTerms = new List<string>();
            sMPullSOProxy.Interval = 1;
            List<UFIDA.U9.SM.Ship.TransformByOrgDTO> list2 = new List<UFIDA.U9.SM.Ship.TransformByOrgDTO>();
            using (UFSoft.UBF.Transactions.UBFTransactionScope scope = new UFSoft.UBF.Transactions.UBFTransactionScope(UFSoft.UBF.Transactions.TransactionOption.RequiresNew))
            {
                try
            {
                bool IsConsign = false;
                debugInfo.AppendLine("遍历行start..");
                List<UFIDA.U9.ISV.SM.SrcDocInfoDTO> srcDocInfoList = new List<UFIDA.U9.ISV.SM.SrcDocInfoDTO>();
                foreach (var reqLine in reqHeader.PullShipLines)
                {
                    //查找行订单
                    UFIDA.U9.Base.Organization.Organization soOrg = UFIDA.U9.Base.Organization.Organization.FindByCode(reqLine.SrcOrg);
                    if (soOrg == null)
                    {
                        return JsonUtil.GetFailResponse(reqLine.SrcOrg + U9Contant.NoFindOrg);
                    }

                    UFIDA.U9.SM.SO.SOLine soLine = UFIDA.U9.SM.SO.SOLine.Finder.Find(String.Format("SO.DocNo='{0}' and DocLineNo='{1}' and Org={2}", reqLine.SrcDocNo.Trim(), reqLine.SrcLineNo, soOrg.ID));

                    if (soLine == null || soLine.ID < 1)
                    {
                        return JsonUtil.GetFailResponse(String.Format("创建出货单时，未根据来源单号{0}找到来源销售订单行", reqLine.SrcDocNo), debugInfo);
                    }
                    foreach (SOShipline soShipline in soLine.SOShiplines)
                    {
                        if (reqLine.SrcSubLineNo != soShipline.DocSubLineNo)
                        {
                            continue;
                        }
                        debugInfo.AppendLine("遍历行doing..");
                        IsConsign = soShipline.SOLine.SO.IsCompensate;
                        TransformByOrgDTO transformByOrgDTOData = this.FindByOrgID(soShipline.SOLine.SO.Org.ID, list2);

                        UFIDA.U9.SM.Ship.TransformDTO transformDTOData = new UFIDA.U9.SM.Ship.TransformDTO();
                        transformDTOData.SrcDocLinesKey = soShipline.Key;

                        DoubleQuantity doubleQuantityData = new DoubleQuantity(reqLine.ShipQty, Decimal.Zero, new UOMInfoDTO(soShipline.TU.Key, soShipline.TBU.Key, soShipline.TUToTBURate), new UOMInfoDTO(soShipline.TU.Key, soShipline.TBU.Key, soShipline.TUToTBURate));
                        Warehouse wh = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);
                        if (wh == null)
                        {
                            return JsonUtil.GetFailResponse(reqLine.WHCode + U9Contant.NoFindWH, debugInfo);
                        }

                        transformDTOData.WareHouse = wh.Key;
                        transformDTOData.ShipmentQty = doubleQuantityData;
                        transformByOrgDTOData.TransData.Add(transformDTOData);
                        transformByOrgDTOData.SrcOrgKey = soOrg.Key;
                    }
                }

                if (list2 == null || list2.Count == 0)
                {
                    return JsonUtil.GetFailResponse("没有待发货行，请检查订单", debugInfo);
                }
                sMPullSOProxy.SOSubLines = list2;
                sMPullSOProxy.IsConsign = IsConsign;
                sMPullSOProxy.ShipDTO = new ShipDTOForPull();
                sMPullSOProxy.SMDate = DateTime.Parse(reqHeader.BusinessDate);
                List<ShipPullSrcDocDTO> list3 = sMPullSOProxy.Do();

                U9Api.CustSV.Model.Response.CommonApproveResponse response = null;
                foreach (var shipdto in list3)
                {
                    UFIDA.U9.SM.Ship.Ship ship = shipdto.ShipID.GetEntity();
                    if (ship != null)
                    {
                        using (ISession session = Session.Open())
                        {
                            //联系人
                            if (!string.IsNullOrEmpty(reqHeader.RcvContact))
                            {
                                UFIDA.U9.SM.Ship.ShipContact rcvContact = UFIDA.U9.SM.Ship.ShipContact.Finder.Find(string.Format("Org={0} and Contact.Name='{1}'", ship.Org.ID, reqHeader.RcvContact));
                               
                                if (rcvContact == null)
                                {
                                    rcvContact = UFIDA.U9.SM.Ship.ShipContact.Create(ship);
                                    rcvContact.Org = ship.Org;
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
                            HashSet<PullShipBatchRequest.PullShipBatchLine> hs = new HashSet<PullShipBatchRequest.PullShipBatchLine>();
                            foreach (var shipLine in ship.ShipLines)
                            {
                                    PullShipBatchRequest.PullShipBatchLine curLine = reqHeader.PullShipLines.Where(a => a.ItemCode == shipLine.ItemInfo.ItemCode && a.SrcLineNo == shipLine.SrcDocLineNo && a.SrcSubLineNo == shipLine.SrcDocSubLineNo
                                && !hs.Contains(a)
                                ).FirstOrDefault();
                                if (curLine != null && !string.IsNullOrEmpty(curLine.LotCode)&&CommonUtil.IsNeedLot(shipLine.ItemInfo.ItemCode, shipLine.Org.ID))
                                {
                                    hs.Add(curLine);
                                    LotMaster lotMaster = CommonUtil.GetLot(curLine.LotCode, curLine.ItemCode, curLine.WHCode, LotSnStatusEnum.S_Ship.Value, false, shipLine.Org.Code);
                                    if (lotMaster == null)
                                    {
                                        throw U9Exception.GetException(curLine.LotCode+U9Contant.NoFindLot, debugInfo);
                                    }
                                    shipLine.LotInfo.LotMaster = lotMaster;
                                    shipLine.LotInfo.LotCode = curLine.LotCode;
                                }

                                shipLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
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
                        shipApproveRequest.OrgID = ship.Org.ID;
                        sv2.JsonRequest = JsonUtil.GetJsonString(shipApproveRequest);
                            res.Clear();
                            res.Append(sv2.Do());
                            scope.Commit();
                            return res.ToString();
                        }
                }
                    scope.Commit();
                    return JsonUtil.GetSuccessResponse(response,debugInfo);
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