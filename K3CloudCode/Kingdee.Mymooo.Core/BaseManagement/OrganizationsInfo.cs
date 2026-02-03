using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class OrganizationsInfo : BaseInfo
    {
        public OrganizationsInfo(string code, string name)
        {
            this.NumberFieldName = "FNUMBER";
            this.DocumentStatusFieldName = "FDocumentStatus";
            this.IdFieldNmber = "FOrgId";
            this.NumberKDDbType = KDDbType.String;
            this.Code = code;
            this.Name = name;
            this.FormId = "ORG_Organizations";
        }
    }
}
