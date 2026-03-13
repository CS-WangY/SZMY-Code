namespace com.mymooo.mall.core.Model.Cache
{
	/// <summary>
	/// 
	/// </summary>
	public class DivsionSupplyResponse
    {

        /// <summary>
        /// 事业部Id
        /// </summary>
        public string BusinessDivisionId { get; set; } = string.Empty;

        /// <summary>
        /// 事业部编码
        /// </summary>
        public string BusinessDivisionNumber { get; set; } = string.Empty;

        /// <summary>
        /// 事业部名称
        /// </summary>
        public string BusinessDivisionName { get; set; } = string.Empty;

        /// <summary>
        /// 供应组织Id
        /// </summary>
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// 供应组织编码
        /// </summary>
        public string SupplyOrgNumber { get; set; } = string.Empty;

        /// <summary>
        /// 供应组织名称
        /// </summary>
        public string SupplyOrgName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public long SmallClassId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SmallClassName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public bool IsDefault { get; set; }

    }
}
