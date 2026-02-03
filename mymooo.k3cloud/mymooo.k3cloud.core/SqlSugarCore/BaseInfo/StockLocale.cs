using System;
using System.Linq;
using System.Text;
using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.BaseInfo
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("T_BD_STOCK_L")]
    public partial class StockLocale
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public int FPKID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FSTOCKID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FLOCALEID { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FNAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FDESCRIPTION { get; set; } = string.Empty;

    }
}