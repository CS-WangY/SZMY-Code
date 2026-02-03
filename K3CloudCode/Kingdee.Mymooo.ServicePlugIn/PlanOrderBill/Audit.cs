using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.BusinessFlow.ServiceArgs;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System.Security.Cryptography;
using Kingdee.K3.FIN.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Newtonsoft.Json.Linq;
using Kingdee.K3.Core.SCM.Mobile;
using System.Web.UI;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    [Description("计划订单审核插件-发送MES"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FSupplyTargetOrgId");//供货组织
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FSupplyMaterialId");
            e.FieldKeys.Add("FReleaseBillType");
            e.FieldKeys.Add("FYieldRate");
            e.FieldKeys.Add("FSugQty");
            e.FieldKeys.Add("FPlanStartDate");
            e.FieldKeys.Add("FPlanFinishDate");
            e.FieldKeys.Add("FFirmQty");
            e.FieldKeys.Add("FDemandQty");
            e.FieldKeys.Add("FOrderQty");
            e.FieldKeys.Add("FDemandDate");
            e.FieldKeys.Add("FDataSource");
            e.FieldKeys.Add("FSaleOrderNo");
            e.FieldKeys.Add("FSaleOrderEntrySeq");
            e.FieldKeys.Add("FDescription");
            e.FieldKeys.Add("FDemandType");
            e.FieldKeys.Add("FSaleOrderId");
            e.FieldKeys.Add("FSaleOrderEntryId");
            e.FieldKeys.Add("FEntrustOrgId");
            e.FieldKeys.Add("FSMALLID");
            e.FieldKeys.Add("FPARENTSMALLID");
            e.FieldKeys.Add("FBUSINESSDIVISIONID");
            e.FieldKeys.Add("FPENYSalDatetime");
            e.FieldKeys.Add("FIsSendMes");
            e.FieldKeys.Add("FDrawingRecordId");
            e.FieldKeys.Add("FInquiryOrder");
            e.FieldKeys.Add("FPENYCustomerID");
            e.FieldKeys.Add("FPENYPRICE");
            e.FieldKeys.Add("FPENYAMOUNT");
            e.FieldKeys.Add("FCustPurchaseNo");
            e.FieldKeys.Add("FSALCREATORNAME");
            e.FieldKeys.Add("FIsSendCNCMES");
            e.FieldKeys.Add("FPlanTenderType");
            e.FieldKeys.Add("FMachineName");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                //如果是华东五部7401803FDemandOrgId//特定小类发送CNCMES
                //long[] OutStockList = new long[] { 7401803 };
                //if (!OutStockList.Contains(Convert.ToInt64(item["DemandOrgId_Id"])))
                //{
                //    return;
                //}
                var sendmes = Convert.ToBoolean(item["FIsSendCNCMES"]);
                if (!sendmes)
                {
                    continue;
                }
                var fid = Convert.ToInt64(item["Id"]);
                var ids = new object[] { fid };
                var saleentryid = Convert.ToInt64(item["SaleOrderEntryId"]);

                string FCustMaterialNo = "";
                string FCustItemNo = "";
                string FCustItemName = "";
                string FCustOrderDetailId = "";
                string ProjectNo = "";
                var sSql = $"SELECT FCustMaterialNo,FCustItemNo,FCustItemName,FCustOrderDetailId,FProjectNo FROM dbo.T_SAL_ORDERENTRY WHERE FENTRYID={saleentryid}";
                var custdata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                foreach (var citem in custdata)
                {
                    FCustMaterialNo = Convert.ToString(citem["FCustMaterialNo"]);
                    FCustItemNo = Convert.ToString(citem["FCustItemNo"]);
                    FCustItemName = Convert.ToString(citem["FCustItemName"]);
                    FCustOrderDetailId = Convert.ToString(citem["FCustOrderDetailId"]);
                    ProjectNo = Convert.ToString(citem["FProjectNo"]);
                }

                PlanorderBillEntity planorderbill = new PlanorderBillEntity();

                planorderbill.BillId = fid;
                planorderbill.BillTypeID = Convert.ToString(((DynamicObject)item["FBillTypeID"])["Name"]);
                planorderbill.BillNo = Convert.ToString(item["BillNo"]);
                planorderbill.SupplyOrgNumber = Convert.ToString(((DynamicObject)item["SupplyOrgId"])["Number"]);
                planorderbill.SupplyOrgName = Convert.ToString(((DynamicObject)item["SupplyOrgId"])["Name"]);
                planorderbill.DemandOrgNumber = Convert.ToString(((DynamicObject)item["DemandOrgId"])["Number"]);
                planorderbill.DemandOrgName = Convert.ToString(((DynamicObject)item["DemandOrgId"])["Name"]);
                planorderbill.MaterialNumber = Convert.ToString(((DynamicObject)item["SupplyMaterialId"])["Number"]);
                planorderbill.MaterialName = Convert.ToString(((DynamicObject)item["SupplyMaterialId"])["Name"]);
                planorderbill.Specification = Convert.ToString(((DynamicObject)item["SupplyMaterialId"])["Specification"]);
                //planorderbill.//AuxPropId = Convert.ToString(((DynamicObject)item["SupplyMaterialId"])["AuxPropId"]);
                planorderbill.ReleaseType = Convert.ToString(item["ReleaseType"]);
                planorderbill.ReleaseBillType = Convert.ToString(((DynamicObject)((DynamicObject)((DynamicObjectCollection)item["PLSubHead"]).FirstOrDefault())["ReleaseBillType"])["Name"]);
                planorderbill.YieldRate = Convert.ToDecimal(((DynamicObject)((DynamicObjectCollection)item["PLSubHead"]).FirstOrDefault())["YieldRate"]);
                planorderbill.SupplyMaterialId = Convert.ToString(item["SupplyMaterialId_Id"]);
                //planorderbill.//BomId = Convert.ToString(item["BomId"]);
                planorderbill.UnitId = Convert.ToString(((DynamicObject)item["BaseUnitId"])?["Name"]);
                planorderbill.SugQty = Convert.ToString(item["SugQty"]);
                planorderbill.PlanStartDate = Convert.ToDateTime(item["PlanStartDate"]);
                planorderbill.PlanFinishDate = Convert.ToDateTime(item["PlanFinishDate"]);
                //planorderbill.//MrpNote = Convert.ToString(item["MrpNote"]);
                planorderbill.PrdDeptId = Convert.ToString(((DynamicObject)item["PrdDeptId"])?["Number"]);
                //planorderbill.//PlanerId = Convert.ToString(item["PlanerId"]);
                planorderbill.InStockOrgId = Convert.ToString(item["InStockOrgId_Id"]);
                planorderbill.DemandQty = Convert.ToString(item["DemandQty"]);
                planorderbill.OrderQty = Convert.ToString(item["FirmQty"]);
                planorderbill.DemandDate = Convert.ToDateTime(item["DemandDate"]);
                planorderbill.DataSource = Convert.ToString(item["DataSource"]);
                planorderbill.DocumentStatus = Convert.ToString(item["DocumentStatus"]);
                planorderbill.ReleaseStatus = Convert.ToString(item["ReleaseStatus"]);
                planorderbill.SaleOrderNo = Convert.ToString(item["SaleOrderNo"]);
                planorderbill.SaleOrderEntrySeq = Convert.ToString(item["SaleOrderEntrySeq"]);
                planorderbill.Description = Convert.ToString(item["Description"]);
                planorderbill.ApproveDate = Convert.ToDateTime(item["ApproveDate"]);
                planorderbill.ApproverId = Convert.ToString(((DynamicObject)item["ApproverId"])?["Name"]);
                planorderbill.DemandType = Convert.ToString(item["DemandType"]);
                planorderbill.SaleOrderId = Convert.ToString(item["SaleOrderId"]);
                planorderbill.SaleOrderEntryId = Convert.ToString(item["SaleOrderEntryId"]);
                planorderbill.EntrustOrgId = Convert.ToString(item["EntrustOrgId_Id"]);
                planorderbill.SMALLID = Convert.ToString(((DynamicObject)item["FSMALLID"])?["Number"]);
                planorderbill.PARENTSMALLID = Convert.ToString(((DynamicObject)item["FPARENTSMALLID"])?["Number"]);
                planorderbill.BUSINESSDIVISIONID = Convert.ToString(((DynamicObject)item["FBUSINESSDIVISIONID"])?["FDataValue"]);
                planorderbill.PENYSalDatetime = Convert.ToString(item["FPENYSalDatetime"]);
                planorderbill.IsSendMes = Convert.ToString(item["FIsSendMes"]);
                planorderbill.FDrawingRecordId = Convert.ToInt64(item["FDrawingRecordId"]);
                planorderbill.FPENYCustomerNumber = Convert.ToString(((DynamicObject)item["FPENYCustomerID"])?["Number"]);
                planorderbill.FPENYCustomerName = Convert.ToString(((DynamicObject)item["FPENYCustomerID"])?["Name"]);
                planorderbill.FCustMaterialNo = FCustMaterialNo;
                planorderbill.FCustItemNo = FCustItemNo;
                planorderbill.FCustItemName = FCustItemName;
                planorderbill.FPENYPRICE = Convert.ToDecimal(item["FPENYPRICE"]);
                planorderbill.FPENYAMOUNT = Convert.ToDecimal(item["FPENYAMOUNT"]);
                planorderbill.CustPurchaseNo = Convert.ToString(item["FCustPurchaseNo"]);
                planorderbill.SalCreatorName = Convert.ToString(item["FSALCREATORNAME"]);
                planorderbill.CustOrderDetailId = FCustOrderDetailId;
                planorderbill.ProjectNo = ProjectNo;

                planorderbill.PlanTenderType = Convert.ToString(item["FPlanTenderType"]);
                planorderbill.MachineName = Convert.ToString(item["FMachineName"]);

                planorderbill.LENGTH = Convert.ToDecimal(((DynamicObjectCollection)((DynamicObject)item["SupplyMaterialId"])["MaterialBase"]).FirstOrDefault()["LENGTH"]);
                planorderbill.WIDTH = Convert.ToDecimal(((DynamicObjectCollection)((DynamicObject)item["SupplyMaterialId"])["MaterialBase"]).FirstOrDefault()["Width"]);
                planorderbill.HEIGHT = Convert.ToDecimal(((DynamicObjectCollection)((DynamicObject)item["SupplyMaterialId"])["MaterialBase"]).FirstOrDefault()["Height"]);

                //                string FVolume = "";
                //                string FWeight = "";
                //                string FTextures = "";
                //                sSql = $@"SELECT t3.FNAME AS FVolume,t4.FNAME AS FWeight,t1.FTEXTURES FROM dbo.T_BD_MATERIAL t1
                //LEFT JOIN T_BD_MATERIALBASE t2 ON t1.FMATERIALID=t2.FMATERIALID
                //LEFT JOIN dbo.T_BD_UNIT_L t3 ON t2.FVOLUMEUNITID=t3.FUNITID
                //LEFT JOIN dbo.T_BD_UNIT_L t4 ON t2.FWEIGHTUNITID=t4.FUNITID
                //WHERE t1.FMATERIALID={((DynamicObject)item["SupplyMaterialId"])["Id"]}";
                //                var drawdata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                //                foreach (var ditem in drawdata)
                //                {
                //                    FVolume = Convert.ToString(ditem["FVolume"]);
                //                    FWeight = Convert.ToString(ditem["FWeight"]);
                //                    FTextures = Convert.ToString(ditem["FTEXTURES"]);
                //                }

                if (!Convert.ToString(item["FINQUIRYORDER"]).IsNullOrEmptyOrWhiteSpace())
                {
                    var Inquiryorderid = Convert.ToString(item["FINQUIRYORDER"]);
                    var Drawingrecordid = Convert.ToString(((DynamicObject)item["SupplyMaterialId"])["Number"]);
                    var pairs = new
                    {
                        InquiryOrder = Inquiryorderid,
                        DrawingNumber = Drawingrecordid
                    };
                    List<object> list = new List<object>();
                    list.Add(pairs);
                    var response = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/PDM/GetPDMImageinfo", JsonConvertUtils.SerializeObject(list));
                    //this.View.ShowMessage($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/PDM/GetPDMImageinfo" + ":" + response);
                    JObject jo = (JObject)JToken.Parse(response);
                    var data = jo["data"].ToString();
                    List<QuoteRequestDtocs> listdw = JsonConvertUtils.DeserializeObject<List<QuoteRequestDtocs>>(data);
                    foreach (var pair in listdw)
                    {
                        planorderbill.FDrawingRecordId = pair.DrawingRecordId;
                        //planorderbill.LENGTH = (decimal)pair.Long;
                        //planorderbill.WIDTH = (decimal)pair.Width;
                        //planorderbill.HEIGHT = (decimal)pair.Height;
                        planorderbill.PENYSurfaceArea = (decimal)pair.SurfaceArea;
                        planorderbill.PENYWeight = (decimal)pair.Weight;
                        planorderbill.VOLUME = (decimal)pair.Volume;

                        planorderbill.VOLUMEUNITNAME = Convert.ToString(pair.Volume);
                        planorderbill.WEIGHTUNITNAME = Convert.ToString(pair.Weight);
                        planorderbill.Textures = pair.MaterialName;

                        planorderbill.PartTypeName = pair.PartTypeName;
                        planorderbill.DrawingFileUrl = pair.DrawingFileUrl;

                        planorderbill.SurfaceTreatment = pair.SurfaceTreatment;
                        planorderbill.BlankType = pair.BlankType;
                        planorderbill.HeatTreatment = pair.HeatTreatment;
                        planorderbill.FinalCost = pair.FinalCost;
                        planorderbill.StlFileUrl = pair.StlFileUrl;
                        planorderbill.ThumbnailFileUrl = pair.ThumbnailFileUrl;
                    }
                }

                KafkaProducerService kafkaProducer = new KafkaProducerService();
                List<RabbitMQMessage> messages = new List<RabbitMQMessage>
                {
                    new RabbitMQMessage()
                    {
                        Exchange = "plnManagement",
                        Routingkey = "Planorder",
                        Keyword = planorderbill.BillNo,
                        Message = JsonConvertUtils.SerializeObject(planorderbill)
                    }
                };
                kafkaProducer.AddMessage(this.Context, messages.ToArray());
            }
        }



        /// <summary>
        /// 立即执行MQ
        /// </summary>
        /// <param name="e"></param>
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            Task.Factory.StartNew(() =>
            {
                //晚5个s,让事务可以提交成功后在发送消息
                System.Threading.Thread.Sleep(5000);
                ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
            });
        }
    }
}
