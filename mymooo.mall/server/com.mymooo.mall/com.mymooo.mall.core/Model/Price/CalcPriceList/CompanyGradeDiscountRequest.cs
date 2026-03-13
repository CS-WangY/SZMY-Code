using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{
    public class CompanyGradeDiscountRequest
    {
        public Guid? CompanyId { get; set; }

        public required List<int> Types { get; set; }

        public required List<int> ProductCategoryIds { get; set; }

    }
}
