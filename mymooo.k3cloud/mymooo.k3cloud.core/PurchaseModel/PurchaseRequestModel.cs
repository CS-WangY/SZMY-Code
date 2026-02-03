using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace mymooo.k3cloud.core.PurchaseModel
{
    /// <summary>
    /// POOrderEntryItem
    /// </summary>
    public class POOrderEntryItem
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long MaterialId_Id { get; set; }
    }
    /// <summary>
    /// 采购订单
    /// </summary>
    public class POOrder
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FFormId { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string BillNo { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string DocumentStatus { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string BillTypeId_Id { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public List<POOrderEntryItem> POOrderEntry { get; set; } = [];
    }
}
