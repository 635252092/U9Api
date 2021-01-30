namespace U9Api.CustSV
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using U9Api.CustSV.Base;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.CBO.Enums;
    using UFIDA.U9.CBO.Pub.Controller;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.InvDoc.MiscShip;
    using UFIDA.U9.ISV.MiscShipISV;
    using UFIDA.U9.Lot;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.PL.Engine;
    using UFSoft.UBF.Util.Context;

    /// <summary>
    /// CreateMiscShipCustSV partial 
    /// </summary>	
    public partial class CreateMiscShipCustSV
    {
        internal BaseStrategy Select()
        {
            return new CreateMiscShipCustSVImpementStrategy();
        }
    }

    #region  implement strategy	
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateMiscShipCustSVImpementStrategy : BaseStrategy
    {
        public CreateMiscShipCustSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateMiscShipCustSV bpObj = (CreateMiscShipCustSV)obj;

            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateMiscShipRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateMiscShipRequest>(bpObj.JsonRequest);
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
                    CommonCreateMiscShip client = new CommonCreateMiscShip();
                    client.ContextFrom = new UFIDA.U9.CBO.Pub.Controller.ContextDTO();
                    client.ContextFrom.CultureName = Context.LoginLanguageCode;
                    client.ContextFrom.EntCode = reqHeader.EntCode;
                    client.ContextFrom.OrgCode = Context.LoginOrg.Code;
                    client.ContextFrom.OrgID = Context.LoginOrg.ID;

                    UFIDA.U9.Base.UserRole.User user = UFIDA.U9.Base.UserRole.User.Finder.FindByID(Context.LoginUserID);

                    if (user == null)
                    {
                        return JsonUtil.GetFailResponse("user == null", debugInfo);
                    }
                    client.ContextFrom.UserCode = user.Code;
                    client.ContextFrom.UserID = user.ID;

                    IC_MiscShipmentDTO dtoHeader = new IC_MiscShipmentDTO();

                    //头
                    dtoHeader.MiscShipDocType = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                    //dto.MiscShipDocType.Code = "MiscShip001";
                    MiscShipDocType miscShipDocType = MiscShipDocType.Finder.Find(string.Format("Org={0} and Code='{1}'", Context.LoginOrg.ID, reqHeader.DocTypeCode));
                    if (miscShipDocType == null)
                    {
                        return JsonUtil.GetFailResponse(reqHeader.DocTypeCode + U9Contant.NoFindDocType);
                    }
                    dtoHeader.MiscShipDocType.ID = miscShipDocType.ID;
                    if (!string.IsNullOrEmpty(reqHeader.WmsDocNo))
                        dtoHeader.DocNo = reqHeader.WmsDocNo;
                    dtoHeader.Org = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                    dtoHeader.Org.ID = Context.LoginOrg.ID;
                    dtoHeader.BenefitOrg = Context.LoginOrg.ID;
                    dtoHeader.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);

                    dtoHeader.Memo = reqHeader.Remark;
                    dtoHeader.MiscShipLs = new List<IC_MiscShipmentLDTO>();
                    dtoHeader.SysState = ObjectState.Inserted;

                    foreach (var reqLine in reqHeader.CreateMiscShipLines)
                    {
                        IC_MiscShipmentLDTO dtoLine = new IC_MiscShipmentLDTO();

                        //行
                        Warehouse benefitWH = Warehouse.FindByCode(Context.LoginOrg, reqLine.BenefitWHCode);
                        if (benefitWH == null)
                        {
                            return JsonUtil.GetFailResponse(reqLine.BenefitWHCode + ":受益仓库不能为空", debugInfo);
                        }
                        dtoLine.BenefitWh = benefitWH.ID;
                        dtoLine.IsMFG = miscShipDocType.IsMFG;

                        dtoLine.BenefitDept = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                        UFIDA.U9.CBO.HR.Department.Department department = UFIDA.U9.CBO.HR.Department.Department.FindByCode(reqLine.BenefitDeptCode);
                        if (department == null)
                        {
                            return JsonUtil.GetFailResponse(reqLine.BenefitDeptCode + "：受益部门不能为空", debugInfo);
                        }
                        dtoLine.BenefitDept.ID = department.ID;
                        //if (benefitWH.Manager != null)
                        //{
                        //    dtoLine.BenefitPsn = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                        //    dtoLine.BenefitPsn.ID = benefitWH.Manager.ID;
                        //}
                        dtoLine.BenefitOrg = Context.LoginOrg.ID;

                        dtoLine.ItemInfo = new UFIDA.U9.CBO.SCM.Item.ItemInfo();
                        dtoLine.ItemInfo.ItemCode = reqLine.ItemCode;

                        dtoLine.CostUOMQty = reqLine.ItemQty;
                        dtoLine.StoreUOMQty = reqLine.ItemQty;
                        dtoLine.OwnerOrg = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                        dtoLine.OwnerOrg.ID = Context.LoginOrg.ID;
                        dtoLine.StoreType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
                        dtoLine.SUToCURate = 1;
                        dtoLine.SUToSBURate = 1;

                        dtoLine.Wh = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
                        dtoLine.Wh.Code = reqLine.FromWHCode;
                        //批号赋值
                        long lotKey = 0L;
                        if (!string.IsNullOrEmpty(reqLine.LotCode) && CommonUtil.IsNeedLot(reqLine.ItemCode, Context.LoginOrg.ID))
                        {
                            LotMaster curLot = CommonUtil.GetLot(reqLine.LotCode, reqLine.ItemCode, reqLine.FromWHCode, LotSnStatusEnum.I_MiscShipment.Value);
                            lotKey = curLot.ID;
                        }
                        if (lotKey > 0L)
                        {
                            dtoLine.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfo();
                            dtoLine.LotInfo.LotCode = reqLine.LotCode;
                            dtoLine.LotInfo.LotMaster = new UFIDA.U9.Base.PropertyTypes.BizEntityKey();
                            dtoLine.LotInfo.LotMaster.EntityID = lotKey;
                        }
                        dtoLine.SysState = ObjectState.Inserted;
                        dtoLine.MiscShipBins = new List<IC_MiscShipBinDTO>();
                        dtoLine.MiscShipmentCost = new List<IC_MiscShipmentCostDTO>();
                        dtoHeader.MiscShipLs.Add(dtoLine);
                    }
                    client.MiscShipmentDTOList = new List<IC_MiscShipmentDTO>();
                    client.MiscShipmentDTOList.Add(dtoHeader);

                    List<CommonArchiveDataDTO> listRes = client.Do();
                    if (listRes == null || listRes.Count == 0)
                    {
                        return JsonUtil.GetFailResponse("resDto == null || resDto.Count == 0", debugInfo);
                    }
                    CommonArchiveDataDTO doc = listRes[0];
                    //修改批号的有效期
                    string sql = string.Format(@"UPDATE line SET LotInfo_DisabledDatetime=NULL from  InvDoc_MiscShipL line 
INNER JOIN InvDoc_MiscShip header on line.MiscShip=header.ID
where header.Org = {0} AND header.DocNo = '{1}'", Context.LoginOrg.ID, doc.Code);
                    CommonUtil.RunUpdateSQL(sql);
                    debugInfo.AppendLine(sql);

                    if (reqHeader.IsAutoApprove)
                    {
                        U9Api.CustSV.ApproveMiscShipCustSV sv2 = new
                          ApproveMiscShipCustSV();
                        CommonApproveRequest req = new CommonApproveRequest();
                        req.DocNo = doc.Code;
                        sv2.JsonRequest = JsonUtil.GetJsonString(req);
                        res.Clear();
                        res.Append(sv2.Do());
                        scope.Commit();
                        return res.ToString();
                    }
                    scope.Commit();
                    return JsonUtil.GetSuccessResponse(doc.Code, "", debugInfo);
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