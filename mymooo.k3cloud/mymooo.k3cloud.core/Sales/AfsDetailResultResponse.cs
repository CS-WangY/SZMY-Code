namespace mymooo.k3cloud.core.Sales
{
    /// <summary>
    /// 售后结果查询接口响应
    /// </summary>
    public class AfsDetailResultResponse
    {
        /// <summary>
        /// 第三方接入系统的售后申请单编号
        /// </summary>
        public required string AfsApplyId { get; set; }

        /// <summary>
        /// 售后处理结果：0 = 拒绝；1 = 同意。
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 备注信息（如失败原因等）。
        /// </summary>
        public string? Remark { get; set; }
    }
}
