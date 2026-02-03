using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    public class PurchaseBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            throw new NotImplementedException();
        }
        public ResponseMessage<dynamic> PurchaseSmall(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var requests = JsonConvert.DeserializeObject<PurchaseSmallRequest[]>(message);
            if (requests.Count() > 0)
            {
                var sSql = "truncate table T_BD_PurchaseSmall";
                DBServiceHelper.Execute(ctx, sSql);
            }
            var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_BD_PurchaseSmall");
            int rowIndex = 0;
            List<DynamicObject> dynamicObjects = new List<DynamicObject>();
            foreach (var request in requests)
            {
                var sSql = $@"select FSTAFFNUMBER from T_HR_EMPINFO where FWECHATCODE='{request.userCode}'";
                var STAFFNUMBER = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, "", null);
                if (STAFFNUMBER == "") { continue; }
                if (rowIndex > 0)
                {
                    billView.CreateNewModelData();
                }
                billView.Model.SetItemValueByNumber("FPurchaseId", STAFFNUMBER, 0);
                billView.Model.SetValue("FMaterialGroup", request.productSmallClassId);

                dynamicObjects.Add(billView.Model.DataObject);
                rowIndex++;
            }
            var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
            if (!oper.IsSuccess)
            {
                if (oper.ValidationErrors.Count > 0)
                {
                    response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                    response.Code = ResponseCode.Exception;
                }
                else
                {
                    response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    response.Code = ResponseCode.Exception;
                }
            }
            else
            {
                response.Code = ResponseCode.Success;
            }
            return response;
        }
    }
}
