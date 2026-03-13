using System;
using System.Linq;
using System.Text;
using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation
{
    [SugarTable("F_CUST_USER")]
    ///<summary>
    ///客户用户表F_CUST_USER
    ///</summary>
    public partial class CustomerUser
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public CustomerUser()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {

        }

        /// <summary>
        /// Desc:用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long FUM_USER_ID { get; set; }

        /// <summary>
        /// Desc:用户编号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_USER_NO { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户KEY
        /// Default:-1
        /// Nullable:True
        /// </summary>           
        public long? FUM_CUST_ID { get; set; }

        /// <summary>
        /// Desc:用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_USER_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户密码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_USER_PASSWORD { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户状态	   E:启用	   D:禁用
        /// Default:D
        /// Nullable:False
        /// </summary>           
        public string FUM_USER_STATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户类型	   C: 客户管理员 （创建客户时默认）	   N: 普通用户
        /// Default:N
        /// Nullable:False
        /// </summary>           
        public string FUM_USER_TYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime FUM_CREATE_DATE { get; set; }

        /// <summary>
        /// Desc:用户创建用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_CREATE_USER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户创建用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long FUM_CREATE_USER_ID { get; set; }

        /// <summary>
        /// Desc:用户修改时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime FUM_UPDATE_DATE { get; set; }

        /// <summary>
        /// Desc:用户修改用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_UPDATE_USER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户修改用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long FUM_UPDATE_USER_ID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FUM_USER_EMAIL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:微信用户OpenId
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_USER_OPENID { get; set; } = string.Empty;

        /// <summary>
        /// Desc:微信用户唯一UnionId
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_USER_UNIONID { get; set; } = string.Empty;

        /// <summary>
        /// Desc:最后一次登录时间
        /// Default:1900-01-01
        /// Nullable:False
        /// </summary>           
        public DateTime FUN_LOGIN_DATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUN_MOBILE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:微信，qq类别
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUM_OPEN_TYPE { get; set; } = string.Empty;

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
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FUserHeadImg { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FPasswordSalt { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long AuditedBy { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? AuditedOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long DisabledBy { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? DisabledOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long EnabledBy { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? EnabledOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long SubmittedBy { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? SubmittedOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte ClientType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool CreatedFrom { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string InvitationCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:4
        /// Nullable:False
        /// </summary>           
        public int DecimalPlacesOfUnitPrice { get; set; }

    }
}