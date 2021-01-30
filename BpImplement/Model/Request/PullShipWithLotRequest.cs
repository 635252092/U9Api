using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Model.Request
{
   public class PullShipWithLotRequest
    {
       public string CustomerCode { get; set; }
        public string SrcDocNo { get; set; }
        public string DocTypeCode { get; set; }
        public string WHCode { get; set; }
        public List<PullShipLine> PullShipLines { get; set; }

        public class PullShipLine
        {
            public string SrcDocNo { get; set; }
            public int SrcLineNo { get; set; }
            public string LotCode { get; set; }
            public decimal ShipQty { get; set; }
            public string ItemCode { get; set; }

        }
    }
}
