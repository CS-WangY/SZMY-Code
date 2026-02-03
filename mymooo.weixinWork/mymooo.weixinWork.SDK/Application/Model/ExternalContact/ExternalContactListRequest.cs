using mymooo.core.Utils;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model.ExternalContact
{
    /// <summary>
    /// 获取企业已配置的「联系我」列表
    /// </summary>
    public class ExternalContactListRequest
    {
        private DateTime _startDate;
        private DateTime _endDate;

        /// <summary>
        /// 「联系我」创建起始时间戳, 默认为90天前
        /// </summary>
        [JsonPropertyName("start_time")]
        public long StartTime { get; private set; }

        /// <summary>
        /// 「联系我」创建起始时间戳, 默认为90天前
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
                StartTime = _startDate.GetTimestampSecond();
            }
        }

        /// <summary>
        /// 「联系我」创建结束时间戳, 默认为当前时间
        /// </summary>
        [JsonPropertyName("end_time")]
        public long EndTime { get; set; }

        /// <summary>
        /// 「联系我」创建结束时间戳, 默认为当前时间
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
                EndTime = _endDate.GetTimestampSecond();
            }
        }
        /// <summary>
        /// 分页查询使用的游标，为上次请求返回的 next_cursor
        /// </summary>
        public string? Cursor { get; set; }

        /// <summary>
        /// 每次查询的分页大小，默认为100条，最多支持1000条
        /// </summary>
        public int Limit { get; set; } = 1000;
    }
}
