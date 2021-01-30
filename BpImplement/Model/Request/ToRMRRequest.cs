using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U9Api.CustSV.Model.Request
{
  public  class ToRMRRequest
    {
        public string SrcDocType { get; set; }

        public List<ToRMRLine> ToRMRLines { get; set; }

        public class ToRMRLine
        {
            public string SrcDocNo { get; set; }
            public int SrcLineNo { get; set; }
        }
    }
}
