using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.core.SystemManage
{
    /// <summary>
    /// 获取部门成员详情
    /// </summary>
    public class DepartmentListResponse
    {
        /// <summary>
        ///  	返回码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 对返回码的文本描述内容
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 成员列表
        /// </summary>
        public Userlist[] userlist { get; set; }

        /// <summary>
        /// 成员列表
        /// </summary>
        public class Userlist
        {
            /// <summary>
            /// 用户id唯一标识
            /// </summary>
            public long id { get; set; }

            /// <summary>
            /// 成员UserID。对应管理端的帐号
            /// </summary>
            public string userid { get; set; }

            /// <summary>
            /// 成员名称，此字段从2019年12月30日起，对新创建第三方应用不再返回，2020年6月30日起，对所有历史第三方应用不再返回，后续第三方仅通讯录应用可获取，第三方页面需要通过通讯录展示组件来展示名字
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// 成员所属部门id列表，仅返回该应用有查看权限的部门id
            /// </summary>
            public long[] department { get; set; }

            /// <summary>
            /// 部门内的排序值，32位整数，默认为0。数量必须和department一致，数值越大排序越前面。
            /// </summary>
            public long[] order { get; set; }
            public string position { get; set; }

            /// <summary>
            /// 是否是助理
            /// </summary>
            public bool? isAssistant { get; set; }

            /// <summary>
            /// 手机号码，第三方仅通讯录应用可获取
            /// </summary>
            public string mobile { get; set; }

            /// <summary>
            /// 性别。0表示未定义，1表示男性，2表示女性
            /// </summary>
            public string gender { get; set; }

            /// <summary>
            /// 邮箱，第三方仅通讯录应用可获取
            /// </summary>
            public string email { get; set; }

            /// <summary>
            /// 表示在所在的部门内是否为上级；第三方仅通讯录应用可获取
            /// </summary>
            public bool IsLeaderInDept { get; set; }

            /// <summary>
            /// 头像url。第三方仅通讯录应用可获取
            /// </summary>
            public string avatar { get; set; }

            /// <summary>
            /// 头像缩略图url。第三方仅通讯录应用可获取
            /// </summary>
            public string thumb_avatar { get; set; }

            /// <summary>
            /// 座机。第三方仅通讯录应用可获取
            /// </summary>
            public string telephone { get; set; }

            /// <summary>
            /// 别名；第三方仅通讯录应用可获取
            /// </summary>
            public string alias { get; set; }

            /// <summary>
            /// 激活状态: 1=已激活，2=已禁用，4=未激活，5=退出企业。
            /// 已激活代表已激活企业微信或已关注微工作台（原企业号）。未激活代表既未激活企业微信又未关注微工作台（原企业号）。
            /// </summary>
            public int status { get; set; }

            /// <summary>
            /// 地址
            /// </summary>
            public string address { get; set; }

            /// <summary>
            /// 是否隐藏手机号
            /// </summary>
            public int hide_mobile { get; set; }

            /// <summary>
            /// 英文名
            /// </summary>
            public string english_name { get; set; }

            /// <summary>
            /// 全局唯一。对于同一个服务商，不同应用获取到企业内同一个成员的open_userid是相同的，最多64个字节。仅第三方应用可获取
            /// </summary>
            public string open_userid { get; set; }

            /// <summary>
            /// 主部门
            /// </summary>
            public int main_department { get; set; }

            /// <summary>
            /// 扩展属性，第三方仅通讯录应用可获取
            /// </summary>
            public Extattr extattr { get; set; }

            /// <summary>
            ///  	员工个人二维码，扫描可添加为外部联系人；第三方仅通讯录应用可获取
            /// </summary>
            public string qr_code { get; set; }

            /// <summary>
            /// 成员对外属性，字段详情见对外属性；第三方仅通讯录应用可获取
            /// </summary>
            public string external_position { get; set; }

            /// <summary>
            /// 对外职务。 第三方仅通讯录应用可获取
            /// </summary>
            public External_Profile external_profile { get; set; }

            /// <summary>
            /// 是否是产品经理
            /// </summary>
            public bool IsManager { get; set; }

            /// <summary>
            /// 是否是采购员
            /// </summary>
            public bool IsPurchaser { get; set; }

            /// <summary>
            /// 岗位
            /// </summary>
            public long Post { get; set; }

            public string DepartmentName { get; set; }
        }

        /// <summary>
        /// 扩展属性，第三方仅通讯录应用可获取
        /// </summary>
        public class Extattr
        {
            /// <summary>
            /// 扩展属性，第三方仅通讯录应用可获取
            /// </summary>
            public Attr[] attrs { get; set; }
        }

        public class Attr
        {
            public int type { get; set; }
            public string name { get; set; }
            public Text text { get; set; }
            public Web web { get; set; }
        }

        public class Text
        {
            public string value { get; set; }
        }

        public class Web
        {
            public string url { get; set; }
            public string title { get; set; }
        }

        public class External_Profile
        {
            public string external_corp_name { get; set; }
            public External_Attr[] external_attr { get; set; }
        }

        public class External_Attr
        {
            public int type { get; set; }
            public string name { get; set; }
            public Text text { get; set; }
            public Web web { get; set; }
            public Miniprogram miniprogram { get; set; }
        }

        public class Miniprogram
        {
            public string appid { get; set; }
            public string pagepath { get; set; }
            public string title { get; set; }
        }
    }
}
