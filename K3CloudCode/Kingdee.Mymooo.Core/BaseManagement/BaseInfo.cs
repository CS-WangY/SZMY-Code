using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class BaseInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long MasterId { get; set; }

        public string MasterFieldName { get; set; }

        public string Code { get; set; }

        public string IdFieldNmber { get; set; }

        public string NumberFieldName { get; set; }

        public string OrgNumberFieldName { get; set; }

        public long UseOrgId { get; set; }

        public KDDbType NumberKDDbType { get; set; }

        public string DocumentStatus { get; set; }
        public string ForbidStatus { get; set; }

        public string DocumentStatusFieldName { get; set; }
        public string ForbidStatusFieldName { get; set; }
        public string FormId { get;  set; }
    }
}
