using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U9Api.CustSV.Model.Request
{
   public class CreateTransferOutRequest
    {
        public string DocTypeCode { get; set; }

        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string Remark { get; set; }
        public string WmsDocNo { get; set; }
        public List<TransferOutLine> TransferOutLines { get; set; }

        public class TransferOutLine
        {
            public string TaskCode { get; set; }
            public string ProjectCode { get; set; }

            public string InOrg { get; set; }

            /// <summary>
            /// 调入存储地点
            /// </summary>
            public string ToWHCode { get; set; }
            /// <summary>
            /// 调出存储地点
            /// </summary>
            public string FromWHCode { get; set; }
            public string LotCode { get; set; }
            public decimal ItemQty { get; set; }
            public string ItemCode { get; set; }
        }
    }
}
