using mymooo.core.Utils;
using mymooo.weixinWork.SDK.Media.Model;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model.ClockIn
{
    /// <summary>
    /// 可通过接口写入打卡记录，匹配打卡规则后可在企业微信打卡明细、统计中参与展示。
    /// </summary>
    public class AddCheckinRecordRequest
    {
        /// <summary>
        /// 打卡记录，一批最多200个
        /// </summary>
        public required AddCheckinRecord[] Records { get; set; }

        /// <summary>
        /// 打开记录
        /// </summary>
        public class AddCheckinRecord
        {
            private DateTime _checkinDate;

            /// <summary>
            /// 用户id
            /// </summary>
            [JsonPropertyName("userid")]
            public required string UserId { get; set; }

            /// <summary>
            /// 打卡时间
            /// </summary>
            public DateTime CheckInDate
            {
                get
                {
                    return _checkinDate;
                }
                set
                {
                    _checkinDate = value;
                    CheckinTimestamp = _checkinDate.GetTimestampSecond();
                }
            }
            /// <summary>
            /// 打卡时间。Unix时间戳
            /// </summary>
            [JsonPropertyName("checkin_time")]
            public long CheckinTimestamp { get; private set; }

            /// <summary>
            /// 打卡地点title，限制1024字符
            /// </summary>
            [JsonPropertyName("location_title")]
            public required string CheckinAddress { get; set; }

            /// <summary>
            /// 打卡地点详情限制1024字符
            /// </summary>
            [JsonPropertyName("location_detail")]
            public string AddressDetail { get; set; } = string.Empty;

            /// <summary>
            /// 打卡的附件media_id，可使用media/upload上传附件。当前最多只允许传1个
            /// </summary>
            public string[]? MediaIds { get; internal set; }

            /// <summary>
            /// 上传打卡图片
            /// </summary>
            public MediaInfo? MediaInfo { get; set; }

            /// <summary>
            /// 打卡备注限制1024字符
            /// </summary>
            public string? Notes { get; set; }

            /// <summary>
            /// 打卡设备类型：1、门禁 2、考勤机（人脸识别、指纹识别） 3、其他；
            /// </summary>
            [JsonPropertyName("device_type")]
            public int DeviceType { get; set; }

            /// <summary>
            /// 位置打卡地点纬度，是实际纬度的1000000倍，与腾讯地图一致采用GCJ-02坐标系统标准 范围 -90000000,90000000
            /// </summary>
            public int Lat { get; set; }

            /// <summary>
            /// 位置打卡地点经度，是实际经度的1000000倍，与腾讯地图一致采用GCJ-02坐标系统标准 范围-180000000,180000000
            /// </summary>
            public int Lng { get; set; }

            /// <summary>
            /// 打卡设备品牌：字符串写入（限制40个字符内）
            /// </summary>
            [JsonPropertyName("device_detail")]
            public string DeviceDetail { get; set; } = string.Empty;

            /// <summary>
            /// 	打卡wifi名称限制1024字符
            /// </summary>
            [JsonPropertyName("wifiname")]
            public string? WifiName { get; set; }

            /// <summary>
            /// 打卡的MAC地址/bssid 满足正则表达式^[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}$。传入wifiname时必填
            /// </summary>
            [JsonPropertyName("wifimac")]
            public string? WifiMac { get; set; }
        }

    }
}
