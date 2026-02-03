using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingdee.Mymooo.Core.SalesManagement.SalesOrderBillRequest;
using System.Xml.Linq;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.App.Core.BaseManagement;
using Kingdee.Mymooo.Core;
using static Kingdee.Mymooo.Core.SalesManagement.SaleOrderRequest;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Common;

namespace Kingdee.Mymooo.App.Core.PurchaseManagement
{

    public class Pur_RequisitionOrderService : IPur_RequisitionOrderService
    {
        public ResponseMessage<dynamic> Add_PUR_Requisition(Context ctx, PUR_Requisition bill)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PUR_Requisition") as FormMetadata;
            var billView = FormMetadataUtils.CreateBillView(ctx, "PUR_Requisition");
            billView.Model.SetValue("FBillTypeID", bill.BillTypeID, 0);
            billView.Model.SetValue("FPENYBackupBillNO", bill.BillNo, 0);
            billView.Model.SetItemValueByNumber("FApplicationOrgId", bill.FApplicationOrgId, 0);

            //根据微信编号获取员工id
            string sSql = $@"SELECT FNUMBER FROM T_HR_EMPINFO
                            WHERE FWECHATCODE='{bill.FApplicantId}'";
            var userid = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, "");
            billView.Model.SetItemValueByNumber("FApplicantId", userid, 0);
            billView.InvokeFieldUpdateService("FApplicantId", 0);

            billView.Model.DeleteEntryData("FEntity");
            var rowcount = 0;
            foreach (var item in bill.Entry)
            {
                billView.Model.CreateNewEntryRow("FEntity");
                billView.Model.SetItemValueByNumber("FMaterialId", item.FMaterialId, rowcount);
                billView.InvokeFieldUpdateService("FMaterialId", rowcount);
                billView.Model.SetValue("FReqQty", item.FReqQty, rowcount);
                billView.InvokeFieldUpdateService("FReqQty", rowcount);
                billView.Model.SetValue("FSUPPLIERUNITPRICE", item.FSUPPLIERUNITPRICE, rowcount);
                billView.Model.SetValue("FEntryNote", item.FEntryNote, rowcount);
                rowcount++;
            }

            IOperationResult oper = new OperationResult();
            response.Code = ResponseCode.Success;
            response.Message = "创建成功";
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);

                SaveService saveService = new SaveService();
                oper = saveService.SaveAndAudit(ctx, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
                if (!oper.IsSuccess)
                {
                    if (oper.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }
                cope.Complete();
            }
            response.Data = oper.OperateResult.Select(x => new { x.PKValue, x.Number, x.Message });
            //创建成功发送企业微信消息
            string sendMessage = $@"#### 您的单号为<font color=""info"">{bill.BillNo}</font>的备库申请已创建成功
>{JsonConvertUtils.SerializeObject(response.Data)}";
            if (!string.IsNullOrWhiteSpace(bill.FApplicantId))
            {
                SendMarkdownMessageRequest WxEntity = new SendMarkdownMessageRequest();
                WxEntity = new SendMarkdownMessageRequest()
                {
                    ToUser = bill.FApplicantId,
                    MarkDown = new SendMarkdownMessageRequest.MarkDownMessage()
                    {
                        Content = sendMessage
                    }
                };
                ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Application/SendMarkdownMessage", JsonConvertUtils.SerializeObject(WxEntity));
                //SendTextMessageUtils.SendTextMessage(cUserWxCode, sendMessage);
            }

            return response;
        }
        public IOperationResult Del_PUR_Requisition(Context ctx, string billid)
        {
            OperateOption option = OperateOption.Create();
            FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, "PUR_Requisition");
            //DynamicObject[] array = BusinessDataServiceHelper.Load(ctx, new object[1] { num }, formMetadata.BusinessInfo.GetDynamicObjectType());
            //BusinessDataServiceHelper.UnAudit(ctx, formMetadata.BusinessInfo, new object[] { billid }, option);
            return BusinessDataServiceHelper.Delete(ctx, formMetadata.BusinessInfo, new object[] { billid }, option);
        }
    }
}
