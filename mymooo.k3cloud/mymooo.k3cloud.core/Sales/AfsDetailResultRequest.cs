namespace mymooo.k3cloud.core.Sales
{
    /// <summary>
    /// 售后结果查询接口请求参数
    /// </summary>
    public class AfsDetailResultRequest
    {
        /// <summary>
        /// 第三方接入系统的售后申请单编号
        /// </summary>
        public required string AfsApplyId { get; set; }
    }
}
