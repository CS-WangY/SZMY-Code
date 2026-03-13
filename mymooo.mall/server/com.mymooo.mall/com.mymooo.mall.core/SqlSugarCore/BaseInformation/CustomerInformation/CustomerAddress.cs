using System;
using System.Linq;
using System.Text;
using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("F_CUST_ADDR")]
    public partial class CustomerAddress
    {
        public CustomerAddress()
        {

        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FCA_ADDR_ID")]
        public long Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>         
        [SugarColumn(ColumnName = "FCA_ADDR_CHI")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCA_ADDR_ZIP")]
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc: 联系人
        /// Default:
        /// Nullable:False
        /// </summary>         
        [SugarColumn(ColumnName = "FCA_LINKMAN")]
        public string Receiver { get; set; } = string.Empty;

        /// <summary>
        /// Desc:部门
        /// Default:
        /// Nullable:False
        /// </summary>       
        [SugarColumn(ColumnName = "FCA_LINKMAN_DEPT")]
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Desc:传真
        /// Default:
        /// Nullable:False
        /// </summary>
        public string FCA_FAX { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCA_TEL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCA_MOBILE")]
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCA_EMAIL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:是否缺省地址的数据
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte FCA_DEFAULT_IND { get; set; }

        /// <summary>
        /// 是否缺省地址, 不存表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsDefault
        {
            get
            {
                return FCA_DEFAULT_IND == 1;
            }
            set
            {
                FCA_DEFAULT_IND = IsDefault ? (byte)1 : (byte)0;
            }
        }


        /// <summary>
        /// 省Id , 不存表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int ProvinceId
        {
            get
            {
                var arrArea = FCA_Area.Split('_');
                return arrArea.Length > 0 ? int.Parse(FCA_Area.Split('_')[0]) : 0;
            }
        }

        /// <summary>
        /// 城Id , 不存表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int CityId
        {
            get
            {
                var arrArea = FCA_Area.Split('_');
                return arrArea.Length > 1 ? int.Parse(FCA_Area.Split('_')[1]) : 0;
            }
        }

        /// <summary>
        /// 城Id , 不存表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int DistrictId
        {
            get
            {
                var arrArea = FCA_Area.Split('_');
                return arrArea.Length > 2 ? int.Parse(FCA_Area.Split('_')[2]) : 0;
            }
        }


        /// <summary>
        /// 省Id , 不存表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string Province
        {
            get
            {
                var arrArea = FCA_AreaName.Split('_');
                return arrArea.Length > 0 ? FCA_AreaName.Split('_')[0] : string.Empty;
            }
        }

        /// <summary>
        /// 城 , 不存表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string City
        {
            get
            {
                var arrArea = FCA_AreaName.Split('_');
                return arrArea.Length > 1 ? FCA_AreaName.Split('_')[1] : string.Empty;
            }
        }

        /// <summary>
        /// 城Id , 不存表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string District
        {
            get
            {
                var arrArea = FCA_AreaName.Split('_');
                return arrArea.Length > 2 ? FCA_AreaName.Split('_')[2] : string.Empty;
            }
        }


        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime FCA_CREATE_DATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCA_CREATE_USER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long FCA_CREATE_USER_ID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime FCA_UPDATE_DATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FCA_UPDATE_USER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public long FCA_UPDATE_USER_ID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>      
        [SugarColumn(ColumnName = "FCA_AddressAlias")]
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCA_Area { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FCA_AreaName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCA_Company")]
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long CustomerId { get; set; }

        /// <summary>
        /// Desc:GUID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? CompanyId { get; set; }

    }
}