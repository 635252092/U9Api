namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;
    using UFSoft.UBF.Business;
    using U9Api.CustSV.Utils;
    using U9Api.CustSV.Model.Request;
    using UFIDA.U9.Base;
    using UFIDA.U9.Lot;
    using U9Api.CustSV.Model.Response;
    using UFIDA.U9.CBO.SCM.PropertyTypes;
    using UFIDA.U9.Complete.RcvRptBP;
    using UFIDA.U9.MO.MO;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using U9Api.CustSV.Base;
    using UFIDA.U9.InvTrans.WhQoh;

    /// <summary>
    /// CreateLotSV partial 
    /// </summary>	
    public partial class CreateLotSV 
	{	
		internal BaseStrategy Select()
		{
			return new CreateLotSVImpementStrategy();	
		}		
	}

    #region  implement strategy	
    /// <summary>
    /// Impement Implement
    /// 
    /// </summary>	
    internal partial class CreateLotSVImpementStrategy : BaseStrategy
    {
        public CreateLotSVImpementStrategy() { }

        public override object Do(object obj)
        {
            CreateLotSV bpObj = (CreateLotSV)obj;
            if (string.IsNullOrEmpty(bpObj.JsonRequest))
            {
                 throw U9Exception.GetException("string.IsNullOrEmpty(bpObj.JsonRequest)");
            }
            CreateLotRequest reqHeader = null;
            try
            {
                reqHeader = JsonUtil.GetJsonObject<CreateLotRequest>(bpObj.JsonRequest);
            }
            catch (Exception ex)
            {
                 throw U9Exception.GetException("JSON格式错误");
            }
            StringBuilder res = new StringBuilder();
            StringBuilder debugInfo = new StringBuilder();

            if (string.IsNullOrEmpty(reqHeader.LotCode) || string.IsNullOrEmpty(reqHeader.ItemCode))
                 throw U9Exception.GetException("JSON格式错误");

            UFIDA.U9.Base.Organization.Organization organization = UFIDA.U9.Base.Organization.Organization.FindByCode(reqHeader.OrgCode);
            if(organization==null)
                throw U9Exception.GetException(reqHeader.OrgCode+U9Contant.NoFindOrg, debugInfo);

         

            UFIDA.U9.CBO.SCM.Item.ItemMaster itemMaster = UFIDA.U9.CBO.SCM.Item.ItemMaster.Finder.Find(string.Format("Org={0} and Code='{1}'", organization.ID, reqHeader.ItemCode));
            if (itemMaster == null)
            {
                 throw U9Exception.GetException(reqHeader.ItemCode+U9Contant.NoFindItemMaster, debugInfo);
            }
            //if (itemMaster.ItemFormAttribute == UFIDA.U9.CBO.SCM.Item.ItemTypeAttributeEnum.Assets)
            //{
            //    throw U9Exception.GetException(reqHeader.ItemCode + U9Contant.NoFindItemMaster, debugInfo);
            //}
            Warehouse wh = UFIDA.U9.CBO.SCM.Warehouse.Warehouse.FindByCode(organization, reqHeader.WHCode);
            if (wh == null)
            {
                 throw U9Exception.GetException(reqHeader.WHCode+U9Contant.NoFindWH);
            }
            //LotMaster lotMaster = LotMaster.Finder.Find(string.Format("LotCode='{0}' and DataOwnerOrg={1} and Warehouse={2}", reqHeader.LotCode, organization.ID, wh.ID));
            //LotMaster.EntityList listLotMaster = LotMaster.Finder.FindAll(string.Format("LotCode='{0}' and ItemOwnerOrg={1}", reqHeader.LotCode, organization.ID));

           //List<LotMaster> listLotMaster = new List<LotMaster>();
            HashSet<LotMaster> listLotMaster = new HashSet<LotMaster>();
            //查询在手量
            //WhQoh.EntityList listWhQoh = WhQoh.Finder.FindAll($"ItemOwnOrg={organization.ID} and LotInfo.LotCode='{reqHeader.LotCode}' and Wh={wh.ID}");
            WhQoh.EntityList listWhQoh = WhQoh.Finder.FindAll($"ItemInfo.ItemID={itemMaster.ID} and LotInfo.LotCode='{reqHeader.LotCode}' and Wh={wh.ID}");
            if (listWhQoh != null&& listWhQoh.Count>0)
            {
                foreach (var whQoh in listWhQoh)
                {
                    listLotMaster.Add(whQoh.LotInfo.LotMaster_EntityID);
                }
            }
           
            if (listLotMaster == null || listLotMaster.Count < 1)
            {
                if (!reqHeader.IsCreate)
                {
                    return null;
                }

                CreateLotOnlineSV createLotOnlineSVProxy = new CreateLotOnlineSV();
                UFIDA.U9.CBO.SCM.Lot.LotRequestDTO lotRequestDTOData = new UFIDA.U9.CBO.SCM.Lot.LotRequestDTO();
                lotRequestDTOData.Item = itemMaster.Key;
                lotRequestDTOData.ItemCode = itemMaster.Code;
                lotRequestDTOData.Warehouse = wh.Key;
                lotRequestDTOData.ItemOrg = organization.Key;
                lotRequestDTOData.UsingOrg = organization.Key;
                lotRequestDTOData.QueryTime = DateTime.Now;

                lotRequestDTOData.Status = UFIDA.U9.CBO.Enums.LotSnStatusEnum.GetFromValue(reqHeader.LotSnStatusEnumValue);
                lotRequestDTOData.Lot = reqHeader.LotCode;

                createLotOnlineSVProxy.LotRequestDTO = lotRequestDTOData;
                LotInfo lotInfoData = null;
                try
                {
                    lotInfoData = createLotOnlineSVProxy.Do();
                }
                catch (Exception ex)
                {
                    LogUtil.WriteDebugInfoLog(debugInfo.ToString());
                    throw Base.U9Exception.GetInnerException(ex);
                }

                return lotInfoData.LotMaster.EntityID.ToString();
            }
            else if (listLotMaster.Count > 1)
            { 
                throw new Exception("批号数量超过1个");
            }
            else
            {
                string lotID = string.Empty;
                foreach (var item in listLotMaster)
                {
                    lotID = item.ID.ToString();
                }
                return lotID;
            }
        }
    }

	#endregion
	
	
}