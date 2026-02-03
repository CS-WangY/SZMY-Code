namespace Kingdee.Mymooo.Core.Utils
{
    /// <summary>
    /// 响应code值
    /// </summary>
    public class ResponseCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        public const string Success = "success";

        /// <summary>
        /// titck无效
        /// </summary>
        public const string TitckInvalid = "titckInvalid";

        /// <summary>
        /// titck失效
        /// </summary>
        public const string TitckFailure = "titckFailure";

        /// <summary>
        /// 被禁用的app应用
        /// </summary>
        public const string ForbidApp = "ForbidApp";

        /// <summary>
        /// 未处理的异常
        /// </summary>
        public const string Exception = "Exception";

        /// <summary>
        /// Toekn无效
        /// </summary>
        public const string ToeknInvalid = "ToeknInvalid";

        /// <summary>
        /// Toekn失效
        /// </summary>
        public const string ToeknFailure = "ToeknFailure";

        /// <summary>
        /// Toekn 空
        /// </summary>
        public const string Emptytoken = "Emptytoken";

        /// <summary>
        /// 不存在第三方应用配置信息
        /// </summary>
        public const string NoThirdConfig = "NoThirdConfig";

        /// <summary>
        /// 模型验证错误
        /// </summary>
        public const string ModelError = "ModelError";

        /// <summary>
        /// 不存在企业客户信息
        /// </summary>
        public const string NoExistsData = "NoExistsData";

        /// <summary>
        /// 已存在
        /// </summary>
        public const string ExistsData = "ExistsData";

        /// <summary>
        /// 权限拒绝
        /// </summary>
        public const string PrivilegeReject = "PrivilegeReject";

        /// <summary>
        /// 数据库执行出错
        /// </summary>
        public const string DbUpdateException = "DbUpdateException";

        /// <summary>
        /// 用户状态异常
        /// </summary>
        public const string UserStatusException = "UserStatusException";

        /// <summary>
        /// 未登录
        /// </summary>
        public const string NoLogin = "NoLogin";

        /// <summary>
        /// 备案失败
        /// </summary>
        public const string CompanyKeeponFailure = "CompanyKeeponFailure";

        /// <summary>
        /// 已存在成功的备案
        /// </summary>
        public const string ExistsKeepon = "ExistsKeepon";

        /// <summary>
        /// 调用第三方错误
        /// </summary>
        public const string ThirdpartyError = "ThirdpartyError";

        /// <summary>
        /// mq服务未打开
        /// </summary>
        public const string MqNotOpen = "MqNotOpen";

        public const string Empty = "Empty";

		public const string Warning = "warning";

		/// <summary>
		/// 一次错误就停止
		/// </summary>
		public const string OneErrorStop = "oneErrorStop";

		/// <summary>
		/// 终止任务
		/// </summary>
		public const string Abort = "abort";

        /// <summary>
        /// 上限
        /// </summary>
        public const string UpperLimit = "UpperLimit";
    }
}
