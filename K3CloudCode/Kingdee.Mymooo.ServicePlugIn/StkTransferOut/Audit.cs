using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.StkTransferOut
{
    [Description("分步式调出单批核插件"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FSrcStockID");//调出仓库
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FOwnerID");//调出货主
            e.FieldKeys.Add("FStockOrgID");//调出库存组织
            e.FieldKeys.Add("FPENYReturnType");//特结=0 退货=1
            e.FieldKeys.Add("FPENYDeliveryNotice");//发货通知单单号
            e.FieldKeys.Add("FPENYSalReturnNo");//退货单号
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
            //退货
            if (Convert.ToString(data["FPENYReturnType"]).Equals("1"))
            {
                //退货订单如果供货组织包含全国一部华南二部则退出
                if (Convert.ToInt64(data["StockInOrgID_Id"]) == 7401780
                    || Convert.ToInt64(data["StockInOrgID_Id"]) == 7401781)
                {
                    return arrList;
                }
                foreach (var item in data["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection)
                {
                    if (item["SrcStockId"] != null)
                    {
                        //是否同步云仓储
                        if (bool.Parse(((DynamicObject)item["SrcStockId"])["FSyncToWarehouse"].ToString()))
                        {
                            var result = new PutToTempDeliveryAreaRequest
                            {
                                DeliveryDetOrgCode = Convert.ToString(((DynamicObject)item["OwnerID"])["Number"]),
                                IsAutoHandle = true,
                                ItemId = data["BillNo"] + "-" + item["Seq"],
                                ExWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
                                ModelNumber = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]),//物料编号
                                Name = Convert.ToString(((DynamicObject)item["MaterialID"])["Name"]),//物料名称
                                Specification = Convert.ToString(((DynamicObject)item["MaterialID"])["Specification"]),//物料规格型号
                                Quantity = Math.Abs(decimal.Parse(item["FQty"].ToString())),
                                ExWarehouseOnUtc = DateTime.Now.ToUniversalTime(),
                                Unit = new NewUnitModel
                                {
                                    Name = Convert.ToString(((DynamicObject)item["UnitID"])["Number"])//单位编号
                                },
                                Type = new ExternalTypeModel
                                {
                                    Value = "RSOTOUT",
                                    Description = "销售退货调拨出仓"
                                },
                                Remark = item["FPENYSalReturnNo"].ToString(),
                                LocCode = Convert.ToString(((DynamicObject)item["SrcStockId"])["FCloudStockCode"]),//(仓库对应的云仓储仓库编码)
                                IsDirectDeliveryStock = Convert.ToBoolean(((DynamicObject)item["SrcStockId"])["FIsDirStock"]),//(仓库对应的是否直发仓库)
                                DeliveryplaceCode = Convert.ToString(((DynamicObject)item["SrcStockId"])["FOutSourceStockLoc"])//仓库对应的仓库发货区域
                            };
                            arrList.Add(result);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in data["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection)
                {
                    if (item["SrcStockId"] != null)
                    {
                        //是否同步云仓储
                        if (bool.Parse(((DynamicObject)item["SrcStockId"])["FSyncToWarehouse"].ToString()))
                        {
                            var IsAutoHandle = false;
                            //存在发货通知单单号才判断自动上下架状态
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(item["FPENYDeliveryNotice"])))
                            {
                                IsAutoHandle = Boolean.Parse(((DynamicObject)data["StockOrgID"])["FTransIsSynCloudStock"].ToString());
                            }

                            var result = new PutToTempDeliveryAreaRequest
                            {
                                DeliveryDetOrgCode = Convert.ToString(((DynamicObject)item["OwnerID"])["Number"]),
                                IsAutoHandle = IsAutoHandle,
                                ItemId = data["BillNo"] + "-" + item["Seq"],
                                ExWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
                                ModelNumber = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]),//物料编号
                                Name = Convert.ToString(((DynamicObject)item["MaterialID"])["Name"]),//物料名称
                                Specification = Convert.ToString(((DynamicObject)item["MaterialID"])["Specification"]),//物料规格型号
                                Quantity = Math.Abs(decimal.Parse(item["FQty"].ToString())),
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
                                Remark = "分步式调拨",
                                LocCode = Convert.ToString(((DynamicObject)item["SrcStockId"])["FCloudStockCode"]),//(仓库对应的云仓储仓库编码)
                                IsDirectDeliveryStock = Convert.ToBoolean(((DynamicObject)item["SrcStockId"])["FIsDirStock"]),//(仓库对应的是否直发仓库)
                                DeliveryplaceCode = Convert.ToString(((DynamicObject)item["SrcStockId"])["FOutSourceStockLoc"])//仓库对应的仓库发货区域
                            };
                            arrList.Add(result);
                        }
                    }
                }
            }

            return arrList;
        }
    }
}
