using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    public class PlanEntry
    {
        /// <summary>
        /// 匹配核销ID
        /// </summary>
        public long FMMEID { get; set; }
        /// <summary>
        /// 源单编码
        /// </summary>
        public string AdvanceNo { get; set; }
        /// <summary>
        /// 源单ID
        /// </summary>
        public long ADVANCEID { get; set; }
        /// <summary>
        /// 源单序号
        /// </summary>
        public int AdvanceSeq { get; set; }
        /// <summary>
        /// 分录ID
        /// </summary>
        public long ADVANCEENTRYID { get; set; }
        /// <summary>
        /// 关联金额
        /// </summary>
        public decimal ActRecAmount { get; set; }
        /// <summary>
        /// 应收金额
        /// </summary>
        public decimal RemainAmount { get; set; }
        /// <summary>
        /// 未知
        /// </summary>
        public decimal PREMATCHAMOUNTFOR { get; set; }
        /// <summary>
        /// 结算组织ID
        /// </summary>
        public long SettleOrgId_Id { get; set; }
        /// <summary>
        /// 结算组织
        /// </summary>
        public string SettleOrgId { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int Seq { get; set; }
    }

    public class SalesOrderDetailListItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string OrderNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PaymentMethodCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PaymentMethodName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SalesMan { get; set; }
        public string salesOrgCode { get; set; }
        /// <summary>
        /// 收款计划
        /// </summary>
        public List<PlanEntry> planentry { get; set; }
    }

    public class MatchMoney
    {
        /// <summary>
        /// 
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<SalesOrderDetailListItem> OrderNews { get; set; }
    }

    public class SalesOrderK3CloudRequest
    {
        public List<PlanEntry> Planentries { get; set; }
        public string OrderCode { get; set; }
        public decimal OrderAmount { get; set; }
    }

    public class ApiRequest
    {
        public string code { get; set; }
        public bool isSuccess { get; set; }
        public object data { get; set; }
        public string errorMessage { get; set; }
        public string[] errorMessages { get; set; }
    }
}
