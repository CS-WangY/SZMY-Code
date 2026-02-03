using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.SalesManagement;

namespace Kingdee.Mymooo.Core.PlanOrderManagement
{
    public class K3CloudClosedForecastOrderRequest
    {
        public string SpStatus { get; set; }
        public string ApplyeventNo { get; set; }
        /// <summary>
        /// 预测单ID
        /// </summary>
        public long ForecastOrderID { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustName { get; set; } = string.Empty;
        /// <summary>
        /// 取消金额
        /// </summary>
        public decimal ClosedAmount { get; set; }
        /// <summary>
        /// 下单日期
        /// </summary>
        public DateTime PlaceDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 预测订单号
        /// </summary>
        public string BillNo { get; set; } = string.Empty;
        /// <summary>
        /// 产品型号/名称
        /// </summary>
        public string Material { get; set; } = string.Empty;

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal OrderQty { get; set; }
        /// <summary>
        /// 产品所属事业部
        /// </summary>
        public string OrgID { get; set; } = string.Empty;
        /// <summary>
        /// 取消订单原因
        /// </summary>
        public string Remarks { get; set; } = string.Empty;
        /// <summary>
        /// 明细ID
        /// </summary>
        public List<long> ForecastOrderEntrys { get; set; }
        /// <summary>
        /// 发起人
        /// </summary>
        public string Creator_userid { get; set; }

        /// <summary>
        /// 备注信息，请提供3行。
        /// </summary>
        public string[] SummaryInfo { get; set; }
        public string SendRabbitCode { get; set; }
        public string EnvCode { get; set; }

        /// <summary>
        /// 抄送人
        /// </summary>
        public List<string> Notifyer { get; set; }
    }
}
