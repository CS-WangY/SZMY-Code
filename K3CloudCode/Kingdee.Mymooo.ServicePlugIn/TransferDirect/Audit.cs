using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Kingdee.Mymooo.ServicePlugIn.TransferDirect
{
    [Description("直接调拨单审核插件"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FStockOutOrgId");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FSrcStockId");//调出仓库
            e.FieldKeys.Add("FDestStockId");//调入仓库
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FOwnerOutId");//调出货主
        }
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            AuditValidator isPoValidator = new AuditValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }

        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            foreach (var data in e.DataEntitys)
            {
                //云仓储做出库
                var whDn = (ArrayList)(GetOutDelivery(data));
                if (whDn.Count > 0)
                {
                    var requestData = JsonConvertUtils.SerializeObject(whDn);
                    AddRabbitMessageUtils.AddRabbitMessage(this.Context, requestData, "CloudStockUpdateTempDeliveryArea", Convert.ToString(data["BillNo"]));
                }
                //云仓储做入库
                var whInDn = (ArrayList)(GetInDelivery(data));
                if (whInDn.Count > 0)
                {
                    var requestData = JsonConvertUtils.SerializeObject(whInDn);
                    AddRabbitMessageUtils.AddRabbitMessage(this.Context, requestData, "CloudStockUpdateTempStockArea", Convert.ToString(data["BillNo"]));
                }
            }

        }

        /// <summary>
        /// 构建云仓储做出库接口参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetOutDelivery(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            foreach (var item in data["TransferDirectEntry"] as DynamicObjectCollection)
            {
                if (item["SrcStockId"] != null)
                {
                    //是否同步云仓储
                    if (bool.Parse(((DynamicObject)item["SrcStockId"])["FSyncToWarehouse"].ToString()))
                    {
                        var result = new PutToTempDeliveryAreaRequest
                        {
                            DeliveryDetOrgCode = Convert.ToString(((DynamicObject)item["FOwnerOutId"])["Number"]),

                            ItemId = data["BillNo"] + "-" + item["Seq"],
                            ExWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
                            ModelNumber = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]),//物料编号
                            Name = Convert.ToString(((DynamicObject)item["MaterialID"])["Name"]),//物料名称
                            Specification = Convert.ToString(((DynamicObject)item["MaterialID"])["Specification"]),//物料规格型号
                            Quantity = Math.Abs(decimal.Parse(item["Qty"].ToString())),
                            ExWarehouseOnUtc = DateTime.Now.ToUniversalTime(),
                            Unit = new NewUnitModel
                            {
                                Name = Convert.ToString(((DynamicObject)item["UnitID"])["Number"])//单位编号
                            },
                            Type = new ExternalTypeModel
                            {
                                Value = "TOUT",
                                Description = "调拨出仓"
                            },
                            Remark = "仓库盘点",
                            LocCode = Convert.ToString(((DynamicObject)item["SrcStockId"])["FCloudStockCode"]),//(仓库对应的云仓储仓库编码)
                            DeliveryplaceCode = Convert.ToString(((DynamicObject)item["SrcStockId"])["FOutSourceStockLoc"]),//仓库发货区域
                        };
                        arrList.Add(result);
                    }
                }
            }
            return arrList;
        }

        /// <summary>
        /// 构建云仓储做入库接口参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetInDelivery(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            foreach (var item in data["TransferDirectEntry"] as DynamicObjectCollection)
            {
                if (item["DestStockId"] != null)
                {
                    //是否同步云仓储
                    if (bool.Parse(((DynamicObject)item["DestStockId"])["FSyncToWarehouse"].ToString()))
                    {
                        var result = new PutToTempStockAreaRequest
                        {
                            DeliveryDetOrgCode = Convert.ToString(((DynamicObject)item["FOwnerOutId"])["Number"]),
                            ItemId = data["BillNo"] + "-" + item["Seq"],
                            EntryWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
                            ModelNumber = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]),//物料编号
                            Name = Convert.ToString(((DynamicObject)item["MaterialID"])["Name"]),//物料名称
                            Specification = Convert.ToString(((DynamicObject)item["MaterialID"])["Specification"]),//物料规格型号
                            Quantity = Math.Abs(decimal.Parse(item["Qty"].ToString())),
                            EntryOnUtc = DateTime.Now,
                            Unit = new NewUnitModel
                            {
                                Name = Convert.ToString(((DynamicObject)item["UnitID"])["Number"])//单位编号
                            },
                            Type = new ExternalTypeModel
                            {
                                Value = "AOUT",
                                Description = "调整出仓"
                            },
                            Remark = "仓库盘点",
                            LocCode = Convert.ToString(((DynamicObject)item["DestStockId"])["FCloudStockCode"]),//(仓库对应的云仓储仓库编码)
                            DeliveryplaceCode = Convert.ToString(((DynamicObject)item["DestStockId"])["FOutSourceStockLoc"]),//仓库发货区域
                        };
                        arrList.Add(result);
                    }
                }
            }
            return arrList;
        }

    }
}
