using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Model.Request
{
    class CreateLotRequest
    {
        public string LotCode { get; set; }
        public string ItemCode { get; set; }
        public string WHCode { get; set; }
        public int LotSnStatusEnumValue { get; set; }
        public bool IsCreate { get; set; }
        public string OrgCode { get; set; }
    }
}
