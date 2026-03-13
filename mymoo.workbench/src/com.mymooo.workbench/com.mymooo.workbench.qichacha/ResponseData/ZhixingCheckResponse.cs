using mymooo.core.Attributes.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.qichacha.ResponseData
{
    public class ZhixingCheckResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 单号
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        public Page Paging { get; set; } = new Page();

        public ZhixingCheckReturn Result { get; set; }

        public class Page
        {
            public int PageSize { get; set; }
            public int PageIndex { get; set; }
            public int TotalRecords { get; set; }
        }
        [RedisKey("mymooo-qichacha-zhixingcheck-part", databaseId: 7)]
        public class ZhixingCheckReturn
        {
            /// <summary>
            /// 数据是否存在(1-存在，0-不存在)
            /// </summary>
            public int VerifyResult { get; set; }

            [RedisPrimaryKey]
            public string Code { get; set; }

            [RedisMainField]
            public long LastDateWeek { get; set; }

            /// <summary>
            /// 最后查询时间
            /// </summary>
            public DateTime LastSelectDate { get; set; }


            /// <summary>
            /// 数据信息
            /// </summary>
            public List<ZhixingCheckData> Data { get; set; }
            public class ZhixingCheckData
            {
                /// <summary>
                /// 主键
                /// </summary>
                public string Id { get; set; }

                /// <summary>
                /// 立案时间
                /// </summary>
                public string Liandate { get; set; }

                /// <summary>
                /// 案号
                /// </summary>
                public string Anno { get; set; }

                /// <summary>
                /// 执行法院
                /// </summary>
                public string ExecuteGov { get; set; }

                /// <summary>
                /// 执行标的（元）
                /// </summary>
                public string Biaodi { get; set; }
                /// <summary>
                /// 疑似申请执行人
                /// </summary>
                public string SuspectedApplicant { get; set; }
            }
        }
    }
}
