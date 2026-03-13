using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{
    public class CompanyGradeDiscountResponse
    {

        public decimal Discount { get; set; }


        public int ProductTypeId { get; set; }  //第一优先
        public int ProductId { get; set; }   // 第二优先

        public int ProductCategoryId { get; set; } //第三优先

    }

    public class GradePriceDiscout : CompanyGradeDiscountResponse
    {
        public int Leavel { get; set; }
    }
}
