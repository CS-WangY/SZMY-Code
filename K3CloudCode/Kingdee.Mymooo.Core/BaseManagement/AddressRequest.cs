using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class AddressRequest
    {
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Code { get; set; } 
        public string Receiver { get; set; }
        public long AddressId { get; set; }

        public int IsDefault { get; set; }
    }
}
