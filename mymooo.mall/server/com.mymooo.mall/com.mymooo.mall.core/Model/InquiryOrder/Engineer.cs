using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.InquiryOrder
{

    /// <summary>
    /// 询价单报价状态。
    /// </summary>
    public class Engineer
    {
       public long Id { get; set; } 
       public string Name { get; set; } = string.Empty;
    }


    public class ProductCodeEnginer
    {
        public Guid DepartId { get; set; }  
        public string ProductCode { get; set; } = string.Empty;
        public long EngineerId { get; set; }
        public long EngineerManagerId { get; set; }

        public string EngineerName { get; set;} = string.Empty;
        public string EngineerManagerName { get; set;} = string.Empty;

    }

    public class EngineerIdAndMangerId
    {
        public long ProductEngineerId { get; set; }
        public long ProductManagerId { get; set; }
        public Guid DepartId { get; set; }

        public int DepartmentLevel { get; set; }
    }

    public class ConfigeEngineer
    {
        public Guid DeptId { get; set; }
        public long EnMId { get; set; }
        public long EnId { get; set; }

        public string EnName { get; set; } = string.Empty;
        public string EnMName { get; set; } = string.Empty;

    }
}
