using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U9Api.CustSV.Model.Request
{
    class RMAPullShipRequest
    {
        public string WmsDocNo { get; set; }
        public string Remark { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string EntCode { get; set; }
        public bool IsAutoApproveRMA { get; set; }
        public string RcvContact { get; set; }
        public List<RMAPullShipLine> RMAPullShipLines { get; set; }
        public class RMAPullShipLine
        {
            public string SrcDocNo { get; set; }
            public string ItemCode { get; set; }
            public string LotCode { get; set; }
            public int SrcLineNo { get; set; }
            public decimal ItemQty { get; set; }
            public string WHCode { get; set; }
        }
    }
}
