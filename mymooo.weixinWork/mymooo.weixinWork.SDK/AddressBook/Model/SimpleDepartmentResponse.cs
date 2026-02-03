using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.AddressBook.Model
{
    /// <summary>
    /// 获取部门成员相应
    /// </summary>
    public class SimpleDepartmentResponse
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public int Errcode { get; set; }

        /// <summary>
        /// 对返回码的文本描述内容
        /// </summary>
        [JsonPropertyName("errmsg")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        ///  	成员列表
        /// </summary>
        public UserListInfo[] UserList { get; set; } = [];

        /// <summary>
        ///  	成员列表
        /// </summary>
        public class UserListInfo
        {
            /// <summary>
            /// 成员UserID。对应管理端的帐号
            /// </summary>
            public string UserId { get; set; } = string.Empty;

            /// <summary>
            /// 成员名称，此字段从2019年12月30日起，对新创建第三方应用不再返回，2020年6月30日起，对所有历史第三方应用不再返回，后续第三方仅通讯录应用可获取，第三方页面需要通过通讯录展示组件来展示名字
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// 成员所属部门列表。列表项为部门ID，32位整型
            /// </summary>
            public long[] Department { get; set; } = [];

            /// <summary>
            /// 全局唯一。对于同一个服务商，不同应用获取到企业内同一个成员的open_userid是相同的，最多64个字节。仅第三方应用可获取
            /// </summary>
            [JsonPropertyName("open_userid")]
            public string OpenUserId { get; set; } = string.Empty;
        }
    }
}
