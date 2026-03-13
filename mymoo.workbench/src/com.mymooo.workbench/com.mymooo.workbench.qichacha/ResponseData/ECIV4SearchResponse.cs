using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.qichacha.ResponseData
{
    public class ECIV4SearchResponse
    {
        public string OrderNumber { get; set; }
        public Page Paging { get; set; }
        public ECIV4SearchResult[] Result { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public class Page
        {
            public int PageSize { get; set; }
            public int PageIndex { get; set; }
            public int TotalRecords { get; set; }
        }

        public class ECIV4SearchResult
        {
            public string KeyNo { get; set; }
            public string Name { get; set; }
            public string OperName { get; set; }
            public string StartDate { get; set; }
            public string Status { get; set; }
            public string No { get; set; }
            public string CreditCode { get; set; }
        }
    }

}
