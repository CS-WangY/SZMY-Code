using Kingdee.BOS;
using Kingdee.Mymooo.Core.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class ENGBomInfo : BaseInfo
    {
        public ENGBomInfo(string name)
        {
            this.NumberFieldName = "FNumber";
            this.DocumentStatusFieldName = "FDocumentStatus";
            this.ForbidStatusFieldName = "FForbidStatus";
            this.IdFieldNmber = "FID";
            this.NumberKDDbType = KDDbType.String;
            this.OrgNumberFieldName = "FUseOrgId";
            this.Name = name;
            this.FormId = "ENG_BOM";
        }
        public int FCreateOrgId { get; set; }
        public string FMATERIALID { get; set; }
        public string FMATERIALNumber { get; set; }
        public List<BomEntity> Entity { get; set; }
        public string FNUMBER { get; set; }
        public bool AutoCraft { get; set; }
        public long SalBillId { get; set; }
        public long SalBillEntryId { get; set; }
        /// <summary>
        /// 工艺ID
        /// </summary>
        public string ProcessID { get; set; }
    }
    public class BomEntity
    {
        public string FMATERIALIDCHILD { get; set; }
        public string FMATERIALIDCHILDNumber { get; set; }
        public string FUnitNumber { get; set; }
        public decimal FNUMERATOR { get; set; }
        /// <summary>
        /// 变动损耗率
        /// </summary>
        public decimal FSCRAPRATE { get; set; }
        public bool SendMes { get; set; }
        /// <summary>
        /// 用量分母
        /// </summary>
        public decimal FDENOMINATOR { get; set; }
        /// <summary>
        /// 发料方式 1/空-直接领料0-不发料
        /// </summary>
        public bool IsSueType { get; set; } = true;
        /// <summary>
        /// 是否委外 1-委外0/空-自制
        /// </summary>
        public bool ErpClsID { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string EntryNote { get; set; } = "";
        /// <summary>
        /// 导入价格
        /// </summary>
        public decimal ImportAmount { get; set; } = 0;
    }
}
