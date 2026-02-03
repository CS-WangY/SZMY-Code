using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model.ExternalContact
{
    /// <summary>
    /// 获取企业已配置的「联系我」列表
    /// </summary>
    public class ExternalContactListResponse
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
        /// 联系方式的配置id
        /// </summary>
        [JsonPropertyName("contact_way")]
        public ExternalContactId[] ContactWay { get; set; } = [];

        /// <summary>
        /// 分页参数，用于查询下一个分页的数据，为空时表示没有更多的分页
        /// </summary>
        [JsonPropertyName("next_cursor")]
        public string? NextCursor { get; set; }

        /// <summary>
        /// 明细联系我ID
        /// </summary>
        public class ExternalContactId
        {
            /// <summary>
            /// 联系方式的配置id
            /// </summary>
            [JsonPropertyName("config_id")]
            public required string ConfigId { get; set; }
        }
    }
}
