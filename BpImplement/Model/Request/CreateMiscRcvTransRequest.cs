using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U9Api.CustSV.Model.Request
{
    class CreateMiscRcvTransRequest
    {
        public string DocTypeCode { get; set; }
        public string BusinessDate { get; set; }
        public bool IsAutoApprove { get; set; }
        public string Remark { get; set; }
        public string WmsDocNo { get; set; }
        public List<CreateMiscRcvTransLine> CreateMiscRcvTransLines { get; set; }

        public class CreateMiscRcvTransLine

        {
            public bool IsZeroCost { get; set; }
            /// <summary>
            /// 受益存储地点	
            /// </summary>
            public string BenefitWHCode { get; set; }
            /// <summary>
            /// 受益部门	
            /// </summary>
            public string BenefitDeptCode { get; set; }

            /// <summary>
            /// 存储地点	
            /// </summary>
            public string FromWHCode { get; set; }
            public string LotCode { get; set; }
            public decimal ItemQty { get; set; }
            public string ItemCode { get; set; }
        }
    }
}
