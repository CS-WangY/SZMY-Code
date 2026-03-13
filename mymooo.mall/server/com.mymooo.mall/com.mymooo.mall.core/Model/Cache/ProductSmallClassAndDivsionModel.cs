using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using Elastic.Clients.Elasticsearch.Core.TermVectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Cache
{
    public class ProductSmallClassAndDivsionModel
    {
        public ProductSmallClass? ProductSmallClass { get; set; }
        public DivsionSupplyResponse? DivsionSupplyDtos { get; set; }

        public List<string> Engineers { get; set; } = [];
    }
}
