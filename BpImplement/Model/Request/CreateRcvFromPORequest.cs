using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Model.Request
{
    class CreateRcvFromPORequest
    {
        public string RcvDocNo { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string Remark { get; set; }
        public List<CreateRcvFromPOLine> CreateRcvFromPOLines { get; set; }

        public class CreateRcvFromPOLine
        {
            public string TotalLotQty { get; set; }

            public int SrcSubLineNo { get; set; }

            public string SrcDocNo { get; set; }
            public string WHCode { get; set; }
            public int SrcLineNo { get; set; }
            public string LotCode { get; set; }
            public decimal RcvQty { get; set; }
            public string ItemCode { get; set; }
        }
    }
}
