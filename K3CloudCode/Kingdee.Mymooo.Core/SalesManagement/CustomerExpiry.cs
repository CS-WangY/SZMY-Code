using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
	public class CustomerExpiry
	{
		/// <summary>
		/// 
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// 日期
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// 金额
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// 单据Id
		/// </summary>
		public string FormId { get; set; }
	}
	public class CustomerExpiryBill : CustomerExpiry
	{
		public string BillNo { get; set; }
	}
}
