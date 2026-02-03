using mymooo.k3cloud.core.SqlSugarCore;
using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;

namespace mymooo.k3cloud.core.Approver
{
    /// <summary>
    /// 销售订单关闭
    /// </summary>
    [ApprovalTemplate("1970324973036836_1688852077365749_754693934_1533121342")]
    public class K3CloudClosedSalOrderRequest : ApprovalRequest
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public SalOrderClosedOperationType OperationType { get; set; }

        /// <summary>
        /// 销售订单ID
        /// </summary>
        public long SaleOrderID { get; set; }

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
        /// 销售订单号
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
        /// 客户类型
        /// </summary>
        public string CustType { get; set; } = string.Empty;
        /// <summary>
        /// 是否退款客户
        /// </summary>
        public string IsRefund { get; set; } = string.Empty;

        /// <summary>
        /// 产品所属事业部
        /// </summary>
        public string OrgID { get; set; } = string.Empty;
        /// <summary>
        /// 供货组织名称
        /// </summary>
        public string OrgName { get; set; } = string.Empty;
        /// <summary>
        /// 取消订单原因
        /// </summary>
        public string Remarks { get; set; } = string.Empty;
        /// <summary>
        /// 明细ID
        /// </summary>
        public List<long> SaleOrderEntrys { get; set; } = [];
        /// <summary>
        /// 附件材料
        /// </summary>
        public List<MediaInfo> AttachmentMaterials { get; set; } = [];
    }
    /// <summary>
    /// 预测订单关闭
    /// </summary>
    [ApprovalTemplate("C4WtbQDq6QvQYsUCBaJYhj4Lz4FBEkTVRoJ4E5H7A")]
    public class K3CloudClosedForecastOrderRequest : ApprovalRequest
    {

        /// <summary>
        /// 预测订单ID
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
        public List<long> ForecastOrderEntrys { get; set; } = [];
        /// <summary>
        /// 附件材料
        /// </summary>
        public List<MediaInfo> AttachmentMaterials { get; set; } = [];
    }
}
