using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.BusinessEntity.ThirdSystem.MessageLog;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.BaseManagement;
using Kingdee.Mymooo.Contracts.ProductionManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.ApigatewayConfiguration.EnterpriseWeChat;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.ProductionManagement.Dispatch;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static Kingdee.Mymooo.Core.ProductionManagement.MesOneClickReturnMaterialRequest;

namespace Kingdee.Mymooo.App.Core.ProductionManagement
{
    public class ProductionMoService : IProductionMoService
    {
        public IOperationResult CancelMakeDispatch(Context ctx, MakeDispatchRequest request)
        {
            IOperationResult operationResult = new OperationResult();
            UserService userService = new UserService();
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            request.UserCode = userService.GetUserWxCode(ctx, ctx.UserId);
            ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
            {
                Url = "RabbitMQ/SendMessage?rabbitCode=WorkOrder_cancel_",
                Message = JsonConvertUtils.SerializeObject(request)
            };
            using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
            {
                DBUtils.Execute(ctx, "update T_PRD_MOENTRY set FISDISPATCHMES = '3' where FENTRYID = @FENTRYID", new SqlParam("@FENTRYID", KDDbType.Int64, request.EntryId));
                taskInfo.Id = service.AddRabbitMqMeaage(ctx, "Apigateway", request.Key, JsonConvertUtils.SerializeObject(taskInfo)).Data;
                cope.Complete();
            }
            Task.Factory.StartNew(() =>
            {
                var result = ApigatewayUtils.InvokePostRabbitService(taskInfo.Url, taskInfo.Message);
                var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
                if (response.IsSuccess)
                {
                    service.UpdateRabbitMqMeaage(ctx, taskInfo.Id, result, true);
                }
            });
            operationResult.IsSuccess = true;
            return operationResult;
        }

        public IOperationResult CancelMakeDispatchForIds(Context ctx, List<long> headIds)
        {
            throw new NotImplementedException();
        }

        public IOperationResult CancelMakeDispatchs(Context ctx, List<long> entryIds)
        {
            throw new NotImplementedException();
        }

        public IOperationResult SendMakeDispatchs(Context ctx, SendMakeDispatchRequest request)
        {
            IOperationResult operationResult = new OperationResult();
            var sql = @"/*dialect*/ select mo.FPRDORGID,mo.FBILLNO makeNo,e.FSEQ makeSeq,e.FSALEORDERNO saleOrderNo,d.FNUMBER worksNo,m.FNUMBER dwgNo,ml.FNAME dwgName,m.FPRODUCTID productId,e.FQTY qty,e.FPlanFinishDate deadLine,isnull(b.FNUMBER, '') FDwgVer,d.FDEPTID
,isnull(mg.FNUMBER, '') partTypeCode,isnull(el.FMEMO, '') remark,e.FENTRYID,isnull(e.FPENYPrice,0) FTAXPRICE,d.FENABLEMES,b.FAUTOCRAFT
,o.FDATE SalesDate,isnull(c.FNUMBER,'') CustomerCode,isnull(cl.FNAME,'') CustomerName ,oe.FENTRYID OrderEntryId,oe.FSEQ OrderEntrySeq
,e.FDRAWINGRECORDID DrawingRecordId,e.FINQUIRYORDER InquiryOrder,e.FINQUIRYORDERLINENO InquiryOrderLineNo,o.FCUSTPURCHASENO custPurchaseNo
,CASE ea.FREQSRC WHEN 1 THEN 0 ELSE 1 END isManual,oe.FCustItemNo,oe.FCustItemName,oe.FCustMaterialNo
from T_PRD_MO mo
	inner join T_PRD_MOENTRY e on mo.FID = e.FID
	inner join T_PRD_MOENTRY_A ea on e.FENTRYID = ea.FENTRYID 
	left join T_PRD_MOENTRY_L el on e.FENTRYID = el.FENTRYID and el.FLOCALEID = 2052
	inner join T_BD_DEPARTMENT d on e.FWORKSHOPID = d.FDEPTID 
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	inner join T_BD_MATERIAL_L ml on e.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	left join T_ENG_BOM b on e.FBOMID = b.FID 
	left join T_BD_MATERIALGROUP mg on m.FMATERIALGROUP = mg.FID
	left join T_SAL_ORDERENTRY oe on e.FSALEORDERENTRYID = oe.FENTRYID
	left join T_SAL_ORDER o on oe.FID = o.FID
	left join T_BD_CUSTOMER c on o.FCUSTID = c.FCUSTID
	left join T_BD_CUSTOMER_L cl on c.FCUSTID = cl.FCUSTID and cl.FLOCALEID = 2052
where mo.FPRDORGID = @FPRDORGID and mo.FDOCUMENTSTATUS = 'C' and mo.FCANCELSTATUS = 'A' and ea.FSTATUS < '6' and mo.FDATE >= @FStartDate ";
            List<SqlParam> sqlParams = new List<SqlParam>
            {
                new SqlParam("@FPRDORGID", KDDbType.Int64, request.OrgId),
                new SqlParam("@FStartDate", KDDbType.Date, request.StartDate)
            };
            if (request.EndDate != null)
            {
                sql += " and mo.FDATE < @FEndDate";
                sqlParams.Add(new SqlParam("@FEndDate", KDDbType.Date, request.EndDate.Value.AddDays(1)));
            }
            var datas = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: sqlParams.ToArray());
            List<ApigatewayTaskInfo> taskInfos = new List<ApigatewayTaskInfo>();
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            foreach (var data in datas)
            {
                MakeDispatchRequest makeDispatchRequest = CreateMakeDispatchRequest(ctx, data);
                ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
                {
                    Url = "RabbitMQ/SendMessage?rabbitCode=WorkOrder_wuxi_send_",
                    Message = JsonConvertUtils.SerializeObject(makeDispatchRequest)
                };
                //taskInfo.Id = service.AddRabbitMqMeaage(ctx, "Apigateway", makeDispatchRequest.Key, JsonConvertUtils.SerializeObject(taskInfo)).Data;
                taskInfos.Add(taskInfo);
            }
            Task.Factory.StartNew(() =>
            {
                foreach (var taskInfo in taskInfos)
                {
                    var result = ApigatewayUtils.InvokePostRabbitService(taskInfo.Url, taskInfo.Message);
                    var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
                    if (response.IsSuccess)
                    {
                        service.UpdateRabbitMqMeaage(ctx, taskInfo.Id, result, true);
                    }
                }
            });
            operationResult.IsSuccess = true;
            return operationResult;
        }
        /// <summary>
        /// 审核发送MES
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<MakeDispatchRequest> SendMakeForBill(Context ctx, long id)
        {
            var sql = @"/*dialect*/ select mo.FPRDORGID,e.FENTRYID makeEid,mo.FBILLNO makeNo,e.FSEQ makeSeq,e.FSALEORDERNO saleOrderNo,isnull(d.FNUMBER,'') worksNo,m.FNUMBER dwgNo,ml.FNAME dwgName,m.FPRODUCTID productId,e.FQTY qty,e.FPlanFinishDate deadLine,b.FNUMBER FDwgVer,d.FDEPTID
,isnull(mg.FNUMBER, '') partTypeCode,el.FMEMO remark,e.FENTRYID,isnull(e.FPENYPrice,0) FTAXPRICE,isnull(d.FENABLEMES, '0') FENABLEMES,b.FAUTOCRAFT
,o.FDATE SalesDate,isnull(c.FNUMBER,'') CustomerCode,isnull(cl.FNAME,'') CustomerName,oe.FENTRYID OrderEntryId,oe.FSEQ OrderEntrySeq
,e.FDRAWINGRECORDID DrawingRecordId,e.FINQUIRYORDER InquiryOrder,e.FINQUIRYORDERLINENO InquiryOrderLineNo,o.FCUSTPURCHASENO custPurchaseNo
,CASE a.FREQSRC WHEN 1 THEN 0 ELSE 1 END isManual,oe.FCustItemNo,oe.FCustItemName,oe.FCustMaterialNo
from T_PRD_MO mo
	inner join T_PRD_MOENTRY e on mo.FID = e.FID
    LEFT JOIN T_PRD_MOENTRY_A a on e.FENTRYID = a.FENTRYID
	left join T_PRD_MOENTRY_L el on e.FENTRYID = el.FENTRYID and el.FLOCALEID = 2052
	left join T_BD_DEPARTMENT d on e.FWORKSHOPID = d.FDEPTID
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	inner join T_BD_MATERIAL_L ml on e.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	inner join T_ENG_BOM b on e.FBOMID = b.FID
	left join T_BD_MATERIALGROUP mg on m.FMATERIALGROUP = mg.FID
	left join T_SAL_ORDERENTRY oe on e.FSALEORDERENTRYID = oe.FENTRYID
	left join T_SAL_ORDER o on oe.FID = o.FID
	left join T_BD_CUSTOMER c on o.FCUSTID = c.FCUSTID
	left join T_BD_CUSTOMER_L cl on c.FCUSTID = cl.FCUSTID and cl.FLOCALEID = 2052
where mo.FID = @FID";
            var datas = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
            List<MakeDispatchRequest> requests = new List<MakeDispatchRequest>();

            foreach (var data in datas)
            {
                requests.Add(CreateMakeDispatchRequest(ctx, data));
            }
            if (requests.Count > 0)
            {
                sql = @"/*dialect*/ select e.FENTRYID,isnull(sem.FNUMBER,'') MtlCode,isnull(sem.FTEXTURES,'') MtlType,isnull(em.FSENDMES, '0') IsSendMes
from T_PRD_MO mo
	inner join T_PRD_MOENTRY e on mo.FID = e.FID
	left join T_PRD_PPBOMENTRY em on e.FENTRYID = em.FMOENTRYID 
	left join T_BD_MATERIAL sem on em.FMATERIALID = sem.FMATERIALID
where e.FID = @FID";
                datas = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
                var groups = datas.GroupBy(p => Convert.ToInt64(p["FENTRYID"])).ToList();
                foreach (var group in groups)
                {
                    var request = requests.First(p => p.EntryId == group.Key);
                    request.MaterialDetails = group.ToList();
                }
            }
            return requests;
        }

        public IOperationResult SendMakeDispatch(Context ctx, long entryId)
        {
            IOperationResult operationResult = new OperationResult();
            var sql = @"/*dialect*/ select distinct mo.FPRDORGID,mo.FBILLNO makeNo,e.FSEQ makeSeq,e.FSALEORDERNO saleOrderNo,d.FNUMBER worksNo,m.FNUMBER dwgNo,ml.FNAME dwgName,m.FPRODUCTID productId,e.FQTY qty,e.FPlanFinishDate deadLine,b.FNUMBER FDwgVer,d.FDEPTID
,isnull(mg.FNUMBER, '') partTypeCode,el.FMEMO remark,isnull(sem.FNUMBER,'') MtlCode,isnull(sem.FTEXTURES,'') MtlType,e.FENTRYID,isnull(e.FPENYPrice,0) FTAXPRICE,d.FENABLEMES,b.FAUTOCRAFT
,o.FDATE SalesDate,isnull(c.FNUMBER,'') CustomerCode,isnull(cl.FNAME,'') CustomerName ,oe.FENTRYID OrderEntryId,oe.FSEQ OrderEntrySeq
,e.FDRAWINGRECORDID DrawingRecordId,e.FINQUIRYORDER InquiryOrder,e.FINQUIRYORDERLINENO InquiryOrderLineNo,o.FCUSTPURCHASENO custPurchaseNo
,CASE ea.FREQSRC WHEN 1 THEN 0 ELSE 1 END isManual,oe.FCustItemNo,oe.FCustItemName,oe.FCustMaterialNo
from T_PRD_MO mo
	inner join T_PRD_MOENTRY e on mo.FID = e.FID
	inner join T_PRD_MOENTRY_A ea on e.FENTRYID = ea.FENTRYID 
	inner join T_PRD_MOENTRY_L el on e.FENTRYID = el.FENTRYID and el.FLOCALEID = 2052
	inner join T_BD_DEPARTMENT d on e.FWORKSHOPID = d.FDEPTID and d.FENABLEMES = '1'
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	inner join T_BD_MATERIAL_L ml on e.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	inner join T_ENG_BOM b on e.FBOMID = b.FID and b.FAUTOCRAFT = '1'
	left join T_PRD_PPBOMENTRY em on e.FENTRYID = em.FMOENTRYID and em.FSENDMES = '1'
	left join T_BD_MATERIAL sem on em.FMATERIALID = sem.FMATERIALID
	left join T_BD_MATERIALGROUP mg on m.FMATERIALGROUP = mg.FID
	left join T_SAL_ORDERENTRY oe on e.FSALEORDERENTRYID = oe.FENTRYID
	left join T_SAL_ORDER o on oe.FID = o.FID
	left join T_BD_CUSTOMER c on o.FCUSTID = c.FCUSTID
	left join T_BD_CUSTOMER_L cl on c.FCUSTID = cl.FCUSTID and cl.FLOCALEID = 2052
where e.FENTRYID = @FENTRYID";
            var datas = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FENTRYID", KDDbType.Int64, entryId));

            MymoooBusinessDataService service = new MymoooBusinessDataService();
            MakeDispatchRequest makeDispatchRequest = CreateMakeDispatchRequest(ctx, datas[0]);
            ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
            {
                Url = "RabbitMQ/SendMessage?rabbitCode=WorkOrder_platformAdmin_send_",
                Message = JsonConvertUtils.SerializeObject(makeDispatchRequest)
            };
            using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
            {
                DBUtils.Execute(ctx, "update T_PRD_MOENTRY set FISDISPATCHMES = '1' where FENTRYID = @FENTRYID", new SqlParam("@FENTRYID", KDDbType.Int64, entryId));
                taskInfo.Id = service.AddRabbitMqMeaage(ctx, "Apigateway", makeDispatchRequest.Key, JsonConvertUtils.SerializeObject(taskInfo)).Data;
                cope.Complete();
            }
            Task.Factory.StartNew(() =>
            {
                var result = ApigatewayUtils.InvokePostRabbitService(taskInfo.Url, taskInfo.Message);
                var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
                if (response.IsSuccess)
                {
                    service.UpdateRabbitMqMeaage(ctx, taskInfo.Id, result, true);
                }
            });
            operationResult.IsSuccess = true;
            return operationResult;
        }

        public IOperationResult SendMakeDispatchForIds(Context ctx, List<long> headIds)
        {
            throw new NotImplementedException();
        }

        public IOperationResult SendMakeDispatchs(Context ctx, List<long> entryIds)
        {
            throw new NotImplementedException();
        }

        private MakeDispatchRequest CreateMakeDispatchRequest(Context ctx, DynamicObject data)
        {
            UserService userService = new UserService();
            DateTime? date = null;
            if (!(data["SalesDate"] is DBNull))
            {
                date = Convert.ToDateTime(data["SalesDate"]);
            }
            //价格取不到就取该物料最近一笔订单的含税单价
            var price = Convert.ToDecimal(data["FTAXPRICE"]);
            var qty = Convert.ToInt32(data["qty"]);
            if (price == 0)
            {
                price = GetPrice(ctx, Convert.ToInt64(data["FPRDORGID"]), Convert.ToString(data["dwgNo"]));
            }
            var amount = price * qty;
            return new MakeDispatchRequest()
            {
                SaleOrderNo = Convert.ToString(data["saleOrderNo"]),
                OrderEntryId = Convert.ToInt64(data["OrderEntryId"]),
                OrderEntrySeq = Convert.ToInt32(data["OrderEntrySeq"]),
                MakeNo = Convert.ToString(data["makeNo"]),
                MakeSeq = Convert.ToInt32(data["makeSeq"]),
                WorksNo = Convert.ToString(data["worksNo"]),
                DwgNo = Convert.ToString(data["dwgNo"]),
                DwgName = Convert.ToString(data["dwgName"]),
                DwgVer = Convert.ToString(data["FDwgVer"]),
                SalesDate = date,
                CustomerCode = Convert.ToString(data["CustomerCode"]),
                CustomerName = Convert.ToString(data["CustomerName"]),
                DeadLine = Convert.ToDateTime(data["deadLine"]),
                PartTypeCode = Convert.ToString(data["partTypeCode"]),
                Remark = Convert.ToString(data["remark"]),
                ProductId = Convert.ToInt64(data["productId"]),
                EntryId = Convert.ToInt64(data["FENTRYID"]),
                MtlCode = data.Contains("MtlCode") ? Convert.ToString(data["MtlCode"]) : null,
                MtlType = data.Contains("MtlType") ? Convert.ToString(data["MtlType"]) : null,
                Price = price,
                Qty = qty,
                Amount = amount,
                EnableMes = Convert.ToString(data["FENABLEMES"]) == "1",
                AutoCraft = Convert.ToString(data["FAUTOCRAFT"]) == "1",
                UserCode = userService.GetUserWxCode(ctx, ctx.UserId),
                DrawingRecordId = Convert.ToString(data["DrawingRecordId"]),
                quotationOrderNo = Convert.ToString(data["InquiryOrder"]),
                quotationOrderLineNo = Convert.ToString(data["InquiryOrderLineNo"]),
                CustPurchaseNo = Convert.ToString(data["custPurchaseNo"]),
                isManual = Convert.ToBoolean(Convert.ToInt32(data["isManual"])),
                CustItemNo = Convert.ToString(data["FCustItemNo"]),
                CustItemName = Convert.ToString(data["FCustItemName"]),
                CustMaterialNo = Convert.ToString(data["FCustMaterialNo"])
            };
        }

        public ResponseMessage<dynamic> MesProductionMoStatus(Context ctx, SendMesMakeResponse request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var sql = "select FMessage from RabbitMQScheduledMessage where FKeyword = @FKeyword ";
            var rabbitMessage = DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, new SqlParam("@FKeyword", KDDbType.String, request.Result.Key));
            var apigatewayTask = JsonConvertUtils.DeserializeObject<ApigatewayTaskInfo>(rabbitMessage);
            MakeDispatchRequest make = JsonConvertUtils.DeserializeObject<MakeDispatchRequest>(apigatewayTask.Message);
            if (request.Success)
            {
                //更新成同步完成
                var updateSql = $"update T_PRD_MOENTRY set FISDISPATCHMES = '2' where FENTRYID = @FENTRYID";
                DBUtils.Execute(ctx, updateSql, new SqlParam("@FENTRYID", KDDbType.Int64, make.EntryId));
                SendMessage(make, $"生产订单号:{make.MakeNo} 序号:{make.MakeSeq} 发送mes成功");
            }
            else
            {
                var updateSql = $"update T_PRD_MOENTRY set FISDISPATCHMES = '0' where FENTRYID = @FENTRYID";
                DBUtils.Execute(ctx, updateSql, new SqlParam("@FENTRYID", KDDbType.Int64, make.EntryId));
                SendMessage(make, $"生产订单号:{make.MakeNo} 序号:{make.MakeSeq} 发送mes失败:{request.Message}");
            }
            message.Code = ResponseCode.Success;

            return message;
        }

        public ResponseMessage<dynamic> MesCancelMoStatus(Context ctx, SendMesMakeResponse request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var sql = "select FMessage from RabbitMQScheduledMessage where FKeyword = @FKeyword ";
            var rabbitMessage = DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, new SqlParam("@FKeyword", KDDbType.String, request.Result.Key));
            var apigatewayTask = JsonConvertUtils.DeserializeObject<ApigatewayTaskInfo>(rabbitMessage);
            MakeDispatchRequest make = JsonConvertUtils.DeserializeObject<MakeDispatchRequest>(apigatewayTask.Message);
            if (request.Success)
            {
                //更新成同步完成
                var updateSql = $"update T_PRD_MOENTRY set FISDISPATCHMES = '0' where FENTRYID = @FENTRYID";
                DBUtils.Execute(ctx, updateSql, new SqlParam("@FENTRYID", KDDbType.Int64, make.EntryId));
                SendMessage(make, $"生产订单号:{make.MakeNo} 序号:{make.MakeSeq} 取消mes成功");
            }
            else
            {
                var updateSql = $"update T_PRD_MOENTRY set FISDISPATCHMES = '2' where FENTRYID = @FENTRYID";
                DBUtils.Execute(ctx, updateSql, new SqlParam("@FENTRYID", KDDbType.Int64, make.EntryId));
                SendMessage(make, $"生产订单号:{make.MakeNo} 序号:{make.MakeSeq} 取消mes失败:{request.Message}");
            }
            message.Code = ResponseCode.Success;

            return message;
        }

        private void SendMessage(MakeDispatchRequest make, string content)
        {
            ApigatewayUtils.InvokePostWebService("EnterpriseWeChat/SendTextMessage", JsonConvertUtils.SerializeObject(
                    new SendTextMessageRequest()
                    {
                        touser = make.UserCode,
                        text =
                    new SendTextMessageRequest.Text()
                    {
                        content = content
                    }
                    }));
        }

        public ResponseMessage<dynamic> SendMakeDispatchForBill(Context ctx, MakeRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            request.Details = SendMakeForBill(ctx, request.Id);
            ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
            {
                Url = "RabbitMQ/SendMessage?rabbitCode=WorkOrder_platformAdmin_send_",
                Message = JsonConvertUtils.SerializeObject(request)
            };

            MymoooBusinessDataService service = new MymoooBusinessDataService();
            taskInfo.Id = service.AddRabbitMqMeaage(ctx, "Apigateway", request.MakeNo, JsonConvertUtils.SerializeObject(taskInfo)).Data;
            Task.Factory.StartNew(() =>
            {
                var result = ApigatewayUtils.InvokePostRabbitService(taskInfo.Url, taskInfo.Message);
                var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
                if (response.IsSuccess)
                {
                    service.UpdateRabbitMqMeaage(ctx, taskInfo.Id, result, true);
                }
            });
            message.Code = ResponseCode.Success;
            return message;
        }



        /// <summary>
        /// 取销售含税单价
        /// </summary>
        /// <param name="materialNo"></param>
        /// <returns></returns>
        private decimal GetPrice(Context ctx, long orgId, string materialNo)
        {
            //半成品取成品的价格
            var index = materialNo.LastIndexOf("-W-1-");
            if (index > 0)
            {
                materialNo = materialNo.Substring(0, index);
            }

            //1、先取组织间需求单
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@OrgId", KDDbType.Int64, orgId), new SqlParam("@MaterialNo", KDDbType.String, materialNo) };
            var sql = $@"/*dialect*/select top 1 F_PENY_PRICE from T_PLN_REQUIREMENTORDER t1
                            inner join T_BD_MATERIAL t2 on t1.FMATERIALID=t2.FMATERIALID
                            where FSUPPLYORGID=@OrgId and t2.FNUMBER=@MaterialNo and t1.FDOCUMENTSTATUS='C' and F_PENY_PRICE>0
                            order by t1.FAPPROVEDATE desc ";
            decimal price = DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
            if (price > 0)
            {
                return price;
            }
            //2、取不到则取华东五部的销售订单
            sql = $@"/*dialect*/SELECT TOP 1 TEF.FTAXPRICE FROM T_SAL_ORDER SAL
                        INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
                        INNER JOIN T_SAL_ORDERENTRY_F TEF ON SALDES.FENTRYID=TEF.FENTRYID
                        INNER JOIN T_BD_MATERIAL MAT on SALDES.FMATERIALID=MAT.FMATERIALID
                        WHERE SAL.FSaleOrgId=7401803 and SALDES.FSUPPLYTARGETORGID=@OrgId AND MAT.FNUMBER=@MaterialNo 
                        AND SAL.FDOCUMENTSTATUS='C' AND TEF.FTAXPRICE>0  ORDER BY SAL.FAUDITTIME DESC ";
            return DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
        }

        /// <summary>
        /// Mes一键退料
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="mostatus"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesOneClickReturnMaterial(Context ctx, MesOneClickReturnMaterialRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                //1补料退料
                if (request.OrderId == 1)
                {
                    foreach (var mo in request.MoDetails)
                    {
                        //判断生产订单状态
                        var sql = $@"/*dialect*/select FSTATUS from T_PRD_MOENTRY_A where FENTRYID = {mo.EntryId} ";
                        var status = DBUtils.ExecuteScalar<int>(ctx, sql, 0);
                        if (status == 5)
                        {
                            if (mo.FeedMtrlInfo == null)
                            {
                                request.OrderId = 2;
                                response.Data = request;
                                response.Code = ResponseCode.Success;
                                response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]已完工。";
                            }
                            else
                            {
                                response.Code = ResponseCode.ModelError;
                                response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]已完工，不能退料。";
                            }
                            return response;
                        }
                        else if (status == 6)
                        {
                            if (mo.FeedMtrlInfo == null)
                            {
                
                                request.OrderId = 2;
                                response.Data = request;
                                response.Code = ResponseCode.Success;
                                response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]已结案。";
                            }
                            else
                            {
                                response.Code = ResponseCode.ModelError;
                                response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]已结案，不能退料。";
                            }
                            return response;
                        }
                        else if (status == 7)
                        {
                            if (mo.FeedMtrlInfo == null)
                            {
                                request.OrderId = 2;
                                response.Data = request;
                                response.Code = ResponseCode.Success;
                                response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]已结算。";
                            }
                            else
                            {
                                response.Code = ResponseCode.ModelError;
                                response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]已结算，不能退料。";
                            }
                            return response;
                        }
                        if (mo.FeedMtrlInfo != null)
                        {
                            List<MesProductionReturnMtrlEntity> list = new List<MesProductionReturnMtrlEntity>();
                            if (IsExistsBillNo(ctx, mo.FeedMtrlInfo.ReturnBillNo))
                            {
                                response.Code = ResponseCode.ModelError;
                                response.Message = $"退料单号[{mo.FeedMtrlInfo.ReturnBillNo}]已存在！";
                                return response;
                            }
                            string sourceFormId = "PRD_FeedMtrl";
                            foreach (var items in mo.FeedMtrlInfo.SourceDetails)
                            {
                                List<SqlParam> pars = new List<SqlParam>() {
                        new SqlParam("@FMOID", KDDbType.Int64, request.Id),
                        new SqlParam("@FMOENTRYID", KDDbType.Int64, mo.EntryId),
                        new SqlParam("@FBILLNO", KDDbType.String, items.ReturnSourceBillNo)
                        };
                                int i = 1;
                                string param = string.Empty;
                                foreach (var item in items.SubDetails)
                                {
                                    if (i == 1)
                                        param = "@ItemNo" + i;
                                    else
                                        param += ",@ItemNo" + i;
                                    pars.Add(new SqlParam("@ItemNo" + i++, KDDbType.String, item.MaterialCode));
                                }
                                sql = $@"/*dialect*/select t1.FID,t1.FPRDORGID,t2.FENTRYID,t1.FDOCUMENTSTATUS,t4.FNUMBER,(t3.FACTUALQTY-t3.FSELPRCDRETURNQTY) FQty from T_PRD_FEEDMTRL t1 
									inner join T_PRD_FEEDMTRLDATA t2 on t1.FID=t2.FID
									inner join T_PRD_FEEDMTRLDATA_Q t3 on t2.FENTRYID=t3.FENTRYID
									inner join T_BD_MATERIAL t4 on t2.FMATERIALID=t4.FMATERIALID
									where t1.FBILLNO=@FBILLNO and t2.FMOID=@FMOID and t2.FMOENTRYID=@FMOENTRYID
									and t4.FNUMBER in ({param}) ";
                                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                                if (datas.Count == 0)
                                {
                                    response.Code = ResponseCode.ModelError;
                                    response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]不存在补料订单[{items.ReturnSourceBillNo}]！";
                                    return response;
                                }
                                if (!datas[0]["FDOCUMENTSTATUS"].ToString().EqualsIgnoreCase("C"))
                                {
                                    response.Code = ResponseCode.ModelError;
                                    response.Message = $"补料订单[{items.ReturnSourceBillNo}]不是已批核状态！";
                                    return response;
                                }
                                //验证物料和数量是否足够
                                foreach (var item in items.SubDetails)
                                {
                                    if (datas.Where(x => x["FNUMBER"].Equals(item.MaterialCode)).Count() == 0)
                                    {
                                        response.Code = ResponseCode.ModelError;
                                        response.Message = $"补料订单[{items.ReturnSourceBillNo}]不存在物料[{item.MaterialCode}]";
                                        return response;
                                    }
                                    var returnableQty = Convert.ToDecimal(datas.Where(x => x["FNUMBER"].Equals(item.MaterialCode)).Select(p => p["FQty"]).FirstOrDefault());
                                    if (item.Qty > returnableQty)
                                    {
                                        response.Code = ResponseCode.ModelError;
                                        response.Message = $"补料订单[{items.ReturnSourceBillNo}]的物料[{item.MaterialCode}]可退数量不足。";
                                        return response;
                                    }
                                }
                                //重构数据
                                foreach (var item in datas)
                                {
                                    list.Add(new MesProductionReturnMtrlEntity
                                    {
                                        FPRDORGID = Convert.ToInt64(item["FPRDORGID"]),
                                        FID = Convert.ToInt64(item["FID"]),
                                        FENTRYID = Convert.ToInt64(item["FENTRYID"]),
                                        FQTY = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.Qty).FirstOrDefault(),
                                        FSTOCKCODE = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.StockCode).FirstOrDefault(),
                                        FReturnType = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.ReturnType).FirstOrDefault(),
                                        FReturnReason = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.ReturnReason).FirstOrDefault()
                                    });
                                }
                            }
                            //下推退料订单
                            PushReturnMtrl(ctx, sourceFormId, mo.FeedMtrlInfo.ReturnBillNo, list);
                        }

                    }
                    request.OrderId = 2;
                    response.Data = request;
                    response.Code = ResponseCode.Success;
                    response.Message = "补料退料成功";

                }
                else if (request.OrderId == 2)
                {
                    foreach (var mo in request.MoDetails)
                    {
                        if (mo.PickMtrlInfo != null)
                        {
                            if (IsExistsBillNo(ctx, mo.PickMtrlInfo.ReturnBillNo))
                            {
                                response.Code = ResponseCode.ModelError;
                                response.Message = $"退料单号[{mo.PickMtrlInfo.ReturnBillNo}]已存在！";
                                return response;
                            }
                            List<MesProductionReturnMtrlEntity> list = new List<MesProductionReturnMtrlEntity>();
                            string sourceFormId = "PRD_PickMtrl";
                            foreach (var items in mo.PickMtrlInfo.SourceDetails)
                            {
                                List<SqlParam> pars = new List<SqlParam>() {
                                    new SqlParam("@FMOID", KDDbType.Int64, request.Id),
                                    new SqlParam("@FMOENTRYID", KDDbType.Int64, mo.EntryId),
                                    new SqlParam("@FBILLNO", KDDbType.String, items.ReturnSourceBillNo)
                                    };
                                int i = 1;
                                string param = string.Empty;
                                foreach (var item in items.SubDetails)
                                {
                                    if (i == 1)
                                        param = "@ItemNo" + i;
                                    else
                                        param += ",@ItemNo" + i;
                                    pars.Add(new SqlParam("@ItemNo" + i++, KDDbType.String, item.MaterialCode));
                                }
                                var sql = $@"/*dialect*/select t1.FID,t1.FPRDORGID,t2.FENTRYID,t1.FDOCUMENTSTATUS,t3.FNUMBER,(t2.FACTUALQTY-t2.FSELPRCDRETURNQTY) FQty from T_PRD_PICKMTRL t1 
									inner join T_PRD_PICKMTRLDATA t2 on t1.FID=t2.FID
									inner join T_BD_MATERIAL t3 on t2.FMATERIALID=t3.FMATERIALID
									where t1.FBILLNO=@FBILLNO and t2.FMOID=@FMOID and t2.FMOENTRYID=@FMOENTRYID
									and t3.FNUMBER in ({param}) ";
                                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                                if (datas.Count == 0)
                                {
                                    response.Code = ResponseCode.ModelError;
                                    response.Message = $"生产订单[{request.BillNo}-{mo.BillSeq}]不存在领料订单[{items.ReturnSourceBillNo}]！";
                                    return response;
                                }
                                if (!datas[0]["FDOCUMENTSTATUS"].ToString().EqualsIgnoreCase("C"))
                                {
                                    response.Code = ResponseCode.ModelError;
                                    response.Message = $"领料订单[{items.ReturnSourceBillNo}]不是已批核状态！";
                                    return response;
                                }
                                //验证物料和数量是否足够
                                foreach (var item in items.SubDetails)
                                {
                                    if (datas.Where(x => x["FNUMBER"].Equals(item.MaterialCode)).Count() == 0)
                                    {
                                        response.Code = ResponseCode.ModelError;
                                        response.Message = $"领料订单[{items.ReturnSourceBillNo}]不存在物料[{item.MaterialCode}]";
                                        return response;
                                    }
                                    var returnableQty = Convert.ToDecimal(datas.Where(x => x["FNUMBER"].Equals(item.MaterialCode)).Select(p => p["FQty"]).FirstOrDefault());
                                    if (item.Qty > returnableQty)
                                    {
                                        response.Code = ResponseCode.ModelError;
                                        response.Message = $"领料订单[{items.ReturnSourceBillNo}]的物料[{item.MaterialCode}]可退数量不足。";
                                        return response;
                                    }
                                }
                                //重构数据
                                foreach (var item in datas)
                                {
                                    list.Add(new MesProductionReturnMtrlEntity
                                    {
                                        FPRDORGID = Convert.ToInt64(item["FPRDORGID"]),
                                        FID = Convert.ToInt64(item["FID"]),
                                        FENTRYID = Convert.ToInt64(item["FENTRYID"]),
                                        FQTY = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.Qty).FirstOrDefault(),
                                        FSTOCKCODE = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.StockCode).FirstOrDefault(),
                                        FReturnType = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.ReturnType).FirstOrDefault(),
                                        FReturnReason = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.ReturnReason).FirstOrDefault()
                                    });
                                }
                            }
                            //下推退料订单
                            PushReturnMtrl(ctx, sourceFormId, mo.PickMtrlInfo.ReturnBillNo, list);
                        }
                    }
                    PrdMoStatus prdMoStatus = new PrdMoStatus();
                    prdMoStatus.Type = request.Type;
                    prdMoStatus.BillNo = request.BillNo;
                    prdMoStatus.MoId = request.Id;
                    prdMoStatus.MoEntryId = request.MoDetails.Select(x => x.EntryId).ToArray();
                    response.Data = prdMoStatus;
                    response.Code = ResponseCode.Success;
                    response.Message = "领料退料成功";
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;

            }
            return response;



            //var operateOption = OperateOption.Create();
            //operateOption.SetIgnoreWarning(true);
            //FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PRD_MO") as FormMetadata;
            //object[] ids = mostatus.MoEntryId.ToArray<object>();
            //List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(mostatus.MoId, x)).ToList();
            ////object[] ids = new object[] { mostatus.MoEntryId };

            //SetStatusService setStatusService = new SetStatusService();
            //using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            //{
            //    IOperationResult oper2 = new OperationResult();
            //    switch (mostatus.Type)
            //    {
            //        case 1:
            //            //执行至结案
            //            oper2 = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "ToClose", operateOption);
            //            break;
            //        case 2:
            //            //强制结案
            //            oper2 = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "ForceClose", operateOption);
            //            break;
            //    }
            //    if (oper2.ValidationErrors.Count > 0)
            //    {
            //        response.Message = string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
            //        return response;
            //    }

            //    response.Code = ResponseCode.Success;
            //    response.Message = "操作成功";
            //    cope.Complete();
            //    return response;
            //}

        }

        /// <summary>
        /// 下推退料
        /// </summary>
        public string PushReturnMtrl(Context ctx, string sourceFormId, string returnBillNo, List<MesProductionReturnMtrlEntity> list)
        {
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            foreach (var item in list)
            {
                selectedRows.Add(new ListSelectedRow(Convert.ToString(item.FID), Convert.ToString(item.FENTRYID), 0, sourceFormId) { EntryEntityKey = "FEntity" });
            }
            // 生产领料或者补料下推生产退料单
            var rules = ConvertServiceHelper.GetConvertRules(ctx, sourceFormId, "PRD_ReturnMtrl");

            PushArgs pushArgs = new PushArgs(rules.Count() == 1 ? rules.FirstOrDefault() : rules.FirstOrDefault(t => t.IsDefault), selectedRows.ToArray())
            {
                TargetBillTypeId = "c4e4cef46c844a2bb2a7faf0cf6dc2cb", // 请设定目标单据单据类型
                TargetOrgId = list[0].FPRDORGID,            // 请设定目标单据主业务组织
            };
            //执行下推操作，并获取下推结果
            var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
            if (operationResult.IsSuccess)
            {
                var view = FormMetadataUtils.CreateBillView(ctx, "PRD_ReturnMtrl");
                foreach (var item in operationResult.TargetDataEntities)
                {
                    view.Model.DataObject = item.DataEntity;
                    view.Model.SetValue("FBillNo", returnBillNo);
                    var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
                    var rowIndex = 0;
                    foreach (var entry in entrys)
                    {
                        var thisList = list.Where(x => x.FENTRYID.Equals(Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"])))
                        .FirstOrDefault();
                        view.Model.SetValue("FAppQty", thisList.FQTY, rowIndex);
                        view.InvokeFieldUpdateService("FAppQty", rowIndex);
                        view.Model.SetValue("FQty", thisList.FQTY, rowIndex);
                        view.InvokeFieldUpdateService("FActualQty", rowIndex);
                        view.Model.SetItemValueByNumber("FStockId", thisList.FSTOCKCODE, rowIndex);
                        view.InvokeFieldUpdateService("FStockId", rowIndex);
                        view.Model.SetValue("FReturnType", thisList.FReturnType, rowIndex);
                        view.Model.SetItemValueByNumber("FReturnReason", thisList.FReturnReason, rowIndex);
                        rowIndex++;
                    }
                }
                //保存批核
                var opers = service.SaveAndAuditBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject }.ToArray());
                if (opers.IsSuccess)
                {
                    //清除释放网控
                    view.CommitNetworkCtrl();
                    view.InvokeFormOperation(FormOperationEnum.Close);
                    view.Close();
                    return string.Join(";", opers.OperateResult.Select(p => p.Message));
                }
                else
                {
                    if (opers.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }
            }
            else
            {
                if (operationResult.ValidationErrors.Count > 0)
                {
                    throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
                }
                else
                {
                    throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                }
            }
        }

        /// <summary>
        /// 验证生产退料单是否存在
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgCode"></param>
        /// <returns></returns>
        private bool IsExistsBillNo(Context ctx, string BillNo)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@BillNo", KDDbType.String, BillNo) };
            var sql = $@"select count(1) from T_PRD_ReturnMTRL where FBILLNO=@BillNo ";
            return DBUtils.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray()) > 0 ? true : false;
        }
    }
}
