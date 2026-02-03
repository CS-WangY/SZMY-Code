using Kingdee.BOS;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
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
    /// 出库单同步到云仓储
    /// 出库类型有：
    /// 1.发货通知单
    /// 2.采购退货
    /// 3.其它出库
    /// 4.组装拆卸出库（含替换）
    /// 5.生产发料
    /// 6.仓存转移出库
    /// 7. 仓存盘点调整
    /// </summary>
    public class PutToTempDeliveryAreaBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            List<PutToTempDeliveryAreaRequest> putToTempDeliveryAreaRequests = JsonConvertUtils.DeserializeObject<List<PutToTempDeliveryAreaRequest>>(message);
            if (putToTempDeliveryAreaRequests.Count == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "同步到云仓储的出库信息不能为空";
                return response;
            }
            var requestData = JsonConvertUtils.SerializeObject(putToTempDeliveryAreaRequests);
            var cwResult = WarehouseApiRequest.Request(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "/api/goods/puttotempdeliveryarea", requestData);
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
