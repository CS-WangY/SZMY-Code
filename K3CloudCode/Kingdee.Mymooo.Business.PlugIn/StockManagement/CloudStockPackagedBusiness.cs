using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    public class CloudStockPackagedBusiness 
    {

        /// <summary>
        /// 修改发货通知单物流等信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> ModifyDeliveryExpressage(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            CloudWarehouseDnNoticeEntity data = JsonConvertUtils.DeserializeObject<CloudWarehouseDnNoticeEntity>(message);
            return SalesOrderServiceHelper.ModifyDeliveryExpressageService(ctx, data);
        }


        /// <summary>
        /// 全国一部,华南二部调拨出库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DeliveryTransferOut(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.ModelError;
                return response;
            }
            CloudWarehouseDnNoticeEntity data = JsonConvertUtils.DeserializeObject<CloudWarehouseDnNoticeEntity>(message);
            return SalesOrderServiceHelper.DeliveryTransferOutService(ctx, data);
        }


        /// <summary>
        /// 全国一部,华南二部调拨入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DeliveryTransferIn(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            CloudWarehouseDnNoticeEntity data = JsonConvertUtils.DeserializeObject<CloudWarehouseDnNoticeEntity>(message);
            return SalesOrderServiceHelper.DeliveryTransferInService(ctx, data);
        }


        /// <summary>
        /// 下推生成销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GenerateOutStock(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.ModelError;
                return response;
            }
            CloudWarehouseDnNoticeEntity data = JsonConvertUtils.DeserializeObject<CloudWarehouseDnNoticeEntity>(message);
            return SalesOrderServiceHelper.GenerateOutStockService(ctx, data);
        }


        /// <summary>
        /// 发货通知单变更数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> ModifyDeliveryQuantity(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.ModelError;
                return response;
            }
            CloudWarehouseDnNoticeEntity data = JsonConvertUtils.DeserializeObject<CloudWarehouseDnNoticeEntity>(message);
            return SalesOrderServiceHelper.ModifyDeliveryQuantityService(ctx, data);
        }


        /// <summary>
        /// 事业部红字调入
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> RedDeliveryTransferIn(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.ModelError;
                return response;
            }
            CloudWarehouseDnNoticeEntity data = JsonConvertUtils.DeserializeObject<CloudWarehouseDnNoticeEntity>(message);
            return SalesOrderServiceHelper.RedDeliveryTransferInService(ctx, data);
        }
    }
}
