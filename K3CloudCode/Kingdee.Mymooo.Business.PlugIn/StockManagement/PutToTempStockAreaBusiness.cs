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
    /// 入库单同步到云仓储
    /// 入库类型有：
    /// 1. 采购入库
    /// 2. 其它入库
    /// 3. 销售退货
    /// 4. 组装拆卸入库（含替换）
    /// 5. 产成品入库
    /// 6. 生产退料
    /// 7. 仓存转移入库
    /// 8. 仓存盘点调整
    /// </summary>
    /// <returns></returns>
    public class PutToTempStockAreaBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            List<PutToTempStockAreaRequest> putToTempStockAreaRequests = JsonConvertUtils.DeserializeObject<List<PutToTempStockAreaRequest>>(message);
            if (putToTempStockAreaRequests.Count == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "同步到云仓储入库的信息不能为空";
                return response;
            }
            var requestData = JsonConvertUtils.SerializeObject(putToTempStockAreaRequests);
            var cwResult = WarehouseApiRequest.Request(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "/api/goods/puttotempstockarea", requestData);
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
