using mymooo.core.Attributes.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.qichacha.ResponseData
{
   
    public class CaseFilingCheckResponse
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public string OrderNumber { get; set; }

        public Page Paging { get; set; }

        public CaseFilingCheckReturn Result { get; set; }


        [RedisKey("mymooo-qichacha-casefling-part", databaseId: 7)]
        public class CaseFilingCheckReturn
        {
            public int VerifyResult { get; set; }
            [RedisPrimaryKey]
            public string Code { get; set; }
            [RedisMainField]
            public long LastDateWeek { get; set; }

            public DateTime LastSelectDate { get; set; }

            public List<CaseFilingCheckData> Data { get; set; }

            public class CaseFilingCheckData
            {
                /// <summary>
                /// 立案信息Id
                /// </summary>
                public string Id { get; set; }

                /// <summary>
                /// 案号
                /// </summary>
                public string CaseNo { get; set; }

                /// <summary>
                /// 法院名称
                /// </summary>
                public string Court { get; set; }

                /// <summary>
                /// 立案日期
                /// </summary>
                public string PunishDate { get; set; }

                /// <summary>
                /// 案由
                /// </summary>
                public string CaseReason { get; set; }
                /// <summary>
                /// 公诉人/原告/上诉人/申请人
                /// </summary>
                public List<KeyName> ProsecutorList { get; set; }

                /// <summary>
                /// 被告人/被告/被上诉人/被申请人
                /// </summary>
                public List<KeyName> DefendantList { get; set; }

                /// <summary>
                /// 当事人
                /// </summary>
                public List<Role> RoleList { get; set; }

                public class KeyName
                {
                    /// <summary>
                    /// 主键
                    /// </summary>
                    public string KeyNo { get; set; }

                    /// <summary>
                    /// 名称
                    /// </summary>
                    public string Name { get; set; }

                }
                /// <summary>
                /// 当事人
                /// </summary>
                public class Role
                {
                    /// <summary>
                    /// 当事人类型Code（0:原告，1:被告，2: 第三人，3:其他当事人）
                    /// </summary>
                    public string RoleType { get; set; }

                    /// <summary>
                    /// 当事人类型名称
                    /// </summary>
                    public string RoleName { get; set; }

                    /// <summary>
                    /// 当事人明细
                    /// </summary>
                    public List<KeyName> RoleItemList { get; set; }
                }
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
