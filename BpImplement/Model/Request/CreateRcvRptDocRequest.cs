using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Model.Request
{
    /// <summary>
    /// 成品入库
    /// </summary>
    class CreateRcvRptDocRequest
    {
        public string RcvDocNo { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string Remark { get; set; }
        public List<CreateRcvRptDocLine> CreateRcvRptDocLines { get; set; }

        public class CreateRcvRptDocLine
        {
            public string SrcDocNo { get; set; }
            public string WHCode { get; set; }
            public string LotCode { get; set; }
            public decimal RcvQty { get; set; }
            public string ItemCode { get; set; }
        }
    }
}
