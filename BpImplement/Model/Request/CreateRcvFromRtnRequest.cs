using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U9Api.CustSV.Model.Request
{
    class CreateRcvFromRtnRequest
    {
        public string DocTypeCode { get; set; }
        public string RcvDocNo { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string Remark { get; set; }
        public List<CreateRcvFromRtnLine> CreateRcvFromRtnLines { get; set; }

        public class CreateRcvFromRtnLine
        {
          
            public string SrcDocNo { get; set; }
            public string WHCode { get; set; }
            public int SrcLineNo { get; set; }
            public string LotCode { get; set; }
            //public decimal RtnFillQty { get; set; }
            //public decimal RtnDeductQty { get; set; }
            public decimal ItemQty { get; set; }

            public string ItemCode { get; set; }

        }
    }
}
