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
	/// <summary>
	/// PullShip2SV partial 
	/// </summary>	
	public partial class PullShip2SV 
	{	
		internal BaseStrategy Select()
		{
			return new PullShip2SVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class PullShip2SVImpementStrategy : BaseStrategy
	{
		public PullShip2SVImpementStrategy() { }

		public override object Do(object obj)
		{						
			PullShip2SV bpObj = (PullShip2SV)obj;
            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            PullShipRequest reqHeader = JsonUtil.GetJsonObject<PullShipRequest>(bpObj.JsonRequest);
            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok"+Context.LoginOrg.ID));
            UFIDA.U9.SM.SO.SO so = UFIDA.U9.SM.SO.SO.Finder.Find(String.Format("DocNO='{0}'", reqHeader.SrcDocNo));
            if (so == null || so.ID < 1)
            {
                return JsonUtil.GetFailResponse(String.Format("创建出货单时，未根据来源单号{0}找到来源销售订单", reqHeader.SrcDocNo), debugInfo);
            }
            debugInfo.AppendLine("找到单号...");
            UFIDA.U9.ISV.SM.SMPullSrcDocSV proxy = new UFIDA.U9.ISV.SM.SMPullSrcDocSV();


            // jizw 20171012 出货单单据类型的处理
            try
            {
                if (!string.IsNullOrEmpty(reqHeader.DocTypeCode))
                {
                    UFIDA.U9.SM.Ship.ShipDocType shipDocType = UFIDA.U9.SM.Ship.ShipDocType.Finder.Find("Code=@code", new UFSoft.UBF.PL.OqlParam("code", reqHeader.DocTypeCode));
                    if (shipDocType != null)
                    {
                        proxy.ShipDocTypeID = shipDocType.ID;
                    }
                    else
                    {
                        return JsonUtil.GetFailResponse("shipDocType != null", debugInfo);

                    }
                }
                debugInfo.AppendLine("遍历行start..");
                List<SrcDocInfoDTO> srcDocInfoList = new List<SrcDocInfoDTO>();
                foreach (SOLine soLine in so.SOLines)
                {
                    foreach (SOShipline headerLine in soLine.SOShiplines)
                    {
                        debugInfo.AppendLine("遍历行doing..");
                        SrcDocInfoDTO srcDocInfoDTO = new SrcDocInfoDTO();
                        U9Api.CustSV.Model.Request.PullShipRequest.PullShipLine reqLine = reqHeader.PullShipLines.Where(a => a.SrcLineNo == soLine.DocLineNo && a.ItemCode == headerLine.ItemInfo.ItemID.Code).FirstOrDefault();
                        if (reqLine == null)
                        {
                            return JsonUtil.GetFailResponse("dtoLine == null", debugInfo);
                        }
                        debugInfo.AppendLine("遍历行-获取到reqLine");
                        if (reqLine.ShipQty <= 0)
                        {
                            return JsonUtil.GetFailResponse("dtoLine.ShipQty <= 0", debugInfo);
                            //srcDocInfo.CurShipQty1 = soLine.OrderByQtyTU;//出货数量
                        }
                        else
                        {
                            srcDocInfoDTO.CurShipQty1 = reqLine.ShipQty;//出货数量
                        }
                        srcDocInfoDTO.CurShipQty2 = decimal.Zero;//传0
                        srcDocInfoDTO.SrcDocLineID = headerLine.ID; //来源销售订单子行id
                        if (headerLine.WH != null)
                            srcDocInfoDTO.WH = headerLine.WH.ID;//仓库
                        else
                        {
                            debugInfo.AppendLine(reqLine.WHCode);
                            Warehouse wh = Warehouse.FindByCode(headerLine.Org, reqLine.WHCode);
                            if (wh == null)
                            {
                                return JsonUtil.GetFailResponse("wh==null", debugInfo);
                            }
                            srcDocInfoDTO.WH = wh.ID;
                        }
                        srcDocInfoDTO.SrcDocType = UFIDA.U9.SM.Ship.ShipSrcDocTypeEnum.SO;//来源类型
                        srcDocInfoList.Add(srcDocInfoDTO);
                    }
                }
                debugInfo.AppendLine("遍历行end");
                if (U9Contant.IS_TEST)
                {
                    proxy.ShipDate = DateTime.Parse("2020-01-02");
                }
                else
                {
                    proxy.ShipDate = DateTime.Now;
                }
                proxy.SrcDocInfoDTOList = srcDocInfoList;
                debugInfo.AppendLine("invoke,start...");
                List<long> idList = proxy.Do();
                debugInfo.AppendLine("invoke,end...");
                if (idList != null && idList.Count > 0)
                    foreach (var id in idList)
                    {
                        UFIDA.U9.SM.Ship.Ship ship = UFIDA.U9.SM.Ship.Ship.Finder.FindByID(id);
                        if (ship != null)
                        {
                            res.AppendLine(ship.DocNo);
                            using (ISession session = Session.Open())
                            {
                                foreach (var item in ship.ShipLines)
                                {

                                    PullShipRequest.PullShipLine curLine = reqHeader.PullShipLines.Where(a => a.ItemCode == item.ItemInfo.ItemCode && a.SrcLineNo == item.SrcDocLineNo).FirstOrDefault();
                                    if (curLine != null)
                                    {
                                        item.LotInfo.LotMaster = LotMaster.Finder.Find(string.Format("Code='{0}' and ItemCode='{1}'", curLine.LotCode, curLine.ItemCode));
                                        //写入扩展字段
                                        //item.DescFlexField.PrivateDescSeg1 = curLine.LotCode;
                                    }
                                }
                                session.Commit();
                            }
                        }
                    }
            }
            catch (Exception ex)
            {

                return JsonUtil.GetFailResponse(ex.ToString(), debugInfo);
            }
            return JsonUtil.GetSuccessResponse(res.ToString(), debugInfo);
		}		
	}

	#endregion
	
	
}