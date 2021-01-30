using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Model.Request
{
    public class PullShipBatchRequest
    {
        public string DocNo { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string Remark { get; set; }
        public string RcvContact { get; set; }
        public List<PullShipBatchLine> PullShipLines { get; set; }

        public class PullShipBatchLine
        {
            public string SrcOrg { get; set; }
            public string SrcDocNo { get; set; }
            public int SrcLineNo { get; set; }
            public int SrcSubLineNo { get; set; }
            public string ItemCode { get; set; }
            public string LotCode { get; set; }
            public decimal ShipQty { get; set; }
            public string WHCode { get; set; }
        }
    }
}
