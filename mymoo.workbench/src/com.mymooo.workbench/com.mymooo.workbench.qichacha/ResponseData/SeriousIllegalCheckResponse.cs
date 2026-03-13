using mymooo.core.Attributes.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.qichacha.ResponseData
{
    public class SeriousIllegalCheckResponse
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public string OrderNumber { get; set; }


        public SeriousIllegalCheckReturn Result { get; set; }

        [RedisKey("mymooo-qichacha-seriousillegal-part", databaseId: 7)]
        public class SeriousIllegalCheckReturn
        {
            /// <summary>
            /// 数据是否存在(1-存在，0-不存在)
            /// </summary>
            public int VerifyResult { get; set; }
            [RedisPrimaryKey]
            public string Code { get; set; }

            /// <summary>
            /// 最后一次查询
            /// </summary>
            [RedisMainField]
            public long LastDateWeek { get; set; }

            public DateTime LastSelectDate { get; set; }

            /// <summary>
            /// 数据信息
            /// </summary>
            public List<SeriousIllegalCheckData> Data { get; set; }

            public class SeriousIllegalCheckData
            {
                /// <summary>
                /// 类型（保留字段）
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                /// 列入原因
                /// </summary>
                public string AddReason { get; set; }

                /// <summary>
                /// 列入时间
                /// </summary>
                public string AddDate { get; set; }

                /// <summary>
                /// 列入决定机关
                /// </summary>
                public string AddOffice { get; set; }

                /// <summary>
                /// 移除原因（保留字段）
                /// </summary>
                public string RemoveReason { get; set; }

                /// <summary>
                /// 移除时间（保留字段）
                /// </summary>
                public string RemoveDate { get; set; }

                /// <summary>
                /// 移除决定机关（保留字段）
                /// </summary>
                public string RemoveOffice { get; set; }
            }
        }
    }
}
