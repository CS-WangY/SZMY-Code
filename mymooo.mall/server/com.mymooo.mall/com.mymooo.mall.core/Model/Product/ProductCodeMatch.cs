using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Product
{
    public class ProductCodeMatch
    {
        public string  ProductCode { get; set; } = string.Empty;

        public List<string> ProductCodeMatchList { get; set; } = [];
    }
}
