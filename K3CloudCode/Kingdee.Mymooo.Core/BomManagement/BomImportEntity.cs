using Kingdee.Mymooo.Core.BaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BomManagement
{
    public class BomImportEntity
    {
        public string FProductCode { get; set; }
        public string FParentID { get; set; }
        public string FNumber { get; set; }
        public string FName { get; set; }
        public string FCount { get; set; }
        public decimal FDENOMINATOR { get; set; }
        public decimal FSCRAPRATE { get; set; }
        public string FBaseUnitId { get; set; }
        public string FStockUnitId { get; set; }
        public string ProductSmallClassName { get; set; }
        public string ProductSmallClassID { get; set; }
        public decimal ImportAmount { get; set; }
        /// <summary>
        /// 1外购2自制
        /// </summary>
        public int ErpClsID { get; set; }

		public MaterialInfo material { get; set; }
    }
}
