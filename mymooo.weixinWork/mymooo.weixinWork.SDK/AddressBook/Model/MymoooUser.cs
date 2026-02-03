using mymooo.core.Attributes.Redis;

namespace mymooo.weixinWork.SDK.AddressBook.Model
{
    /// <summary>
    /// 蚂蚁用户表
    /// </summary>
    [RedisKey("mymooo-weixin-AddressBook", 14)]
    public partial class MymoooUser
    {
        /// <summary>
        /// 主键
        /// </summary>
        [RedisIndex]
        public long Id { get; set; }

        /// <summary>
        /// appid
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 企业微信用户code
        /// </summary>
        [RedisPrimaryKey]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 企业微信名称
        /// </summary>
        [RedisValue]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 职务信息；第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱，第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        [RedisValue]
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// 性别。0表示未定义，1表示男性，2表示女性
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱，第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        [RedisValue]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// 企业邮箱
        /// </summary>
        [RedisValue]
        public string Biz_Email { get; set; } = string.Empty;

        /// <summary>
        /// 座机。第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Telephone { get; set; } = string.Empty;

        /// <summary>
        /// 别名；第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// 地址。第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 全局唯一。对于同一个服务商，不同应用获取到企业内同一个成员的open_userid是相同的，最多64个字节。仅第三方应用可获取
        /// </summary>
        public string OpenUserid { get; set; } = string.Empty;

        /// <summary>
        /// 主部门
        /// </summary>
        [RedisValue]
        public long MainDepartmentId { get; set; }

        /// <summary>
        /// 激活状态: 1=已激活，2=已禁用，4=未激活，5=退出企业。
        /// 已激活代表已激活企业微信或已关注微工作台（原企业号）。未激活代表既未激活企业微信又未关注微工作台（原企业号）。
        /// </summary>
        [RedisValue]
        public int Status { get; set; }

        /// <summary>
        /// 员工个人二维码，扫描可添加为外部联系人(注意返回的是一个url，可在浏览器上打开该url以展示二维码)；第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        [RedisValue]
        public string QrCode { get; set; } = string.Empty;

        /// <summary>
        /// 对外职务，如果设置了该值，则以此作为对外展示的职务，否则以position来展示。第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string ExternalPosition { get; set; } = string.Empty;

        /// <summary>
        /// 入职时间
        /// </summary>
        public DateTime? EntryDate { get; set; }

        /// <summary>
        /// 直属上级
        /// </summary>
        public string DirectSupervisor { get; set; } = string.Empty;

        /// <summary>
        /// 职级 
        /// </summary>
        public string Grade { get; set; } = string.Empty;

        /// <summary>
        /// 学历
        /// </summary>
        public string Education { get; set; } = string.Empty;

        /// <summary>
        /// 版本
        /// </summary>
        public long Versions { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [RedisValue]
        public bool IsDelete { get; set; }
    }
}
