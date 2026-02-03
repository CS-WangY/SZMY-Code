using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
	/// <summary>
	/// 客诉
	/// </summary>
	public class CompanyComplaintRequest
	{

		/// <summary>
		/// 客诉编码
		/// </summary>
		public string ComplaintNo { get; set; }

		/// <summary>
		/// 客诉日期
		/// </summary>
		public DateTime? ComplaintDate { get; set; }

		/// <summary>
		/// 业务员
		/// </summary> 

		public string SaleMan { get; set; }

		/// <summary>
		/// 公司编码
		/// </summary>

		public string CompanyCode { get; set; }

		/// <summary>
		/// 公司名称
		/// </summary>

		public string CompanyName { get; set; }

		/// <summary>
		/// 蚂蚁型号
		/// </summary>
		public string AntComplaintModel { get; set; }

		/// <summary>
		/// 客户型号
		/// </summary>
		public string CustComplaintModel { get; set; }

		/// <summary>
		/// 投诉订单
		/// </summary>
		public string ComplaintOrder { get; set; }

		/// <summary>
		/// 订单日期
		/// </summary>
		public DateTime? OrderDate { get; set; }

		/// <summary>
		/// 投诉产品
		/// </summary>
		public string ComplaintProduct { get; set; }

		/// <summary>
		/// 投诉详情
		/// </summary>
		public string ComplaintDetailContent { get; set; }

		/// <summary>
		/// 创建日期
		/// </summary>
		public DateTime? CreateOn { get; set; }

		/// <summary>
		/// 创建人名称
		/// </summary>
		public string CreateByName { get; set; }

		/// <summary>
		/// 小类
		/// </summary>
		public long ProductSmallClassId { get; set; } = 0;

	}
}
