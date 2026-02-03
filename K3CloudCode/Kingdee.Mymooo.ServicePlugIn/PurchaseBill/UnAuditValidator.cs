using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Utils;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
	public class UnAuditValidator : AbstractValidator
	{
		public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
		{
			foreach (var headEntity in dataEntities)
			{
				var supplier = headEntity["SupplierId"] as DynamicObject;
				if (supplier == null || supplier["FMQVirtualHosts"].IsNullOrEmptyOrWhiteSpace() || headEntity["FElectricSyncStatus"].ToString() != "sync")
				{
					continue;
				}
				var result = ElectricActuatorServiceUtils.InvokePostWebService("Kingdee.ElectricActuator.WebApi.ServicesStub.SalesManagementService.QuerySalesOrder.common.kdsvc"
					, JsonConvertUtils.SerializeObject(new {
						PurchaseNo = headEntity["BillNo"],
						CustomerNumber = supplier["FCustomerCode"]
					}));
				var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
				if (response.Code == ResponseCode.ExistsData)
				{
					//获取临时供应商采购订单数
					validateContext.AddError(headEntity, new ValidationErrorInfo(
						  string.Empty,
						  headEntity["Id"].ToString(),
						  headEntity.DataEntityIndex,
						  headEntity.RowIndex,
						  headEntity["Id"].ToString(),
						  string.Format("采购订单号[{0}],已同步到鑫翼胜,请联系对方删除后再反审核。", headEntity["BillNo"]),
						  $"采购订单反审核[{headEntity["BillNo"]}]",
						  ErrorLevel.FatalError));
				}
			}
		}
	}
}
