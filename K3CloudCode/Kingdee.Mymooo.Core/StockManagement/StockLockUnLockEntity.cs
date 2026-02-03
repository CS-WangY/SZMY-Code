using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
    public class StockLockUnLockEntity
    {
        /// <summary>
        /// 库存组织
        /// </summary>
        public long StockOrgID { get; set; } = 0;
        /// <summary>
        /// 物料内码
        /// </summary>
        public long MaterialID { get; set; } = 0;
        /// <summary>
        /// 锁库数量
        /// </summary>
        public decimal FLockQTY { get; set; } = 0;
        /// <summary>
        /// 单据对象内码
        /// </summary>
        public string ObjectId { get; set; }
        /// <summary>
        /// 单据内码
        /// </summary>
        public string BillId { get; set; }
        /// <summary>
        /// 单据明细内码
        /// </summary>
        public string BillDetailID { get; set; }
        /// <summary>
        /// 单据类型内码
        /// </summary>
        public string BillTypeID { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 单据行序号
        /// </summary>
        public int BillSEQ { get; set; } = 0;
        public long StockID { get; set; } = 0;
    }
    public class StockPushEntity
    {
        public string FID { get; set; }
        public string EntryID { get; set; }
        public List<ListSelectedRow> listSelectedRow { get; set; }
        public string EntryEntityKey { get; set; }
        public string ConvertRule { get; set; }
        public long TargetOrgId { get; set; }
        public List<ReplaceVal> replaceVals { get; set; }
        public bool SetVariableValue { get; set; }
        public string TargetBillTypeId { get; set; } = string.Empty;
    }
    public class Allocate
    {
        public long CreatorId_Id { get; set; }
        public DynamicObject CreatorId { get; set; }

        public string FID { get; set; }
        public string FENTRYID { get; set; }
        public string TargetOrgId { get; set; }
        //调入物料
        public DynamicObject FDestMaterialID { get; set; }
        /// <summary>
        /// 物料内码
        /// </summary>
        public long FMaterialId { get; set; }
        /// <summary>
        /// 调拨数量
        /// </summary>
        public decimal FQTY { get; set; }
        /// <summary>
        /// 调拨基本数量
        /// </summary>
        public decimal FBASEQTY { get; set; }
        /// <summary>
        /// 调出仓库
        /// </summary>
        public DynamicObject FSrcStockId { get; set; }
        public long FSrcStock_Id { get; set; }
        /// <summary>
        /// 调入仓库
        /// </summary>
        public DynamicObject FDestStockId { get; set; }
        public long FDestStock_Id { get; set; }
        public string DeliveryNoticeNumber { get; set; }
        public long DeliveryNoticeSEQ { get; set; }
        public long DeliveryNoticeID { get; set; }
        public long DeliveryNoticeEntryID { get; set; }
        public string SalBillNo { get; set; }
        public string SalBillSEQ { get; set; }
    }

    public class EntryItem
    {
        /// <summary>
        /// 调出物料
        /// </summary>
        public string FMaterialID { get; set; }
        /// <summary>
        /// 调出数量
        /// </summary>
        public decimal FQty { get; set; }
        /// <summary>
        /// 调出仓库
        /// </summary>
        public string FSrcStockID { get; set; }
        /// <summary>
        /// 调入仓库
        /// </summary>
        public string FDestStockID { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string FEntryNote { get; set; }
    }

    public class TransferOut
    {
        public string FSrcBillNo { get; set; }
        /// <summary>
        /// 调出组织
        /// </summary>
        public string FStockOrgID { get; set; }
        /// <summary>
        /// 调入组织
        /// </summary>
        public string FStockInOrgID { get; set; }
        /// <summary>
        /// 分录
        /// </summary>
        public List<EntryItem> Entry { get; set; }
    }
}
