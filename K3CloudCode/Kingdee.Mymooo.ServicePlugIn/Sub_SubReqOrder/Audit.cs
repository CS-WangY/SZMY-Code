using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.ShareCenter.Enum;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.Core;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.App.Core.ProductionManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Kingdee.Mymooo.Core.SubReqOrderManagement;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Mymooo.ServicePlugIn.Sub_SubReqOrder
{
    [Description("委外订单审核-获取图纸发送MQ"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FDescription");
            e.FieldKeys.Add("FPPBOMType");
            e.FieldKeys.Add("FProductType");
            e.FieldKeys.Add("FUnitId");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FPlanStartDate");
            e.FieldKeys.Add("FPlanFinishDate");
            e.FieldKeys.Add("FBomId");
            e.FieldKeys.Add("FSrcBillId");
            e.FieldKeys.Add("FSrcBillId");
            e.FieldKeys.Add("FSrcBillEntryId");
            e.FieldKeys.Add("FSaleOrderId");
            e.FieldKeys.Add("FSaleOrderEntryId");
            e.FieldKeys.Add("FPurOrderId");
            e.FieldKeys.Add("FPurOrderEntryId");
            e.FieldKeys.Add("FStockID");
            e.FieldKeys.Add("FScheduleStatus");
            e.FieldKeys.Add("FPickMtrlStatus");
            e.FieldKeys.Add("FDescription1");
            e.FieldKeys.Add("FSrcBillType");
            e.FieldKeys.Add("FSrcBillNo");
            e.FieldKeys.Add("FSrcBillEntrySeq");
            e.FieldKeys.Add("FSALEORDERNO");
            e.FieldKeys.Add("FSaleOrderEntrySeq");
            e.FieldKeys.Add("FSMALLID");
            e.FieldKeys.Add("FPARENTSMALLID");
            e.FieldKeys.Add("FBUSINESSDIVISIONID");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                SubReqOrderEntity subReqOrderEntity = new SubReqOrderEntity();
                subReqOrderEntity.FQty = 1;
                this.CreateMakeRequest(item);
            }
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            AuditValidator isPoValidator = new AuditValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }

        private void CreateMakeRequest(DynamicObject data)
        {
            var fid = Convert.ToInt64(data["Id"]);

            SubReqOrder subReqOrder = new SubReqOrder();
            subReqOrder.Id = fid;
            subReqOrder.BillNo = Convert.ToString(data["BillNo"]);
            subReqOrder.BillType = Convert.ToString(((DynamicObject)data["BillType"])["Name"]);
            subReqOrder.Date = Convert.ToString(data["ApproveDate"]);
            subReqOrder.DocumentStatus = Convert.ToString(data["DocumentStatus"]);
            subReqOrder.SubOrgId = Convert.ToString(((DynamicObject)data["SubOrgId"])["Name"]);
            subReqOrder.Description = Convert.ToString(data["Description"]);
            subReqOrder.PPBOMType = Convert.ToString(data["PPBOMType"]);

            List<SubReqOrderEntity> subReqOrderEntities = new List<SubReqOrderEntity>();
            foreach (var item in (DynamicObjectCollection)data["TreeEntity"])
            {
                SubReqOrderEntity subReqOrderEntity = new SubReqOrderEntity();
                subReqOrderEntity.FProductType = Convert.ToString(item["ProductType"]);
                subReqOrderEntity.FMaterialId = Convert.ToString(((DynamicObject)item["MaterialId"])["Id"]);
                subReqOrderEntity.FMaterialNumber = Convert.ToString(((DynamicObject)item["MaterialId"])["Number"]);
                subReqOrderEntity.FMaterialName = Convert.ToString(((DynamicObject)item["MaterialId"])["Name"]);
                subReqOrderEntity.FSpecification = Convert.ToString(((DynamicObject)item["MaterialId"])["Specification"]);
                subReqOrderEntity.FUnit = Convert.ToString(((DynamicObject)item["UnitId"])["Number"]);
                subReqOrderEntity.FQty = Convert.ToDecimal(item["Qty"]);
                subReqOrderEntity.FPlanStartDate = Convert.ToString(item["PlanStartDate"]);
                subReqOrderEntity.FPlanFinishDate = Convert.ToString(item["PlanFinishDate"]);
                if (((DynamicObject)item["BomId"]) != null)
                {
                    subReqOrderEntity.FBom = Convert.ToString(((DynamicObject)item["BomId"])["Number"]);
                    subReqOrderEntity.BomChildren = GetZCZBom(Convert.ToInt64(item["BomId_Id"]));
                }
                
                //if (subReqOrderEntity.BomChildren.Count <= 0)
                //{
                //    continue;
                //}
                subReqOrderEntity.FSrcBillId = Convert.ToString(item["SrcBillId"]);
                subReqOrderEntity.FSrcBillEntryId = Convert.ToString(item["SrcBillEntryId"]);
                subReqOrderEntity.FSaleOrderId = Convert.ToString(item["SaleOrderId"]);
                subReqOrderEntity.FSaleOrderEntryId = Convert.ToString(item["SaleOrderEntryId"]);
                subReqOrderEntity.FPurOrderId = Convert.ToString(item["PurOrderId"]);
                subReqOrderEntity.FPurOrderEntryId = Convert.ToString(item["PurOrderEntryId"]);
                //subReqOrderEntity.FStockID = Convert.ToString(((DynamicObject)item["StockID"])["Number"]);
                subReqOrderEntity.FScheduleStatus = Convert.ToString(item["ScheduleStatus"]);
                subReqOrderEntity.FPickMtrlStatus = Convert.ToString(item["PickMtrlStatus"]);
                subReqOrderEntity.FDescription1 = Convert.ToString(item["Description"]);
                subReqOrderEntity.FSrcBillType = Convert.ToString(item["SrcBillType"]);
                subReqOrderEntity.FSrcBillNo = Convert.ToString(item["SrcBillNo"]);
                subReqOrderEntity.FSrcBillEntrySeq = Convert.ToString(item["SrcBillEntrySeq"]);
                subReqOrderEntity.FSALEORDERNO = Convert.ToString(item["SALEORDERNO"]);
                subReqOrderEntity.FSaleOrderEntrySeq = Convert.ToString(item["SaleOrderEntrySeq"]);
                if ((DynamicObject)item["FSMALLID"] != null)
                {
                    subReqOrderEntity.FSMALLID = Convert.ToString(((DynamicObject)item["FSMALLID"])["Number"]);
                }
                if ((DynamicObject)item["FPARENTSMALLID"] != null)
                {
                    subReqOrderEntity.FPARENTSMALLID = Convert.ToString(((DynamicObject)item["FPARENTSMALLID"])["Number"]);
                }
                subReqOrderEntity.FBUSINESSDIVISIONID = Convert.ToString(((DynamicObject)item["FBUSINESSDIVISIONID"])["FDataValue"]);
                subReqOrderEntities.Add(subReqOrderEntity);
            }
            subReqOrder.Details = subReqOrderEntities;
            if (subReqOrder.Details.Count > 0)
            {
                KafkaProducerService kafkaProducer = new KafkaProducerService();
                List<RabbitMQMessage> messages = new List<RabbitMQMessage>
                {
                    new RabbitMQMessage()
                    {
                        Exchange = "subManagement",
                        Routingkey = "AuditSubReqOrder",
                        Keyword = subReqOrder.BillNo,
                        Message = JsonConvertUtils.SerializeObject(subReqOrder)
                    }
                };
                kafkaProducer.AddMessage(this.Context, messages.ToArray());
            }

        }
        private List<SubReqOrderBomChild> GetZCZBom(long bomid)
        {
            List<SubReqOrderBomChild> BomChildrens = new List<SubReqOrderBomChild>();
            string sSql = $@"SELECT t1.FMATERIALID FMaterialId,t2.FNUMBER FMaterialNumber,t3.FNAME FMaterialName,t4.FNAME FUnit,
t1.FNUMERATOR FQty,t3.FSPECIFICATION,t1.FISSUETYPE FROM T_ENG_BOMCHILD t1
LEFT JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
LEFT JOIN T_BD_MATERIAL_L t3 ON t1.FMATERIALID=t3.FMATERIALID
LEFT JOIN T_BD_UNIT_L t4 ON t1.FUNITID=t4.FUNITID
WHERE FID={bomid} AND FISSUETYPE=7";
            var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
            if (datas.Count > 0)
            {
                foreach (var item in datas)
                {
                    BomChildrens.Add(new SubReqOrderBomChild()
                    {
                        FMaterialId = Convert.ToString(item["FMaterialId"]),
                        FMaterialNumber = Convert.ToString(item["FMaterialNumber"]),
                        FMaterialName = Convert.ToString(item["FMaterialName"]),
                        FUnit = Convert.ToString(item["FUnit"]),
                        FQty = Convert.ToDecimal(item["FQty"]),
                        FSpecification = Convert.ToString(item["FSPECIFICATION"]),
                    });
                }
            }
            return BomChildrens;
        }
    }
}
