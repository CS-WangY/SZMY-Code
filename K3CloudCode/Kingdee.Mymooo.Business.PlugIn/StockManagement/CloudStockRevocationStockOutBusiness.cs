using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    /// <summary>
    /// 云仓储撤回出库单接口
    /// </summary>
    public class CloudStockRevocationStockOutBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            List<CloudStockRevocationStockOutRequest> requests = JsonConvertUtils.DeserializeObject<List<CloudStockRevocationStockOutRequest>>(message);
            if (requests.Count == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "同步到云仓储撤回出库单信息不能为空";
                return response;
            }
            var requestData = JsonConvertUtils.SerializeObject(requests);
            var cwResult = WarehouseApiRequest.Request(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "/api/goods/cancelleddelivery", requestData, "MYMO", "DELETE");
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
