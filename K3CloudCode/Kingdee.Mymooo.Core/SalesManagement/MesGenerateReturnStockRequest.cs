using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    /// <summary>
    /// MES下推生成销售退货单
    /// </summary>

    public class MesGenerateReturnStockRequest
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 退货通知单号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 退货订单单号
        /// </summary>
        public string RetBillNo { get; set; }

        /// <summary>
        /// 明细
        /// </summary>
        public List<MesGenerateReturnStockDetEntity> Details { get; set; }

    }

    /// <summary>
    /// 退货明细
    /// </summary>
    public class MesGenerateReturnStockDetEntity
    {
        /// <summary>
        /// 明细ID
        /// </summary>
        public long EntryId { get; set; }

        /// <summary>
        /// 退货通知单序号
        /// </summary>
        public int BillSeq { get; set; }

        /// <summary>
        /// 退货通知单数量
        /// </summary>
        public decimal NoticeQty { get; set; }

        /// <summary>
        /// 退货子明细
        /// </summary>
        public List<MesGenerateReturnStockSubDetEntity> SubDetails { get; set; }

    }

    /// <summary>
    /// 退货子明细
    /// </summary>
    public class MesGenerateReturnStockSubDetEntity
    {
        /// <summary>
        /// 退货数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 出货仓库编码
        /// </summary>
        public string StockCode { get; set; }

        /// <summary>
        /// 出货仓库名称
        /// </summary>
        public string StockName { get; set; }
    }
}
