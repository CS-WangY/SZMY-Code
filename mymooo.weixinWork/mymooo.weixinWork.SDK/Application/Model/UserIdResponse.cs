namespace mymooo.weixinWork.SDK.Application.Model
{
    /// <summary>
    /// 获取userid响应
    /// </summary>
    public class UserIdResponse
    {
        /// <summary>
        /// 出错返回码，为0表示成功，非0表示调用失败
        /// </summary>
        public int Errcode { get; set; }

        /// <summary>
        /// 返回码提示语
        /// </summary>
        public string? Errmsg { get; set; }

        /// <summary>
        /// 成员UserID。若需要获得用户详情信息，可调用通讯录接口：读取成员
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 非企业成员的标识，对当前企业唯一
        /// </summary>
        public string OpenId { get; set; } = string.Empty;
    }
}
