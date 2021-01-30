namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Util.Context;
    using UFIDA.U9.SM.RMR;
    using UFIDA.U9.CBO.SCM.Customer;
    using UFIDA.U9.Base.Currency;
    using UFIDA.U9.Base;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model;
    using UFIDA.U9.SM.Ship;
    using UFIDA.U9.CBO.SCM.Enums;
    using U9Api.CustSV.Model.Request;

    /// <summary>
    /// CreateRMRSV partial 
    /// </summary>	
    public partial class CreateRMRSV
    {
        internal BaseStrategy Select()
        {
            return new CreateRMRSVImpementStrategy();
        }
    }

    #region  implement strategy
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateRMRSVImpementStrategy : BaseStrategy
    {
        public CreateRMRSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateRMRSV bpObj = (CreateRMRSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateRMRRequest reqHeader = JsonUtil.GetJsonObject<CreateRMRRequest>(bpObj.JsonRequest);
            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));


            try
            {

                //说明:反编译UFIDA.U9.SM.SMBP.dll 可从UFIDA.U9.SM.RMR命名空间的 CreateRMR类中查看到创建销售退回申请单方法。
                //CreateRMR createRMR = new CreateRMR();//创建退回申请单BP
                //List<RMRHeadDTO> headDTOList = new List<RMRHeadDTO>();//创建的申请单头入口参数DTO集合
                //List<RMRInfoDTO> rmrDTOList = new List<RMRInfoDTO>();//创建的申请单行入口参数DTO集合
                //RMRHeadDTO headDTO = new RMRHeadDTO();//创建的申请单头入口参数DTO

                //RMRHead head = new RMRHead();//表头参数退回申请单
                //查找收货单
                Ship s = Ship.Finder.Find(string.Format("Org={0} and DocNo='{1}'", Context.LoginOrg.ID, reqHeader.SrcDocNo));

                if (s == null || s.ID < 1)
                {
                    return JsonUtil.GetFailResponse(String.Format("创建退回申请单时，未根据来源单号{0}找到来源销售订单行", reqHeader.SrcDocNo), debugInfo);
                }
                CreateRMR createRMR = new CreateRMR();//创建退回申请单BP
                createRMR.RMRHeadDTOs = new List<RMRHeadDTO>();

                //只做一单就好
                RMRHeadDTO rmrHeadDTO = new RMRHeadDTO();
                rmrHeadDTO.RMRHead = new RMRHead();
                rmrHeadDTO.RMRHead.DocumentType= RMRDocType.Finder.Find("Code='G0001'");//单据类型参数
                //head.DocumentType = RMRDocType.Finder.Find(string.Format("Code='{0}'", reqHeader.DocTypeCode));//单据类型参数
                rmrHeadDTO.RMRHead.Customer = s.OrderBy;//客户参数
                rmrHeadDTO.RMRHead.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
                rmrHeadDTO.RMRHead.RequireDate = DateTime.Parse(reqHeader.BusinessDate);
                rmrHeadDTO.RMRHead.TC = s.TC;//交易币参数
                rmrHeadDTO.RMRHead.SuggestType = UFIDA.U9.CBO.SCM.Enums.RMRProcessTypeEnum.RcvNoRtnRMA;//申请类型	
                rmrHeadDTO.RMRHead.ApprovedType = UFIDA.U9.CBO.SCM.Enums.RMRProcessTypeEnum.RcvNoRtnRMA;
                rmrHeadDTO.RMRHead.SrcDocType = RMRSrcDocTypeEnum.Shipment;

                int rowNo = 0, rowStep = 10;
                rmrHeadDTO.RMRHead.RMRList = new List<RMR>();
                foreach (var item in s.ShipLines)
                {
                    RMR rmr = new RMR();
                    rmrHeadDTO.RMRHead.RMRList.Add(rmr);

                    rowNo += rowStep;
                    rmr.RMADocLineNo = rowNo;
                    rmr.CustomerItemCode = item.CustomerItemCode;
                    rmr.CustomerItemName = item.CustomerItemName;
                    rmr.Customer = s.OrderBy;
                    rmr.ItemInfo = item.ItemInfo;
                    rmr.Lot = item.LotInfo.LotMaster;
                    rmr.SuggestType = rmrHeadDTO.RMRHead.SuggestType;
                    rmr.ApplyQtyTU1 = item.ShipQtyInvAmount;
                    rmr.ApprovedType = rmrHeadDTO.RMRHead.ApprovedType;
                    rmr.ApplyPriceTC = item.OrderPrice;
                    rmr.ApplyMoneyTC = rmr.ApplyPriceTC * rmr.ApplyQtyTU1;
                    rmr.SrcDoc = item.ID;
                }

                // .....类似TestSMRMR()方法赋值表头、表体、子行


                createRMR.RMRHeadDTOs.Add(rmrHeadDTO);
                List<RMRHeadEntityDTO> listResult = createRMR.Do();//创建销售退回申请单
                if (listResult != null && listResult.Count > 0)
                {
                    foreach (var item in listResult)
                    {
                        if (item.RMRHead != null)
                        {
                            RMRHead r = RMRHead.Finder.FindByID(item.RMRHead.ID);
                            res.AppendLine(r.DocNo);
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