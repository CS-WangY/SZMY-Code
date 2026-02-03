using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model.ExternalContact
{
    /// <summary>
    /// 获取企业已配置的「联系我」方式
    /// </summary>
    public class ExternalContactDetailResponse
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
        /// 详情
        /// </summary>
        [JsonPropertyName("contact_way")]
        public ExternalContactDetail? Detail { get; set; }

        /// <summary>
        /// 详情
        /// </summary>
        public class ExternalContactDetail
        {
            /// <summary>
            /// 新增联系方式的配置id
            /// </summary>
            [JsonPropertyName("config_id")]
            public required string ConfigId { get; set; }

            /// <summary>
            /// 联系方式类型，1-单人，2-多人
            /// </summary>
            public int Type { get; set; }

            /// <summary>
            /// 场景，1-在小程序中联系，2-通过二维码联系
            /// </summary>
            public int Scene { get; set; }

            /// <summary>
            /// 小程序中联系按钮的样式，仅在scene为1时返回，详见附录
            /// </summary>
            public int Style { get; set; }

            /// <summary>
            /// 联系方式的备注信息，用于助记
            /// </summary>
            public string? Remark { get; set; }

            /// <summary>
            /// 外部客户添加时是否无需验证
            /// </summary>
            [JsonPropertyName("skip_verify")]
            public bool SkipVerify { get; set; }

            /// <summary>
            /// 企业自定义的state参数，用于区分不同的添加渠道，在调用“获取客户详情”时会返回该参数值
            /// </summary>
            public string? State { get; set; }

            /// <summary>
            /// 联系二维码的URL，仅在scene为2时返回
            /// </summary>
            [JsonPropertyName("qr_code")]
            public string? QrCode { get; set; }

            /// <summary>
            /// 使用该联系方式的用户userID列表
            /// </summary>
            [JsonPropertyName("user")]
            public string[] Users { get; set; } = [];

            /// <summary>
            /// 使用该联系方式的部门id列表
            /// </summary>
            [JsonPropertyName("party")]
            public int[] Partys { get; set; } = [];

            /// <summary>
            ///是否临时会话模式，默认为false，true表示使用临时会话模式
            /// </summary>
            [JsonPropertyName("is_temp")]
            public bool IsTemp { get; set; }

            /// <summary>
            /// 临时会话二维码有效期，以秒为单位
            /// </summary>
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            /// <summary>
            /// 临时会话有效期，以秒为单位
            /// </summary>
            [JsonPropertyName("chat_expires_in")]
            public int ChatExpiresIn { get; set; }

            /// <summary>
            /// 可进行临时会话的客户unionid
            /// </summary>
            public string? Unionid { get; set; }

            /// <summary>
            /// 结束语，可参考“结束语定义”
            /// </summary>
            public dynamic? Conclusions { get; set; }
        }
    }
}
