using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class CustomerInfo : BaseInfo
    {
        public CustomerInfo(string code, string name)
        {
            this.NumberFieldName = "FNumber";
            this.DocumentStatusFieldName = "FDocumentStatus";
            this.ForbidStatusFieldName = "FForbidStatus";
            this.IdFieldNmber = "FCUSTID";
            this.NumberKDDbType = KDDbType.String;
            this.Code = code;
            this.Name = name;
            this.FormId = "BD_Customer";
        }
    }
}
