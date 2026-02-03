using mymooo.core.Attributes.Redis;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.AddressBook.Model
{
    /// <summary>
    /// 部门详情
    /// </summary>
	[RedisKey("mymooo-weixinwork-sync", 14)]
    public class DepartmentListResponse
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public long Errcode { get; set; }

        /// <summary>
        /// 对返回码的文本描述内容
        /// </summary>
        [JsonPropertyName("errmsg")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 部门列表数据。
        /// </summary>
        public DepartmentInfo[] Department { get; set; } = [];

        /// <summary>
        /// 部门列表数据。
        /// </summary>
        public class DepartmentInfo
        {
            /// <summary>
            /// 创建的部门id
            /// </summary>
            public long Id { get; set; }

            /// <summary>
            /// 部门名称，此字段从2019年12月30日起，对新创建第三方应用不再返回，2020年6月30日起，对所有历史第三方应用不再返回，后续第三方仅通讯录应用可获取，第三方页面需要通过通讯录展示组件来展示部门名称
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            ///  	英文名称
            /// </summary>
            [JsonPropertyName("name_en")]
            public string NameEn { get; set; } = string.Empty;

            /// <summary>
            /// 父部门id。根部门为1
            /// </summary>
            [JsonPropertyName("parentid")]
            public int ParentId { get; set; }

            /// <summary>
            /// 在父部门中的次序值。order值大的排序靠前。值范围是[0, 2^32)
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// 系统号
            /// </summary>
            public string AppId { get; set; } = string.Empty;
        }

    }
}
