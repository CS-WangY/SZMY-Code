namespace com.mymooo.workbench.core
{
    /// <summary>
    /// 金蝶配置
    /// </summary>
    public class K3CloudConfig
    {
        /// <summary>
        /// 数据中心Id
        /// </summary>
        public string DataCenterNumber { get; set; } = string.Empty;
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// 应用Id
        /// </summary>
        public string AppId { get; set; } = string.Empty;
        /// <summary>
        /// 应用密钥
        /// </summary>
        public string AppSecret { get; set; } = string.Empty;

        /// <summary>
        /// url地址
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
