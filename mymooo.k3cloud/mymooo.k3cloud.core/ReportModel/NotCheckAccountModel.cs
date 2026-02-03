using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.ReportModel
{
    /// <summary>
    /// 未对账合计
    /// </summary>
    public class NotCheckAccountTotals
    {
        /// <summary>
        /// 当月合计
        /// </summary>
        public decimal AmountA { get; set; }
        /// <summary>
        /// 前2月
        /// </summary>
        public decimal AmountB { get; set; }
        /// <summary>
        /// 前5月 - 前2月
        /// </summary>
        public decimal AmountC { get; set; }
        /// <summary>
        /// 前2年-前5月
        /// </summary>
        public decimal AmountD { get; set; }
        /// <summary>
        /// 本月
        /// </summary>
        public int CusCountA { get; set; }
        /// <summary>
        /// 前2月
        /// </summary>
        public int CusCountB { get; set; }
        /// <summary>
        /// 前5月 - 前2月
        /// </summary>
        public int CusCountC { get; set; }
        /// <summary>
        /// 前2年-前5月
        /// </summary>
        public int CusCountD { get; set; }
    }
    /// <summary>
    /// 未对账明细
    /// </summary>
    public class NotCheckAccountList
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustNumber { get; set; } = string.Empty;
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustName { get; set; } = string.Empty;
        /// <summary>
        /// 销售员
        /// </summary>
        public string SalerName { get; set; } = string.Empty;
        /// <summary>
        /// 项数
        /// </summary>
        public int ItemCount { get; set; }
        /// <summary>
        /// 未对账总数
        /// </summary>
        public decimal NotArJoinQty { get; set; }
        /// <summary>
        /// 未对账金额
        /// </summary>
        public decimal NotArJoinAmount { get; set; }
    }
    /// <summary>
    /// 未对账客户明细
    /// </summary>

    public class CustNotCheckAccountList
    {
        /// <summary>
        /// 发货单号
        /// </summary>
        public string SrcBillNo { get; set; } = string.Empty;
        /// <summary>
        /// 销售明细ID
        /// </summary>
        public long SOEntryid { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public DateTime? Approvedate { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SOOrderNo { get; set; } = string.Empty;
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemNumber { get; set; } = string.Empty;
        /// <summary>
        /// 未对账总额
        /// </summary>
        public decimal NotArJoinAmount { get; set; }
        /// <summary>
        /// 未对账总数
        /// </summary>
        public decimal NotArJoinQty { get; set; }
    }
}
