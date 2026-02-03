using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.BaseManagement;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{

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

    public class POOrder
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FFormId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DocumentStatus { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string BillTypeId_Id { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public List<POOrderEntryItem> POOrderEntry { get; set; }
    }

}
