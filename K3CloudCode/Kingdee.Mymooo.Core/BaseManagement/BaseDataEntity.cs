using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    /// <summary>
    /// 事业部
    /// </summary>
    public class BaseDataEntity
    {
        /// <summary>
        /// 事业部ID
        /// </summary>
        public string BusinessDivisionId { get; set; }
        /// <summary>
        /// 事业部名称
        /// </summary>
        public string BusinessDivisionName { get; set; }
    }

    /// <summary>
    /// 组织
    /// </summary>
    public class OrgEntity
    {
        /// <summary>
        /// 组织ID
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 组织编码
        /// </summary>
        public string OrgCode { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrgName { get; set; }
    }

    /// <summary>
    /// 收款条件
    /// </summary>
    public class PaymentEntity
    {
        /// <summary>
        /// 付款编号
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// 付款描述
        /// </summary>
        public string TermDesc { get; set; }

    }

    /// <summary>
    /// 货币
    /// </summary>
    public class CurrencyEntity
    {
        /// <summary>
        /// 货币编码
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>
        /// 货币中文名称
        /// </summary>
        public string Name { get; set; } = "";

    }
    /// <summary>
    /// 增值税
    /// </summary>
    public class VatEntity
    {
        /// <summary>
        /// 增值税编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 增值税中文描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public decimal VatRate { get; set; } = 0;

    }
    /// <summary>
    /// 查询是否同源供应商
    /// </summary>
    public class CheckSameSupplierFilter
    {
        /// <summary>
        /// 客户编号
        /// </summary>
        public string CustomerCode { get; set; }
        /// <summary>
        /// 供应商编号
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 物料号
        /// </summary>
        public string ItemNumber { get; set; }
        /// <summary>
        /// 采购日期
        /// </summary>
        public DateTime PurchaseDate { get; set; }
        /// <summary>
        /// 组织编码
        /// </summary>
        public string CompanyId { get; set; }
    }
    /// <summary>
    /// 同源供应商
    /// </summary>
    public class SameSupplierDto
    {
        /// <summary>
        /// 是否同源
        /// </summary>
        public bool IsSameSupplier { get; set; }
        /// <summary>
        /// 送货天数
        /// </summary>
        public int DeliveryDay { get; set; }
        /// <summary>
        /// 采购价格
        /// </summary>
        public decimal PurchasePrice { get; set; }
    }
    /// <summary>
    /// 物料体积
    /// </summary>
    public class ItemVolumeEntity
    {
        /// <summary>
        /// 物料编号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 体积
        /// </summary>
        public decimal Volume { get; set; }

    }
}
