using Newtonsoft.Json;

namespace com.mymooo.mall.core.Model.Address
{
	/// <summary>
	/// 用户地址。
	/// </summary>
	[Serializable]
    public class CustomerAddress
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 客户ID
        /// </summary>
        [JsonIgnore]
        public long CustomerId { get; set; }

        /// <summary>
        /// 公司Id
        /// </summary>
        [JsonIgnore]
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 接收人
        /// </summary>
        public string Receiver { get; set; } = string.Empty;

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// 公司名
        /// </summary>
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 别名
        /// </summary>
        [JsonProperty(PropertyName = "AddressAlias")]
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// 省
        /// </summary>
        public int ProvinceId { get; set; }

		/// <summary>
		/// 省
		/// </summary>
		public string Province { get; set; } = string.Empty;

		/// <summary>
		/// 市
		/// </summary>
		public int CityId { get; set; }

		/// <summary>
		/// 市
		/// </summary>
		public string City { get; set; } = string.Empty;

		/// <summary>
		/// 区
		/// </summary>
		public int DistrictId { get; set; }

		/// <summary>
		/// 区
		/// </summary>
		public string District { get; set; } = string.Empty;

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
