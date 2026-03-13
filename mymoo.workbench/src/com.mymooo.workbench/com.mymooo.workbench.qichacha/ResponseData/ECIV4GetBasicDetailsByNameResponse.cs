using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.qichacha.ResponseData
{
    public class ECIV4GetBasicDetailsByNameResponse
    {
        public string OrderNumber { get; set; }
        public ResponseResult Result { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public class ResponseResult
        {
            public string KeyNo { get; set; }
            public string Name { get; set; }
            public string No { get; set; }
            public string BelongOrg { get; set; }
            public string OperName { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Status { get; set; }
            public string Province { get; set; }
            public string UpdatedDate { get; set; }
            public string CreditCode { get; set; }
            public string RegistCapi { get; set; }
            public string EconKind { get; set; }
            public string Address { get; set; }
            public string Scope { get; set; }
            public string TermStart { get; set; }
            public string TeamEnd { get; set; }
            public string CheckDate { get; set; }
            public string OrgNo { get; set; }
            public string IsOnStock { get; set; }
            public dynamic StockNumber { get; set; }
            public dynamic StockType { get; set; }
            public dynamic OriginalName { get; set; }
            public string ImageUrl { get; set; }
            public string EntType { get; set; }
            public string RecCap { get; set; }
            public QccAreaDto Area { get; set; }
        }

        public class QccAreaDto
        {
            public string Province { get; set; }
            public string City { get; set; }
            public string County { get; set; }
        }
    }

}
