using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    /// <summary>
    /// MES的供应商信息
    /// </summary>
    public class MesSupplyInfoEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public string FormId { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        public long SupplierId { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 简称
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// 数据状态
        /// </summary>
        public string DocumentStatus { get; set; }
        /// <summary>
        /// 禁用状态
        /// </summary>
        public string ForbidStatus { get; set; }
        /// <summary>
        /// 临时供应商
        /// </summary>
        public string IsTemporary { get; set; }
        /// <summary>
        /// 通讯地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 公司网址
        /// </summary>
        public string WebSite { get; set; }
        /// <summary>
        /// 注册地址
        /// </summary>
        public string RegisterAddress { get; set; }
        /// <summary>
        /// 法人代表
        /// </summary>
        public string LegalPerson { get; set; }
        /// <summary>
        /// 注册资金(万元)
        /// </summary>
        public decimal RegisterFund { get; set; }
        /// <summary>
        /// 工商登记号
        /// </summary>
        public string RegisterCode { get; set; }
        /// <summary>
        /// 生产经营许可证
        /// </summary>
        public string TendPermit { get; set; }
        /// <summary>
        /// 供应类别
        /// </summary>
        public string SupplyClassify { get; set; }
        /// <summary>
        /// 统一社会信用代码
        /// </summary>
        public string SocialCreditCode { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// 联系人明细
        /// </summary>
        public List<MesSupplyContactDetEntity> ContactDetails { get; set; }

		/// <summary>
		/// 银行明细
		/// </summary>
		public List<MesSupplyBankEntity> BankDetails { get; set; }

	}
    /// <summary>
    /// MES的供应商联系人信息
    /// </summary>
    public class MesSupplyContactDetEntity
    {
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 职务
        /// </summary>
        public string Post { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 传真
        /// </summary>
        public string Fax { get; set; }
        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string EMail { get; set; }
        /// <summary>
        /// 默认联系人
        /// </summary>
        public string IsDefault { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 禁用状态
        /// </summary>
        public string ConForbidStatus { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string ContactNumber { get; set; }
    }

	/// <summary>
	/// MES的供应商银行信息
	/// </summary>
	public class MesSupplyBankEntity
	{
		/// <summary>
		/// 开户银行名称
		/// </summary>
		public string OpenBankName { get; set; }

		/// <summary>
		/// 账户名称
		/// </summary>
		public string BankAccountName { get; set; }

		/// <summary>
		/// 银行账号
		/// </summary>
		public string BankCode { get; set; }

		/// <summary>
		/// 开户行地
		/// </summary>
		public string OpenBankAddress { get; set; }

		/// <summary>
		/// 是否默认
		/// </summary>
		public string IsDefault { get; set; }
	}

}
