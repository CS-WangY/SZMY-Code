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
using static Kingdee.Mymooo.Core.StockManagement.WarehousesInventoryQueryEntity.Goods;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.K3.Core.SCM.Args;

namespace Kingdee.Mymooo.ServicePlugIn.PRD_MO
{
    [Description("生产订单审核调用云平台开工"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        List<ApigatewayTaskInfo> requests = new List<ApigatewayTaskInfo>();
        private readonly MymoooBusinessDataService service = new MymoooBusinessDataService();
        private readonly ProductionMoService productionMoService = new ProductionMoService();
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FBillNo");
            e.FieldKeys.Add("FDate");
            e.FieldKeys.Add("FPrdOrgId");
            e.FieldKeys.Add("FPlannerID");
            e.FieldKeys.Add("FInquiryOrder");
            e.FieldKeys.Add("FPENYCustomerID");
            e.FieldKeys.Add("FDrawingRecordId");
            e.FieldKeys.Add("FDescription");
            e.FieldKeys.Add("FIssueMtrl");
            e.FieldKeys.Add("FWorkShopID");
            e.FieldKeys.Add("FUnitId");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FStatus");
            e.FieldKeys.Add("FPlanStartDate");
            e.FieldKeys.Add("FPlanFinishDate");
            e.FieldKeys.Add("FRequestOrgId");
            e.FieldKeys.Add("FBomId");
            e.FieldKeys.Add("FStockInOrgId");
            e.FieldKeys.Add("FSrcBillType");
            e.FieldKeys.Add("FSrcBillEntryId");
            e.FieldKeys.Add("FSaleOrderEntryId");
            e.FieldKeys.Add("FSaleOrderId");
            e.FieldKeys.Add("FSrcBillId");
            e.FieldKeys.Add("FSrcBillNo");
            e.FieldKeys.Add("FSrcBillEntrySeq");
            e.FieldKeys.Add("FMemoItem");
            e.FieldKeys.Add("FSaleOrderNo");
            e.FieldKeys.Add("FBaseUnitId");
            e.FieldKeys.Add("FStockId");
            e.FieldKeys.Add("FPlanConfirmDate");
            e.FieldKeys.Add("FReqSrc");
            e.FieldKeys.Add("FSMALLID");
            e.FieldKeys.Add("FPARENTSMALLID");
            e.FieldKeys.Add("FBUSINESSDIVISIONID");
            e.FieldKeys.Add("FPENYCustomerID");
            e.FieldKeys.Add("FPENYCustomerName");
            e.FieldKeys.Add("FInquiryOrder");
            e.FieldKeys.Add("FInquiryOrderLineNo");
            e.FieldKeys.Add("FDrawingRecordId");
            e.FieldKeys.Add("FPENYPrice");
        }
        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                this.CreateMakeRequest(item);
                this.SendMes(item);
            }
        }
        private void SendMes(DynamicObject data)
        {
            var fid = Convert.ToInt64(data["Id"]);
            //如果是华东五部7401803
            //long[] OutStockList = new long[] { 7401803 };
            //if (!OutStockList.Contains(Convert.ToInt64(data["PrdOrgId_Id"])))
            //{
            //    return;
            //}

            var linkEntity = this.BusinessInfo.GetForm().LinkSet.LinkEntitys[0];//关联实体
            var parentKey = linkEntity.ParentEntityKey; //关联主实体key（也叫父实体key）
            var parentEntity = this.BusinessInfo.GetEntity(parentKey);//关联主实体
            DynamicObjectCollection dynObjs;
            if (parentEntity is HeadEntity) //如果关联主实体是单据头
            {
                dynObjs = new DynamicObjectCollection(data.DynamicObjectType);
                dynObjs.Add(data);
            }
            else
            {
                dynObjs = parentEntity.DynamicProperty.GetValue(data) as DynamicObjectCollection;
            }
            //循环关联数据包，得到上游单据内码集合
            List<object> lstBillId = new List<object>();
            foreach (var entityObj in dynObjs)
            {
                var linkObjs = entityObj[linkEntity.Key] as DynamicObjectCollection;  //关联数据包
                foreach (var linkObj in linkObjs)
                {
                    var sBillId = linkObj["SBillId"];//上游单据内码
                    if (!lstBillId.Contains(sBillId))
                    {
                        lstBillId.Add(sBillId);
                    }
                }
            }

            if (lstBillId.Count > 0)
            {
                var sSql = "SELECT FISSENDCNCMES FROM dbo.T_PLN_PLANORDER WHERE FID=" + lstBillId.First();
                var ismes = DBUtils.ExecuteScalar<bool>(this.Context, sSql, false);
                if (!ismes)
                {
                    return;
                }
            }
            else
            {
                foreach (var item in (DynamicObjectCollection)data["TreeEntity"])
                {
                    var materialgroupid = Convert.ToInt64(((DynamicObject)item["MaterialId"])["MaterialGroup_Id"]);
                    string sSql = $"SELECT * FROM dbo.T_BD_MATERIALGROUP WHERE FID={materialgroupid} AND FIsSendMES=1";
                    var materialgroup = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                    if (materialgroup.Count <= 0)
                    {
                        return;
                    }
                }
            }

            PrOrMoBill planorderbill = new PrOrMoBill();
            planorderbill.Id = fid;
            planorderbill.BillNo = Convert.ToString(data["BillNo"]);
            planorderbill.BillType = Convert.ToString(((DynamicObject)data["BillType"])["Name"]);
            planorderbill.Date = Convert.ToString(data["Date"]);
            planorderbill.DocumentStatus = Convert.ToString(data["DocumentStatus"]);
            planorderbill.PrdOrgId = Convert.ToString(((DynamicObject)data["PrdOrgId"])["Name"]);
            planorderbill.Description = Convert.ToString(data["Description"]);
            planorderbill.IssueMtrl = Convert.ToString(data["IssueMtrl"]);

            List<PrOrMoBillEntity> prOrMoBillEntities = new List<PrOrMoBillEntity>();
            foreach (var item in (DynamicObjectCollection)data["TreeEntity"])
            {
                PrOrMoBillEntity billEntity = new PrOrMoBillEntity();
                billEntity.EntryId = Convert.ToInt64(item["Id"]);
                billEntity.Seq = Convert.ToInt32(item["Seq"]);


                billEntity.MaterialId = Convert.ToString(((DynamicObject)item["MaterialId"])["Number"]);
                billEntity.MaterialName = Convert.ToString(((DynamicObject)item["MaterialId"])["Name"]);
                billEntity.Specification = Convert.ToString(((DynamicObject)item["MaterialId"])["Specification"]);
                billEntity.WorkShopID = Convert.ToString(((DynamicObject)item["WorkShopID"])["Number"]);
                billEntity.UnitId = Convert.ToString(((DynamicObject)item["UnitId"])["Number"]);
                billEntity.Qty = Convert.ToDecimal(item["Qty"]);
                billEntity.Status = Convert.ToString(item["Status"]);
                billEntity.PlanStartDate = Convert.ToDateTime(item["PlanStartDate"]);
                billEntity.PlanFinishDate = Convert.ToDateTime(item["PlanFinishDate"]);
                billEntity.RequestOrgId = Convert.ToString(((DynamicObject)item["RequestOrgId"])["Number"]);
                billEntity.BomId = Convert.ToString(((DynamicObject)item["BomId"])["Number"]);
                billEntity.StockInOrgId = Convert.ToString(((DynamicObject)item["StockInOrgId"])["Number"]);
                billEntity.SrcBillType = Convert.ToString(item["SrcBillType"]);
                billEntity.SrcBillEntryId = Convert.ToInt64(item["SrcBillEntryId"]);
                billEntity.SaleOrderEntryId = Convert.ToInt64(item["SaleOrderEntryId"]);
                billEntity.SaleOrderId = Convert.ToInt64(item["SaleOrderId"]);
                billEntity.SrcBillId = Convert.ToInt64(item["SrcBillId"]);
                billEntity.SrcBillNo = Convert.ToString(item["SrcBillNo"]);
                billEntity.SrcBillEntrySeq = Convert.ToInt64(item["SrcBillEntrySeq"]);
                billEntity.MemoItem = Convert.ToString(item["Memo"]);
                billEntity.SaleOrderNo = Convert.ToString(item["SaleOrderNo"]);
                billEntity.BaseUnitId = Convert.ToString(((DynamicObject)item["BaseUnitId"])["Number"]);
                //billEntity.StockId = Convert.ToString(((DynamicObject)item["StockId"])["Number"]);
                billEntity.PlanConfirmDate = Convert.ToDateTime(item["PlanConfirmDate"]);
                billEntity.ReqSrc = Convert.ToString(item["ReqSrc"]);
                if ((DynamicObject)item["FSMALLID"] != null)
                {
                    billEntity.SMALLID = Convert.ToString(((DynamicObject)item["FSMALLID"])["Number"]);
                }
                if ((DynamicObject)item["FPARENTSMALLID"] != null)
                {
                    billEntity.PARENTSMALLID = Convert.ToString(((DynamicObject)item["FPARENTSMALLID"])["Number"]);
                }
                billEntity.BUSINESSDIVISIONID = Convert.ToString(((DynamicObject)item["FBUSINESSDIVISIONID"])["FDataValue"]);
                //billEntity.PENYCustomerID = Convert.ToString(((DynamicObject)item["FPENYCUSTOMERID"])["Number"]);
                //billEntity.PENYCustomerName = Convert.ToString(((DynamicObject)item["FPENYCUSTOMERID"])["Name"]);
                billEntity.InquiryOrder = Convert.ToString(item["FInquiryOrder"]);
                billEntity.InquiryOrderLineNo = Convert.ToString(item["FInquiryOrderLineNo"]);
                billEntity.DrawingRecordId = Convert.ToString(item["FDrawingRecordId"]);
                billEntity.PENYPrice = Convert.ToDecimal(item["FPENYPrice"]);
                prOrMoBillEntities.Add(billEntity);
            }
            planorderbill.Entity = prOrMoBillEntities;

            KafkaProducerService kafkaProducer = new KafkaProducerService();
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>
                {
                    new RabbitMQMessage()
                    {
                        Exchange = "moManagement",
                        Routingkey = "PRD_MO",
                        Keyword = planorderbill.BillNo,
                        Message = JsonConvertUtils.SerializeObject(planorderbill)
                    }
                };
            kafkaProducer.AddMessage(this.Context, messages.ToArray());
        }

        private void CreateMakeRequest(DynamicObject data)
        {
            MakeRequest request = new MakeRequest();
            request.MakeNo = data["BillNo"].ToString();
            request.Date = Convert.ToDateTime(data["Date"]);
            var org = data["PrdOrgId"] as DynamicObject;
            request.PrdOrgCode = Convert.ToString(org["Number"]);
            request.PrdOrgName = Convert.ToString(org["Name"]);
            var planner = data["PlannerID"] as DynamicObject;
            if (planner != null)
            {
                request.PlannerCode = Convert.ToString(planner["Number"]);
                request.PlannerName = Convert.ToString(planner["Name"]);
            }

            request.Details = productionMoService.SendMakeForBill(this.Context, Convert.ToInt64(data["Id"]));
            ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
            {
                Url = "RabbitMQ/SendMessage?rabbitCode=WorkOrder_platformAdmin_send_",
                Message = JsonConvertUtils.SerializeObject(request)
            };

            taskInfo.Id = service.AddRabbitMqMeaage(this.Context, "Apigateway", request.MakeNo, JsonConvertUtils.SerializeObject(taskInfo)).Data;
            requests.Add(taskInfo);
        }

        //public NonStandardRequest SendApi(string url, object pairs)
        //{
        //    return JsonConvertUtils.DeserializeObject<NonStandardRequest>(Kingdee.Mymooo.Core.Utils.WebApiServiceUtils.HttpPost(url, pairs));
        //}

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            Task.Factory.StartNew(() =>
            {
                foreach (var apigateway in requests)
                {
                    var result = ApigatewayUtils.InvokePostRabbitService(apigateway.Url, apigateway.Message);
                    var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
                    if (response.IsSuccess)
                    {
                        service.UpdateRabbitMqMeaage(this.Context, apigateway.Id, result, true);
                    }
                }
            });
        }
    }


}

