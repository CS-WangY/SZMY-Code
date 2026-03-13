using System;
using System.Linq;
using System.Text;
using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation
{
    ///<summary>
    ///客户资料表（平台级别）
    ///补充: F_CUST_MSTR这个表关联  F_CUST_USER 相对于其是从表, 对于后台来说是客户主表
    ///</summary>
    [SugarTable("F_CUST_MSTR")]
    public partial class CustomerUserMstr
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public CustomerUserMstr()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {

        }

        /// <summary>
        /// Desc:客户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long FCM_CUST_ID { get; set; }

        /// <summary>
        /// Desc:客户编号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_CUST_CODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户简称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_SHORT_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户名称中文
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_NAME_CHI { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户名称英文
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_NAME_ENG { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户地址中文
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_ADDR_CHI { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户地址英文
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_ADDR_ENG { get; set; } = string.Empty;

        /// <summary>
        /// Desc:国家
        /// Default:-1
        /// Nullable:True
        /// </summary>           
        public long? FCM_COUNTRY_ID { get; set; }

        /// <summary>
        /// Desc:省
        /// Default:-1
        /// Nullable:True
        /// </summary>           
        public long? FCM_PROVINCE_ID { get; set; }

        /// <summary>
        /// Desc:市
        /// Default:-1
        /// Nullable:True
        /// </summary>           
        public long? FCM_CITY_ID { get; set; }

        /// <summary>
        /// Desc:县
        /// Default:-1
        /// Nullable:True
        /// </summary>           
        public long? FCM_COUNTY_ID { get; set; }

        /// <summary>
        /// Desc:客户平台状态（正常、冻结、终止）	   N:正常	   F:冻结	   T:终止
        /// Default:N'N'
        /// Nullable:False
        /// </summary>           
        public string FCM_CUST_STATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:法人代表
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_REGISTER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:商业登记证号（唯一性）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_REGISTER_CODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:注册资金
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_REGISTER_FUND { get; set; } = string.Empty;

        /// <summary>
        /// Desc:公司注册性质
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_REGISTER_CLASS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:联系人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_LINKMAN { get; set; } = string.Empty;

        /// <summary>
        /// Desc:邮编
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_ZIP_CODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:网址
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_HOMEPAGE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:备注
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCM_REMARK { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FCM_APP_IND { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FCM_LAST_CN { get; set; }

        /// <summary>
        /// Desc:客户信用额度
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal FCM_MAX_CREDIT { get; set; }

        /// <summary>
        /// Desc:客户已使用信用额度
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal FCM_USED_CREDIT { get; set; }

        /// <summary>
        /// Desc:客户创建日期
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime FCM_CREATE_DATE { get; set; }

        /// <summary>
        /// Desc:客户创建用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_CREATE_USER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户创建用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long FCM_CREATE_USER_ID { get; set; }

        /// <summary>
        /// Desc:客户修改日期
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime FCM_UPDATE_DATE { get; set; }

        /// <summary>
        /// Desc:客户修改用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_UPDATE_USER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户修改用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long FCM_UPDATE_USER_ID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_BANK { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_ACCOUNT { get; set; } = string.Empty;

        /// <summary>
        /// Desc:开户账号名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_ACCOUNT_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户类型
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_CUST_TYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:主营行业
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_CUST_INDUSTRY { get; set; } = string.Empty;

        /// <summary>
        /// Desc:选择‘其他行业’时的需输入描述
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_INDUSTRY_DESC { get; set; } = string.Empty;

        /// <summary>
        /// Desc:省名
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_PROVINCE_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:市名
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_CITY_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:县名
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_COUNTY_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCM_Sex { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_DeptName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_CompanyScale { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:2
        /// Nullable:False
        /// </summary>           
        public byte CustomerStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? DataSourice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:4
        /// Nullable:False
        /// </summary>           
        public int DecimalPlacesOfUnitPrice { get; set; }

    }
}