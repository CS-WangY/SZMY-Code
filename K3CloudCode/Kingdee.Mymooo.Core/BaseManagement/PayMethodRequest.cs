using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class PayMethodInfo : BaseInfo
    {
        public PayMethodInfo(string code, string name)
        {
            this.NumberFieldName = "FNumber";
            this.DocumentStatusFieldName = "FDocumentStatus";
            this.ForbidStatusFieldName = "FForbidStatus";
            this.IdFieldNmber = "FID";
            this.NumberKDDbType = KDDbType.String;
            this.Code = code;
            this.Name = name;
            this.FormId = "BD_RecCondition";
        }
    }

    public class PayWayInfo : BaseInfo
    {
        public PayWayInfo(string code, string name)
        {
            this.NumberFieldName = "FNumber";
            this.DocumentStatusFieldName = "FDocumentStatus";
            this.ForbidStatusFieldName = "FForbidStatus";
            this.IdFieldNmber = "FID";
            this.NumberKDDbType = KDDbType.String;
            this.Code = code;
            this.Name = name;
            this.FormId = "BD_SETTLETYPE";
        }
    }
}
