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

    /// <summary>
    /// CreateMaterialInDocSV partial 
    /// </summary>	
    public partial class CreateMaterialInDocSV
    {
        internal BaseStrategy Select()
        {
            return new CreateMaterialInDocSVImpementStrategy();
        }
    }

    #region  implement strategy	
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateMaterialInDocSVImpementStrategy : BaseStrategy
    {
        public CreateMaterialInDocSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateMaterialInDocSV bpObj = (CreateMaterialInDocSV)obj;


            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateMaterialDeliveryDocRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateMaterialDeliveryDocRequest>(bpObj.JsonRequest);
            }
            catch (Exception ex)
            {
                return JsonUtil.GetFailResponse("JSON格式错误");
            }
            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine("strat...");
            debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

            UFIDA.U9.IssueNew.MaterialDeliveryBP.CreateMaterislDeliveryDocForWOOrPickListsBP sv = new UFIDA.U9.IssueNew.MaterialDeliveryBP.CreateMaterislDeliveryDocForWOOrPickListsBP();
            sv.MOPickLists = new List<MOPickList.EntityKey>();
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

                sv.MOPickLists.Add(_srcPlist.Key);
            }

            List<MaterialDeliveryDoc.EntityKey> listIssueKeyDTO = sv.Do();
            if (listIssueKeyDTO == null || listIssueKeyDTO.Count == 0)
            {
                return JsonUtil.GetFailResponse("listIssueKeyDTO == null || listIssueKeyDTO.Count == 0", debugInfo);
            }
            foreach (var dKey in listIssueKeyDTO)
            {
                res.AppendLine(dKey.ToString());
            }

            return JsonUtil.GetSuccessResponse(res.ToString(), debugInfo);
        }
    }

    #endregion


}