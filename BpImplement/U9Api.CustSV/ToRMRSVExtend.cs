namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.Base;

    /// <summary>
    /// ToRMRSV partial 
    /// </summary>	
    public partial class ToRMRSV
    {
        internal BaseStrategy Select()
        {
            return new ToRMRSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class ToRMRSVImpementStrategy : BaseStrategy
    {
        public ToRMRSVImpementStrategy() { }

        public override object Do(object obj)
        {
            ToRMRSV bpObj = (ToRMRSV)obj;
            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            ToRMRRequest reqHeader = JsonUtil.GetJsonObject<ToRMRRequest>(bpObj.JsonRequest);
            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

            // jizw 20171012 出货单单据类型的处理
            try
            {
                if (reqHeader.SrcDocType.Equals("so"))
                {
                    //未完成
                    debugInfo.AppendLine("so...");
                    UFIDA.U9.SM.SO.SOTORMR proxy = new UFIDA.U9.SM.SO.SOTORMR();

                    List<UFIDA.U9.SM.SO.RMRTransformDTO> srcDocInfoList = new List<UFIDA.U9.SM.SO.RMRTransformDTO>();
                    foreach (var reqLine in reqHeader.ToRMRLines)
                    {
                        //查找行订单
                        UFIDA.U9.SM.SO.SOLine soLine = UFIDA.U9.SM.SO.SOLine.Finder.Find(String.Format("SO.DocNo='{0}' and DocLineNo='{1}' and Org={2}", reqLine.SrcDocNo.Trim(), reqLine.SrcLineNo, Context.LoginOrg.ID));

                        if (soLine == null || soLine.ID < 1)
                        {
                            return JsonUtil.GetFailResponse(String.Format("创建出货单时，未根据来源单号{0}找到来源销售订单行", reqLine.SrcDocNo), debugInfo);
                        }
                        debugInfo.AppendLine("找到单号...");
                        UFIDA.U9.SM.SO.RMRTransformDTO dto = new UFIDA.U9.SM.SO.RMRTransformDTO();
                        dto.SOLineOrShipLineID = soLine.ID;
                        dto.ThisRequireReturnDoubleQty = new UFIDA.U9.CBO.DTOs.DoubleQuantity(soLine.OrderByQtyRU, soLine.OrderByQtyRU2, soLine.GetUOMInfoOfTU(), soLine.GetUOMInfoOfTU2());
                        srcDocInfoList.Add(dto);
                    }
                    proxy.TransformDTOLst = srcDocInfoList;

                    List<UFIDA.U9.SM.RMR.RMRHeadDTO> list = proxy.Do();
                    foreach (var item in list)
                    {
                        res.AppendLine(item.RMRHead.DocNo);
                    }
                }
                else if (reqHeader.SrcDocType.Equals("ship"))
                {
                    //已完成
                    debugInfo.AppendLine("ship...");
                    UFIDA.U9.SM.Ship.ShipToRMR proxy = new UFIDA.U9.SM.Ship.ShipToRMR();

                    List<UFIDA.U9.SM.Ship.ToRMRTransformDTO> srcDocInfoList = new List<UFIDA.U9.SM.Ship.ToRMRTransformDTO>();
                    foreach (var reqLine in reqHeader.ToRMRLines)
                    {
                        //查找行订单
                        UFIDA.U9.SM.Ship.ShipLine soLine = UFIDA.U9.SM.Ship.ShipLine.Finder.Find(String.Format("Ship.DocNo='{0}' and DocLineNo='{1}' and Org={2}", reqLine.SrcDocNo.Trim(), reqLine.SrcLineNo, Context.LoginOrg.ID));

                        if (soLine == null || soLine.ID < 1)
                        {
                            return JsonUtil.GetFailResponse(String.Format("创建出货单时，未根据来源单号{0}找到来源销售订单行", reqLine.SrcDocNo), debugInfo);
                        }
                        UFIDA.U9.SM.Ship.ToRMRTransformDTO dto = new UFIDA.U9.SM.Ship.ToRMRTransformDTO(soLine.Key, new UFIDA.U9.CBO.DTOs.DoubleQuantity(soLine.ShipQtyInvAmount, Decimal.Zero, new UFIDA.U9.CBO.DTOs.UOMInfoDTO(soLine.PriceUomKey), new UFIDA.U9.CBO.DTOs.UOMInfoDTO(soLine.PriceUomKey)), new UFIDA.U9.CBO.DTOs.DoubleQuantity(Decimal.Zero, Decimal.Zero, new UFIDA.U9.CBO.DTOs.UOMInfoDTO(soLine.PriceUomKey), new UFIDA.U9.CBO.DTOs.UOMInfoDTO(soLine.PriceUomKey)));
                        srcDocInfoList.Add(dto);
                    }
                    debugInfo.AppendLine("Invoke start");
                    proxy.SrcDocInfo = srcDocInfoList;
                    List<UFIDA.U9.SM.RMR.RMRHeadDTO> list = proxy.Do();
                    debugInfo.AppendLine("Invoke end");

                    if (list != null && list.Count > 0)
                    {
                        UFIDA.U9.SM.RMR.CreateRMR createRMR = new UFIDA.U9.SM.RMR.CreateRMR();//创建退回申请单BP

                        createRMR.RMRHeadDTOs = list;
                       List<UFIDA.U9.SM.RMR.RMRHeadEntityDTO> listRes = createRMR.Do();
                       if (listRes != null && listRes.Count > 0) {
                           foreach (var item in listRes)
                           {
                                res.Append(item.RMRHead.ID+",");
                           }
                       }
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