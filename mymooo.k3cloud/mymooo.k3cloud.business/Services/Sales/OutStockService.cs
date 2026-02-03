using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Utils;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.Sales;
using mymooo.k3cloud.core.SqlSugarCore.Sales;
using SqlSugar;

namespace mymooo.k3cloud.business.Services.Sales
{
	/// <summary>
	/// 销售出库单服务
	/// </summary>
	[AutoInject(InJectType.Scope)]
	public class OutStockService(KingdeeContent kingdeeContent)
	{

		public async Task<ResponseMessage<List<OutStockResponse>>> GetDeliveryList(string orderNo)
		{
			var sql = @"select o.FBILLNO DeliveryNo,o.FDATE DeliveryDate,sl.FNAME WarehouseName,o.FTrackingName TrackingName,o.FTRACKINGNUMBER TrackingNumber,e.FENTRYID DetailId,m.FNUMBER SkuId,ml.FNAME SkuDesc,e.FRealQty Quantity,ul.FNAME Unit,er.FSoorDerno
from T_SAL_OUTSTOCK o
	inner join T_SAL_OUTSTOCKENTRY e on o.FID = e.FID
	inner join T_SAL_OUTSTOCKENTRY_R er on e.FENTRYID = er.FENTRYID
	inner join T_BD_STOCK_L sl on e.FSTOCKID = sl.FSTOCKID and sl.FLOCALEID = 2052
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	inner join T_BD_UNIT_L ul on e.FBASEUNITID = ul.FUNITID and ul.FLOCALEID = 2052
where o.FDOCUMENTSTATUS = 'C' and er.FSoorDerno = @OrderNo";
			var paramList = new List<SugarParameter>()
				{
					new("@OrderNo", orderNo)
				};
			ResponseMessage<List<OutStockResponse>> response = new() { Data = [] };
			foreach (var detail in await kingdeeContent.SqlSugar.Ado.SqlQueryAsync<OutStockDetail>(sql, paramList))
			{
				var delivery = response.Data.FirstOrDefault(p => p.DeliveryNo.EqualsOrdinalIgnoreCase(detail.DeliveryNo));
				if (delivery == null)
				{
					delivery = new OutStockResponse()
					{
						DeliveryNo = detail.DeliveryNo,
						DeliveryDate = detail.DeliveryDate,
						WarehouseName = detail.WarehouseName,
						Skus = []
					};
					delivery.LogisticsList.Add(new OutStockResponse.LogisticsInfo()
					{
						ThirdOrderId = orderNo,
						LogisticsCompany = detail.TrackingName,
						DeliveryId = detail.TrackingNumber
                    });

                    response.Data.Add(delivery);
				}
				delivery.Skus.Add(new OutStockResponse.SkuInfo()
				{
					SkuId = detail.SkuId,
					SkuDesc = detail.SkuDesc,
					DetailId = detail.DetailId,
					Quantity = detail.Quantity,
					Unit = detail.Unit,
				});
			}

			return response;
		}

		public async Task<ResponseMessage<AfsDetailResultResponse>> AfsDetailResult(AfsDetailResultRequest request)
		{
			ResponseMessage<AfsDetailResultResponse> response = new() { Data = new AfsDetailResultResponse() { AfsApplyId = request.AfsApplyId, State = 2 } };

			var returnNotice = await kingdeeContent.SqlSugar.Queryable<ReturnNotice>()
				.Where(p => p.BillNo == request.AfsApplyId)
				.Select(p => new ReturnNotice() { DocumentStatus = p.DocumentStatus, CancelStatus = p.CancelStatus }).FirstAsync();
			if (returnNotice == null)
			{
				response.Data.State = 3;
				return response;
			}
			if (returnNotice.DocumentStatus == "C")
			{
				response.Data.State = 1;
				return response;
			}
			if (returnNotice.CancelStatus == "B")
			{
				response.Data.State = 0;
				return response;
			}
			return response;
		}

		public ResponseMessage<List<OutStockDetail>> GetOutStockList(SalOutStockRequest request)
		{
			var sql = @"select o.FBILLNO DeliveryNo,o.FDATE DeliveryDate,sl.FNAME WarehouseName,o.FTRACKINGNUMBER TrackingNumber,e.FENTRYID DetailId,m.FNUMBER SkuId,ml.FNAME SkuDesc,e.FRealQty Quantity,ul.FNAME Unit
from T_SAL_OUTSTOCK o
	inner join T_SAL_OUTSTOCKENTRY e on o.FID = e.FID
	inner join T_SAL_OUTSTOCKENTRY_R er on e.FENTRYID = er.FENTRYID
	inner join T_BD_STOCK_L sl on e.FSTOCKID = sl.FSTOCKID and sl.FLOCALEID = 2052
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	inner join T_BD_UNIT_L ul on e.FBASEUNITID = ul.FUNITID and ul.FLOCALEID = 2052
	LEFT JOIN dbo.T_SAL_ORDERENTRY t3 ON er.FSOENTRYID=t3.FENTRYID
where o.FDOCUMENTSTATUS = 'C' and er.FSoorDerno = @OrderNo ";
			if (request.SalOrderSeq > 0)
			{
				sql += " and t3.FSEQ=@OrderSeq";
            }
			var paramList = new List<SugarParameter>()
				{
					new("@OrderNo", request.SalOrderBillNo),
					new("@OrderSeq", request.SalOrderSeq)
				};
			ResponseMessage<List<OutStockDetail>> response = new() { Data = [] };
			response.Data = kingdeeContent.SqlSugar.Ado.SqlQuery<OutStockDetail>(sql, paramList);
			return response;
		}
	}
}
