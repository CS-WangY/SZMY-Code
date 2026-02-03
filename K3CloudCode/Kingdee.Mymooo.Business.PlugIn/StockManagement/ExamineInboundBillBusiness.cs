using Kingdee.BOS;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    /// <summary>
    /// 修改云存储调拨入库审核状态
    /// </summary>
    public class ExamineInboundBillBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            List<ExamineInboundBillModel> requests = JsonConvertUtils.DeserializeObject<List<ExamineInboundBillModel>>(message);
            if (requests.Count == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "同步到云仓储的出库信息不能为空";
                return response;
            }
            var requestData = JsonConvertUtils.SerializeObject(requests);
            var cwResult = WarehouseApiRequest.Request(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "/api/goods/ExamineInboundBill", requestData, "MYMO", "POST");
            var returnInfo = JsonConvertUtils.DeserializeObject<ResponseCloudWarehouseMessage>(cwResult);
            if (!returnInfo.IsSuccess)
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = returnInfo.Message;
                return response;
            }
            response.Code = ResponseCode.Success;
            response.Message = "同步成功";
            return response;
        }
    }
}
