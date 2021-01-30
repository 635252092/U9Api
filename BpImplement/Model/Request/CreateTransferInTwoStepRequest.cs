using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U9Api.CustSV.Model.Request
{
    class CreateTransferInTwoStepRequest
    {
        public string OutOrg { get; set; }

        public string WmsDocNo { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string Remark { get; set; }
        public List<CreateTransferInTwoStepLine> CreateTransferInTwoStepLines { get; set; }

        public class CreateTransferInTwoStepLine
        {
            public string SrcDocNo { get; set; }
            //public int SrcSubLineNo { get; set; }

            public string WHCode { get; set; }
            //public int SrcLineNo { get; set; }
            //public string LotCode { get; set; }
            //public decimal ItemQty { get; set; }
            //public string ItemCode { get; set; }
        }
    }
}
