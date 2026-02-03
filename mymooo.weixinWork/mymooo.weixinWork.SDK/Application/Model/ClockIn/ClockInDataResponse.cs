using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model.ClockIn
{
    /// <summary>
    /// 获取可见范围内员工指定时间段内的打卡记录数据 响应
    /// </summary>
    public class ClockInDataResponse
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
        /// 打卡明细
        /// </summary>
        [JsonPropertyName("checkindata")]
        public ClockInDataDetail[]? ClockInDatas { get; set; }

        /// <summary>
        /// 打卡数据
        /// </summary>
        public class ClockInDataDetail
        {
            private int _checkinTimestamp;
            private int _standardCheckinTimestamp;

            /// <summary>
            /// 用户id
            /// </summary>
            public required string UserId { get; set; }

            /// <summary>
            /// 打卡规则名称
            /// </summary>
            public string? GroupName { get; set; }

            /// <summary>
            /// 打卡类型。字符串，目前有：上班打卡，下班打卡，外出打卡
            /// </summary>
            [JsonPropertyName("checkin_type")]
            public string CheckinType { get; set; } = string.Empty;

            /// <summary>
            /// 异常类型，字符串，包括：时间异常，地点异常，未打卡，wifi异常，非常用设备。如果有多个异常，以分号间隔
            /// </summary>
            [JsonPropertyName("exception_type")]
            public string ExceptionType { get; set; } = string.Empty;

            /// <summary>
            /// 打卡时间。Unix时间戳
            /// </summary>
            [JsonPropertyName("checkin_time")]
            public int CheckinTimestamp
            {
                get
                {
                    return _checkinTimestamp;
                }
                set
                {
                    _checkinTimestamp = value;
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(_checkinTimestamp);
                    CheckoutDate = dateTimeOffset.LocalDateTime;
                }
            }

            /// <summary>
            /// 打开时间
            /// </summary>
            public DateTime CheckoutDate { get; set; }

            /// <summary>
            /// 打卡地点title
            /// </summary>
            [JsonPropertyName("location_title")]
            public string CheckinAddress { get; set; } = string.Empty;

            /// <summary>
            /// 打卡地点详情
            /// </summary>
            [JsonPropertyName("location_detail")]
            public string AddressDetail { get; set; } = string.Empty;

            /// <summary>
            /// 打卡wifi名称
            /// </summary>
            public string WiFiName { get; set; } = string.Empty;

            /// <summary>
            /// 打卡设备id
            /// </summary>
            public string DeviceId { get; set; } = string.Empty;
            /// <summary>
            /// 打卡备注
            /// </summary>
            public string Notes { get; set; } = string.Empty;

            /// <summary>
            /// 打卡的MAC地址/bssid
            /// </summary>
            public string WifiMac { get; set; } = string.Empty;

            /// <summary>
            /// 打卡的附件media_id，可使用media/get获取附件
            /// </summary>
            public string[] Mediaids { get; set; } = [];

            /// <summary>
            /// 标准打卡时间，指此次打卡时间对应的标准上班时间或标准下班时间
            /// </summary>
            [JsonPropertyName("sch_checkin_time")]
            public int StandardCheckinTimestamp
            {
                get
                {
                    return _standardCheckinTimestamp;
                }
                set
                {
                    _standardCheckinTimestamp = value;
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(_standardCheckinTimestamp);
                    StandardCheckoutDate = dateTimeOffset.LocalDateTime;
                }
            }

            /// <summary>
            /// 标准打卡时间
            /// </summary>
            public DateTime StandardCheckoutDate { get; set; }

            /// <summary>
            /// 规则id，表示打卡记录所属规则的id
            /// </summary>
            public int GroupId { get; set; }

            /// <summary>
            /// 班次id，表示打卡记录所属规则中，所属班次的id
            /// </summary>
            [JsonPropertyName("schedule_id")]
            public int ScheduleId { get; set; }

            /// <summary>
            /// 时段id，表示打卡记录所属规则中，某一班次中的某一时段的id，如上下班时间为9:00-12:00、13:00-18:00的班次中，9:00-12:00为其中一组时段
            /// </summary>
            [JsonPropertyName("timeline_id")]
            public int TimelineId { get; set; }
        }

    }
}
