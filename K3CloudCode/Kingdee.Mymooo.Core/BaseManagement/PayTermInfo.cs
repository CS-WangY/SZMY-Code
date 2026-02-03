using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class PayTermInfo : BaseInfo
    {
        public PayTermInfo(string code, string name)
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
}
