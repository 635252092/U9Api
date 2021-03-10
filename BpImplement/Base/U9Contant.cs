using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Base
{
   public class U9Contant
    {
       public static string SUCCESS_CODE = "200";
       public static string Fail_CODE = "-1";
       public static string CUST_SUCCESS_MSG = "CustSV执行成功";
       //public static string CUST_FAIL_MSG = "CustSV执行成功";
       public static bool IS_TEST = false;
        public static string LotEntityType = "UFIDA.U9.Lot.LotMaster";

        public static string NoFindDoc = "：没有找到订单";
        public static string NoFindWH = "：没有找到仓库";
        public static string NoFindWHDept = "仓库下面部门不能为空";
        public static string NoFindWHMan = "仓库下面管理员不能为空";

        public static string NoFindLot = "：没有找到批号";
        public static string NoFindOrg = ":没有找到组织";
        public static string NoFindItemMaster = "：没有找到料品";
        public static string NoFindDocType = "：没有找到单据类型";
        public static string RcvCountOver = "创建的收货单数量大于1，请检查供应商是否一致";
        public static string U9Fail_NoResponse = "创建失败，请检查来源单据、单据类型是否异常";
    }
}
