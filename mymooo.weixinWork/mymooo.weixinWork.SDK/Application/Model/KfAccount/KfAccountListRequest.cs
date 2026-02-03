namespace mymooo.weixinWork.SDK.Application.Model.KfAccount
{
    /// <summary>
    /// 获取客服账号列表，包括客服账号的客服ID、名称和头像。
    /// </summary>
    public class KfAccountListRequest
    {
        /// <summary>
        /// 分页，偏移量, 默认为0
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// 分页，预期请求的数据量，默认为100，取值范围 1 ~ 100
        /// </summary>
        public int Limit { get; set; } = 100;
    }
}
