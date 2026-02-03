using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class LinkManRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public string Sex { get; set; }

        public string Mobile { get; set; }

        public string ProfessionName { get; set; }

        public string CustCode { get; set; }

        public string CompanyCode { get; set; }

        public bool IsValid { get; set; } = true;

        public string Address { get; set; }

    }
}
