using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U9Api.CustSV.Model.Request
{
    class DeleteCustRequest
    {
        public string DocTypeCode { get; set; }
        public string WmsDocNo { get; set; }
    }
}
