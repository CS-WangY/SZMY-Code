using Kingdee.BOS.Core.Metadata.PreInsertData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class SupplierRequest
    {
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 供应商中文名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 统一信用代码
        /// </summary>
        public string BusinessLicenceCode { get; set; }

        /// <summary>
        /// 法人
        /// </summary>
        public string Corporation { get; set; }

        /// <summary>
        /// 注册资金
        /// </summary>
        public string RegisteredCapital { get; set; }

        /// <summary>
        /// 工商注册号
        /// </summary>
        public string BusinessRegistrationNo { get; set; }


        /// <summary>
        /// 网址
        /// </summary>
        public string InternetAddress { get; set; }

        /// <summary>
        /// 供应商类型
        /// </summary>
        public string SupplierType { get; set; }

        /// <summary>
        /// 注册地址
        /// </summary>
        public string RegisterAddress { get; set; }


        /// <summary>
        /// 结算方式
        /// </summary>
        public string PayMethod { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        public string InvoiceType { get; set; }

        /// <summary>
        /// 增值税编号
        /// </summary>
        public string VatCode { get; set; }

        public SupplierContact SupplierContact { get; set; }
        public SupplierBank SupplierBank { get; set; }

        /// <summary>
        /// 是否临时供应商
        /// </summary>
        public bool IsTemporary { get; set; }
    }
    public class SupplierContact
    {
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 联系人Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        public string Post { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 传真
        /// </summary>
        public string Fax { get; set; }
    }

    public class SupplierBankRequest
    { 
     public string Code { get; set; }
     public List<SupplierBank> SupplierBanks { get; set; }
    }
    public class SupplierBank
    {
        /// <summary>
        /// 银行账户名称
        /// </summary>
        public string BankAccountName { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }
        /// <summary>
        /// 开户银行
        /// </summary>
        public string OpenBankName { get; set; }
        /// <summary>
        /// 银行账号
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        /// 联行号
        /// </summary>
        public string BankLineNumber { get; set; }

        /// <summary>
        /// 银行网点名称
        /// </summary>
        public string BankBranchName { get; set; }

        /// <summary>
        /// 开户行地
        /// </summary>
        public string OpenBankAddress { get; set; }
    }

    /// <summary>
    /// 供应商分配组织
    /// </summary>
    public class SupplierAllotOrgRequest
    {
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 公司编码
        /// </summary>
        public string CompanyCode { get; set; }
    }

    /// <summary>
    /// 会计核算体系
    /// </summary>
    public class OrgAccountSystemEntity
    {
        /// <summary>
        /// 组织ID
        /// </summary>
        public long OrgId { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrgName { get; set; }
    }
}
