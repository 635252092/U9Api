using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Model.Request
{
    class CreateRcvFromRMRRequest
    {
        public string RcvDocNo { get; set; }
        public string Remark { get; set; }
        public string SrcDocNo { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public List<CreateRcvFromRMRLine> CreateRcvFromRMRLines { get; set; }
        public class CreateRcvFromRMRLine {
            public int SrcSubLineNo { get; set; }

            public string ItemCode { get; set; }
            public string LotCode { get; set; }  
            /// <summary>
            /// 来源行号
            /// </summary>
            public int SrcLineNo { get; set; }
            /// <summary>
            /// 实收数量
            /// </summary>
            public decimal RcvQty { get; set; }
            /// <summary>
            /// 仓库code
            /// </summary>
            public string WHCode { get; set; }
        }
    }
}
