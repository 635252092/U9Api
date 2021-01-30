using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using U9Api.CustSV.Model.Request;
using UFIDA.U9.Base;
using UFIDA.U9.Lot;
using UFSoft.UBF.Util.DataAccess;

namespace U9Api.CustSV.Utils
{
    class CommonUtil
    {
        public static UFIDA.U9.Base.SOB.SOBAccountingPeriod GetSOBAccountPeriodEntity(DateTime DateInfo, long orgID)
        {
            StringBuilder _qStr = new StringBuilder(128);
            _qStr.Append("SetofBooks.Org=" + orgID);
            _qStr.Append(" and @DateInfo between AccountPeriod.FromDate and AccountPeriod.ToDate");
            //if (string.IsNullOrEmpty(SOBCode))
            //    _qStr.Append(" and SetofBooks.SOBType=" + UFIDA.U9.Base.SOB.SOBTypeEnum.PrimarySOB.Value);
            //else
            //    _qStr.Append(" and SetofBooks.Code='" + SOBCode + "'");

            _qStr.Append(" and SetofBooks.SOBType=" + UFIDA.U9.Base.SOB.SOBTypeEnum.PrimarySOB.Value);

            UFIDA.U9.Base.SOB.SOBAccountingPeriod SobAccList = UFIDA.U9.Base.SOB.SOBAccountingPeriod.Finder.Find(_qStr.ToString(), new UFSoft.UBF.PL.OqlParam(DateInfo));
            return SobAccList;
        }

        public static LotMaster GetLot(string lotCode, string itemCode, string whCode, int enumValue)
        {
            return GetLot(lotCode, itemCode, whCode, enumValue, true);
        }
        public static LotMaster GetLot(string lotCode, string itemCode, string whCode, int enumValue, bool isCreate)
        {
            return GetLot(lotCode, itemCode, whCode, enumValue, isCreate,Context.LoginOrg.Code);
        }
        public static LotMaster GetLot(string lotCode, string itemCode, string whCode, int enumValue, bool isCreate,string orgCode)
        {
            if (string.IsNullOrEmpty(lotCode) || string.IsNullOrEmpty(itemCode) || string.IsNullOrEmpty(whCode))
                return null;

            CreateLotSV sv3 = new CreateLotSV();
            CreateLotRequest req = new CreateLotRequest();
            req.LotCode = lotCode;
            req.ItemCode = itemCode;
            req.LotSnStatusEnumValue = enumValue;
            req.WHCode = whCode;
            req.IsCreate = isCreate;
            req.OrgCode = orgCode;
            sv3.JsonRequest = JsonUtil.GetJsonString(req);
            string lotID = sv3.Do();
            if (string.IsNullOrEmpty(lotID))
            {
                return null;
            }
            else
            {
                return UFIDA.U9.Lot.LotMaster.Finder.FindByID(lotID);
            }
        }
        public static bool IsNeedLot(string itemCode, string orgCode)
        {
            UFIDA.U9.Base.Organization.Organization organization = UFIDA.U9.Base.Organization.Organization.FindByCode(orgCode);
            if (organization == null)
                throw Base.U9Exception.GetException(orgCode + Base.U9Contant.NoFindOrg);
            return IsNeedLot(itemCode, organization.ID);
        }
        public static bool IsNeedLot(string itemCode, long orgID)
        {
            UFIDA.U9.CBO.SCM.Item.ItemMaster itemMaster = UFIDA.U9.CBO.SCM.Item.ItemMaster.Finder.Find(string.Format("Org={0} and Code='{1}'", orgID, itemCode));
            if (itemMaster == null)
            {
                throw Base.U9Exception.GetException(itemCode + Base.U9Contant.NoFindItemMaster);
            }
            bool isNeedLot = true;
            if (itemMaster.ItemFormAttribute == UFIDA.U9.CBO.SCM.Item.ItemTypeAttributeEnum.Assets)
            {
                isNeedLot = false;
            }

            return isNeedLot;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateSql"></param>
        public static void RunUpdateSQL(string updateSql)
        {
            //D:\yonyou\U9V50\Portal\ApplicationServer\bin\UFSoft.UBF.Util.DataAccess.dll
            DataAccessor.RunSQL(DataAccessor.GetConn(), updateSql, null);
        }
    }
}
