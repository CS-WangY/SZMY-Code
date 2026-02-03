using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
	public class CustomerRequest
	{

		/// <summary>
		/// 企业编号
		/// </summary>
		public string Code { get; set; }
		/// <summary>
		/// 企业名称
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 企业营业执照代码
		/// </summary>
		public string BusinessLicenceCode { get; set; }

		/// <summary>
		/// 是否有效
		/// </summary>
		public bool IsValid { get; set; } = true;

		/// <summary>
		/// 法人
		/// </summary>
		public string Corporation { get; set; }

		/// <summary>
		/// 企业性质
		/// </summary>
		public string NatureText { get; set; }
		/// <summary>
		/// 联系人
		/// </summary>
		public string Linkman { get; set; }

		/// <summary>
		/// 联系人编码
		/// </summary>
		public string LinkCode { get; set; }

		/// <summary>
		/// 手机号
		/// </summary>
		public string Mobile { get; set; }

		/// <summary>
		/// 邮箱地址
		/// </summary>
		public string EMail { get; set; }

		/// <summary>
		/// 地址
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// 性别
		/// </summary>
		public string Sex { get; set; }

		/// <summary>
		/// 邮政编码
		/// </summary>
		public string ZipCode { get; set; }

		/// <summary>
		/// 注册资金
		/// </summary>
		public string RegisteredCapital { get; set; }

		public int DecimalPlacesOfUnitPrice { get; set; }

		/// <summary>
		/// 是否添加联系人
		/// </summary>
		public bool IsAddLink { get; set; } = true;
		/// <summary>
		/// 客户等级
		/// </summary>
		public string CustomerLevel { get; set; }
		/// <summary>
		/// 特殊发货
		/// </summary>
		public bool SpecialDelivery { get; set; } = false;
		/// <summary>
		/// 包装要求
		/// </summary>
		public string PackagingReq { get; set; }
	}
}
