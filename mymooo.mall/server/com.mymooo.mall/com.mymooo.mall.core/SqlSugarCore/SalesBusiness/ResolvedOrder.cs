using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
    ///<summary>
    ///
    ///</summary>
    public partial class ResolvedOrder
    {
        public ResolvedOrder()
		{

        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, InsertSql = "NEXT VALUE FOR billseq")]
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long InquiryOrderId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:newid()
        /// Nullable:False
        /// </summary>           
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(InsertSql = "GETDATE()")]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal TotalWithTax { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long ProductEngineerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte OrderType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public int ExpectedDeliveryDays { get; set; }

        /// <summary>
        /// Desc:
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public decimal ExpectedTotalWithTax { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long ProductManagerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:NULL
        /// Nullable:True
        /// </summary>           
        public string RProductEngineerName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:NULL
        /// Nullable:True
        /// </summary>           
        public string RProductManagerName { get; set; } = string.Empty;

    }
}