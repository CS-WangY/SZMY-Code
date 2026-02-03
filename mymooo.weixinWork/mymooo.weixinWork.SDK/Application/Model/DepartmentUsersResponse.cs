using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model
{
    /// <summary>
    /// 获取部门下用户 详细信息
    /// </summary>
    public class DepartmentUsersResponse
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
        public UserInfoResponse[] UserList { get; set; } = [];
    }
}
