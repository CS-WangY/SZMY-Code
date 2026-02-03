using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model.KfAccount
{
    /// <summary>
    /// 获取客服账号列表 响应消息
    /// </summary>
    public class KfAccountListResponse
    {
        /// <summary>
        /// 出错返回码，为0表示成功，非0表示调用失败
        /// </summary>
        public int Errcode { get; set; }

        /// <summary>
        /// 返回码提示语
        /// </summary>
        [JsonPropertyName("errmsg")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 客服账号明细
        /// </summary>
        [JsonPropertyName("account_list")]
        public KfAccountListDetail[] Details { get; set; } = [];

        /// <summary>
        /// 客服账号明细
        /// </summary>
        public class KfAccountListDetail
        {
            /// <summary>
            /// 客服账号ID
            /// </summary>
            [JsonPropertyName("open_kfid")]
            public required string KfId { get; set; }

            /// <summary>
            /// 客服名称
            /// </summary>
            public required string Name { get; set; }

            /// <summary>
            /// 客服头像URL
            /// </summary>
            public string? Avatar { get; set; }

            /// <summary>
            /// 当前调用接口的应用身份，是否有该客服账号的管理权限（编辑客服账号信息、分配会话和收发消息）。组件应用不返回此字段
            /// </summary>
            [JsonPropertyName("manage_privilege")]
            public bool ManagePrivilege { get; set; }
        }
    }

}
