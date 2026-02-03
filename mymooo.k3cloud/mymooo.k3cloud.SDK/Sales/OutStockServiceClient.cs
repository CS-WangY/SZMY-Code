using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils;
using mymooo.core.Utils.NewtonsoftJsonConverter;
using mymooo.core.Utils.Service;
using mymooo.k3cloud.core.Sales;

namespace mymooo.k3cloud.SDK.Sales
{
    /// <summary>
    /// 销售出库单http服务客户端
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class OutStockServiceClient<C, U>(HttpService httpService) where U : UserBase, new() where C : MymoooContext<U>
    {
        /// <summary>
        /// 获取发货数据
        /// </summary>
        public async Task<ResponseMessage<List<OutStockResponse>>> GetDeliveryList(C mymoooContext, string orderNo)
        {
            if (orderNo.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"“{nameof(orderNo)}”不能为 null 或空白。", nameof(orderNo));
            }
            return await httpService.InvokeWebServiceAsync<C, U, List<OutStockResponse>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/OutStock/GetDeliveryList?orderNo={orderNo}");
        }

        /// <summary>
        /// 售后, 退货或者换货
        /// </summary>
        public async Task<ResponseMessage<AfterSalesResponse>> ChangingOrRefunding(C mymoooContext, AfterSalesRequest request)
        {
            return await httpService.InvokeWebServiceAsync<C, U, AfterSalesResponse>(mymoooContext, $"k3cloud/{mymoooContext.ApigatewayConfig.EnvCode}/Kingdee.Mymooo.webapi.ServicesStub.SalesManagementService.ChangingOrRefunding.common.kdsvc", JsonConvertUtils.Serialize(request));
        }

        /// <summary>
        /// 查询售后单据状态
        /// </summary>
        public async Task<ResponseMessage<AfsDetailResultResponse>> AfsDetailResult(C mymoooContext, AfsDetailResultRequest request)
        {
            return await httpService.InvokeWebServiceAsync<C, U, AfsDetailResultResponse>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/OutStock/AfsDetailResult", JsonConvertUtils.Serialize(request));
        }
        /// <summary>
        /// 查询单据出库数据
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
		public async Task<ResponseMessage<List<OutStockDetail>>> QuerySalOrderOutStock(C mymoooContext, SalOutStockRequest request)
		{
			return await httpService.InvokeWebServiceAsync<C, U, List<OutStockDetail>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/OutStock/GetOutStockList", JsonConvertUtils.Serialize(request));
		}
	}
}
