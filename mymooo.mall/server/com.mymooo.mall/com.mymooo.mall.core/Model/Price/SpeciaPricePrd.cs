using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price
{
    public class SpeciaPricePrd
    {  
        public long InquiryItemId { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public bool IsSpecialPrice { get; set; } 

        public Guid? CompanyId { get; set; }


    }
}
