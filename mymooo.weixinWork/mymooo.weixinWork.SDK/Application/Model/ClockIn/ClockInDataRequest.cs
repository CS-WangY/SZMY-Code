using mymooo.core.Utils;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model.ClockIn
{
    /// <summary> 
    /// 获取可见范围内员工指定时间段内的打卡记录数据  请求
    /// </summary>
    public class ClockInDataRequest
    {
        private DateTime _startDate;
        private DateTime _endDate;

        /// <summary>
        /// 打卡类型。1：上下班打卡；2：外出打卡；3：全部打卡
        /// </summary>
        [JsonPropertyName("opencheckindatatype")]
        public int CheckinType { get; set; }

        /// <summary>
        /// 开始时间
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
                StartTimestamp = _startDate.GetTimestampSecond();
            }
        }

        /// <summary>
        /// 获取打卡记录的开始时间。Unix时间戳
        /// </summary>
        [JsonPropertyName("starttime")]
        public long StartTimestamp { get; private set; }

        /// <summary>
        /// 开始时间
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
                EndTimestamp = _endDate.GetTimestampSecond(); 
            }
        }

        /// <summary>
        /// 获取打卡记录的结束时间。Unix时间戳
        /// </summary>
        [JsonPropertyName("endtime")]
        public long EndTimestamp { get; set; }

        /// <summary>
        /// 需要获取打卡记录的用户列表
        /// </summary>
        [JsonPropertyName("useridlist")]
        public required string[] UserIdList { get; set; }
    }
}
