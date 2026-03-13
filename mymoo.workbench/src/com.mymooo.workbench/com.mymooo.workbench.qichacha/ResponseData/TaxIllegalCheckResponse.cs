using mymooo.core.Attributes.Redis;
using NPOI.OpenXmlFormats.Wordprocessing;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.qichacha.ResponseData
{
    public class TaxIllegalCheckResponse
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public string OrderNumber { get; set; }

        public Page Paging { get; set; }

        public TaxIllegalCheckReturn Result { get; set; }

        [RedisKey("mymooo-qichacha-taxillegal-part", databaseId: 7)]
        public class TaxIllegalCheckReturn
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
            public long lastDateWeek { get; set; }

            /// <summary>
            /// 最后一次查询时间
            /// </summary>
            public DateTime LastSelectDate { get; set; }

            /// <summary>
            /// 数据信息
            /// </summary>
            public List<TaxIllegalCheckData> Data { get; set; }

            public class TaxIllegalCheckData
            {
                /// <summary>
                /// 主键
                /// </summary>
                public string Id { get; set; }

                /// <summary>
                /// 发布日期
                /// </summary>
                public string PublishDate { get; set; }

                /// <summary>
                /// 案件性质
                /// </summary>
                public string CaseNature { get; set; }

                /// <summary>
                /// 所属税务机关
                /// </summary>
                public string TaxGov { get; set; }

                /// <summary>
                /// 主要违法事实
                /// </summary>
                public string IllegalContent { get; set; }

                /// <summary>
                /// 法律依据及处理处罚情况
                /// </summary>
                public string PunishContent { get; set; }
            }
        }



        public class Page
        {
            public int PageSize { get; set; }
            public int PageIndex { get; set; }
            public int TotalRecords { get; set; }
        }

    }
}
