using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.Quotation;
using mymooo.core;
using mymooo.core.Attributes;
using SqlSugar;

namespace com.mymooo.mall.business.Service.SalesService
{
	[AutoInject(InJectType.Scope)]
	public class SalesOrderService(MallContext mymoooContext)
	{
		private readonly MallContext _mymoooContext = mymoooContext;

		public void ReloadCache()
		{
            
            var sql = @"select *
from (
	select isnull(pc.Code,c.Code) CompanyCode,c.Name AS CompanyName,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel,b.AuditTime,lower(replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '')) Id,d.FBD_PRD_CODE ProductNumber,isnull(d.SmallClassId,0) SmallId,isnull(s.Code,'') SmallCode,isnull(s.Name,'') SmallName
	,b.FBM_BOOK_CODE SalesOrderNo,id.ProductTypeId,d.FBD_PRD_ID ProductId,isnull(qd.QtyDiscount,100) as QtyDiscount,isnull(qd.LevelDiscount,100) as LevelDiscount,d.FBD_NUM AS Qty
	,isnull(qd.AutoDeliverySource,9) DeliverySource,isnull(qd.FQD_DELIVE_DAYS, -1) DeliveDays,qd.OriginalUnitPriceWithTax OriginalPrice,d.FBD_UNIT_TAX_PRICE TaxPrice
	,row_number() over (partition by isnull(pc.Code,c.Code),replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '') order by b.AuditTime desc) RowIndex,s.IsFa
	from F_CUST_BOOK_MSTR b
		inner join Company c on b.CompanyId = c.Id
		left join Grade e on c.GradeLevel = e.Id
		left join GradeRule f on c.OccupancyId = f.Id
		left join GradeRule g on c.SensitivityId = g.Id
		left join SubAndParentCompany sc on c.Id = sc.CompanyId
		left join Company pc on sc.ParentCompanyId = pc.Id
		inner join F_CUST_BOOK_DETAIL d on b.FBM_BOOK_ID = d.FBD_BOOK_MSTR_ID
		left join ProductSmallClass s on d.SmallClassId = s.Id
		left join F_CUST_INQUIRY_DETAIL id on d.InquiryItemId = id.FID_DETAIL_ID
		left join F_CUST_QUOTATION_DETAIL qd on qd.FQD_INQ_DETAIL_ID = id.FID_DETAIL_ID
	where b.FBM_BOOK_STATE not in (3,8,9)  and d.FBD_UNIT_TAX_PRICE > 0 and b.AuditTime > @StartDate
	) t 
where t.RowIndex = 1";
			var startDate = DateTime.Now.Date.AddYears(-1);
			var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<SalesHistoryPrice>(sql, new SugarParameter("@StartDate", startDate)).ToList();

			foreach (SalesHistoryPrice data in datas)
			{
                //data.PriceSource = PriceSource.history;
                _mymoooContext.RedisCache.HashSet(data);
            }
			ReloadFullCache();
            ReloadMinCache();
        }

		public ResponseMessage<string> UpdateCache(UpdateSalesHistoryCacheRequest request)
		{
			ResponseMessage<string> response = new();
			var sql = @"select *
from (
	select isnull(pc.Code,c.Code) CompanyCode,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel,c.Name AS CompanyName,b.AuditTime,lower(replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '')) Id,d.FBD_PRD_CODE ProductNumber,isnull(d.SmallClassId,0) SmallId,isnull(s.Code,'') SmallCode,isnull(s.Name,'') SmallName
	,b.FBM_BOOK_CODE SalesOrderNo,id.ProductTypeId,d.FBD_PRD_ID ProductId,isnull(qd.QtyDiscount,100) as QtyDiscount,isnull(qd.LevelDiscount,100) as LevelDiscount,d.FBD_NUM AS Qty
	,isnull(qd.AutoDeliverySource,9) DeliverySource,isnull(qd.FQD_DELIVE_DAYS, -1) DeliveDays,qd.OriginalUnitPriceWithTax OriginalPrice,d.FBD_UNIT_TAX_PRICE TaxPrice
	,row_number() over (partition by isnull(pc.Code,c.Code),replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '') order by b.AuditTime desc) RowIndex
	from F_CUST_BOOK_MSTR b
		inner join Company c on b.CompanyId = c.Id
		left join Grade e on c.GradeLevel = e.Id
		left join GradeRule f on c.OccupancyId = f.Id
		left join GradeRule g on c.SensitivityId = g.Id
		left join SubAndParentCompany sc on c.Id = sc.CompanyId
		left join Company pc on sc.ParentCompanyId = pc.Id
		inner join F_CUST_BOOK_DETAIL d on b.FBM_BOOK_ID = d.FBD_BOOK_MSTR_ID
		left join ProductSmallClass s on d.SmallClassId = s.Id
		left join F_CUST_INQUIRY_DETAIL id on d.InquiryItemId = id.FID_DETAIL_ID
		left join F_CUST_QUOTATION_DETAIL qd on qd.FQD_INQ_DETAIL_ID = id.FID_DETAIL_ID
	where b.FBM_BOOK_STATE not in (3,8,9)  and d.FBD_UNIT_TAX_PRICE > 0 and b.FBM_BOOK_CODE = @FBM_BOOK_CODE
	) t 
where t.RowIndex = 1";
			var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<SalesHistoryPrice>(sql, new SugarParameter("@FBM_BOOK_CODE", request.SalesOrderNo)).ToList();

			foreach (SalesHistoryPrice data in datas)
			{
                _mymoooContext.RedisCache.HashSet(data);
            }
            UpdateFullCache(request);
			UpdateMinCache(request);
            response.Code = ResponseCode.Success;
			return response;
		}

        public void ReloadFullCache()
        {
            var sql = @"select *
from (
	select isnull(pc.Code,c.Code) CompanyCode,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel,c.Name AS CompanyName,b.AuditTime,lower(replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '')) Id,d.FBD_PRD_CODE ProductNumber,isnull(d.SmallClassId,0) SmallId,isnull(s.Code,'') SmallCode,isnull(s.Name,'') SmallName
	,b.FBM_BOOK_CODE SalesOrderNo,id.ProductTypeId,d.FBD_PRD_ID ProductId,isnull(qd.QtyDiscount,100) as QtyDiscount,isnull(qd.LevelDiscount,100) as LevelDiscount,d.FBD_NUM AS Qty
	,isnull(qd.AutoDeliverySource,9) DeliverySource,isnull(qd.FQD_DELIVE_DAYS, -1) DeliveDays,qd.OriginalUnitPriceWithTax OriginalPrice,d.FBD_UNIT_TAX_PRICE TaxPrice
	,row_number() over (partition by replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '') order by b.AuditTime desc) RowIndex
	from F_CUST_BOOK_MSTR b
		inner join Company c on b.CompanyId = c.Id
		left join Grade e on c.GradeLevel = e.Id
		left join GradeRule f on c.OccupancyId = f.Id
		left join GradeRule g on c.SensitivityId = g.Id
		left join SubAndParentCompany sc on c.Id = sc.CompanyId
		left join Company pc on sc.ParentCompanyId = pc.Id
		inner join F_CUST_BOOK_DETAIL d on b.FBM_BOOK_ID = d.FBD_BOOK_MSTR_ID
		left join ProductSmallClass s on d.SmallClassId = s.Id
		left join F_CUST_INQUIRY_DETAIL id on d.InquiryItemId = id.FID_DETAIL_ID
		left join F_CUST_QUOTATION_DETAIL qd on qd.FQD_INQ_DETAIL_ID = id.FID_DETAIL_ID
	where b.FBM_BOOK_STATE not in (3,8,9)  and d.FBD_UNIT_TAX_PRICE > 0 and b.AuditTime > @StartDate
	) t 
where t.RowIndex = 1";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<SalesFullHistoryPrice>(sql, new SugarParameter("@StartDate", startDate)).ToList();

            foreach (var data in datas)
            {
                data.PriceSource = PriceSource.fhistory;
                _mymoooContext.RedisCache.HashSet(data);
            }

			
        }

        public ResponseMessage<string> UpdateFullCache(UpdateSalesHistoryCacheRequest request)
        {
            ResponseMessage<string> response = new();
            var sql = @"select *
from (
	select isnull(pc.Code,c.Code) CompanyCode,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel,c.Name AS CompanyName,b.AuditTime,lower(replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '')) Id,d.FBD_PRD_CODE ProductNumber,isnull(d.SmallClassId,0) SmallId,isnull(s.Code,'') SmallCode,isnull(s.Name,'') SmallName
	,b.FBM_BOOK_CODE SalesOrderNo,id.ProductTypeId,d.FBD_PRD_ID ProductId,isnull(qd.QtyDiscount,100) as QtyDiscount,isnull(qd.LevelDiscount,100) as LevelDiscount,d.FBD_NUM AS Qty
	,isnull(qd.AutoDeliverySource,9) DeliverySource,isnull(qd.FQD_DELIVE_DAYS, -1) DeliveDays,qd.OriginalUnitPriceWithTax OriginalPrice,d.FBD_UNIT_TAX_PRICE TaxPrice
	,row_number() over (partition by replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '') order by b.AuditTime desc) RowIndex
	from F_CUST_BOOK_MSTR b
		inner join Company c on b.CompanyId = c.Id
		left join Grade e on c.GradeLevel = e.Id
		left join GradeRule f on c.OccupancyId = f.Id
		left join GradeRule g on c.SensitivityId = g.Id
		left join SubAndParentCompany sc on c.Id = sc.CompanyId
		left join Company pc on sc.ParentCompanyId = pc.Id
		inner join F_CUST_BOOK_DETAIL d on b.FBM_BOOK_ID = d.FBD_BOOK_MSTR_ID
		left join ProductSmallClass s on d.SmallClassId = s.Id
		left join F_CUST_INQUIRY_DETAIL id on d.InquiryItemId = id.FID_DETAIL_ID
		left join F_CUST_QUOTATION_DETAIL qd on qd.FQD_INQ_DETAIL_ID = id.FID_DETAIL_ID
	where b.FBM_BOOK_STATE not in (3,8,9)  and d.FBD_UNIT_TAX_PRICE > 0 and b.FBM_BOOK_CODE = @FBM_BOOK_CODE
	) t 
where t.RowIndex = 1";
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<SalesFullHistoryPrice>(sql, new SugarParameter("@FBM_BOOK_CODE", request.SalesOrderNo)).ToList();

            foreach (var data in datas)
            {
				data.PriceSource = PriceSource.fhistory;
                _mymoooContext.RedisCache.HashSet(data);
            }

            response.Code = ResponseCode.Success;
            return response;
        }

        public void ReloadMinCache()
        {
            var sql = @"select *
from (
	select isnull(pc.Code,c.Code) CompanyCode,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel,c.Name AS CompanyName,b.AuditTime,lower(replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '')) Id,d.FBD_PRD_CODE ProductNumber,isnull(d.SmallClassId,0) SmallId,isnull(s.Code,'') SmallCode,isnull(s.Name,'') SmallName
	,b.FBM_BOOK_CODE SalesOrderNo,id.ProductTypeId,d.FBD_PRD_ID ProductId,isnull(qd.QtyDiscount,100) as QtyDiscount,isnull(qd.LevelDiscount,100) as LevelDiscount,d.FBD_NUM AS Qty
	,isnull(qd.AutoDeliverySource,9) DeliverySource,isnull(qd.FQD_DELIVE_DAYS, -1) DeliveDays,qd.OriginalUnitPriceWithTax OriginalPrice,d.FBD_UNIT_TAX_PRICE TaxPrice
	,row_number() over (partition by replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '') order by d.FBD_UNIT_TAX_PRICE) RowIndex
	from F_CUST_BOOK_MSTR b
		inner join Company c on b.CompanyId = c.Id
		left join Grade e on c.GradeLevel = e.Id
		left join GradeRule f on c.OccupancyId = f.Id
		left join GradeRule g on c.SensitivityId = g.Id
		left join SubAndParentCompany sc on c.Id = sc.CompanyId
		left join Company pc on sc.ParentCompanyId = pc.Id
		inner join F_CUST_BOOK_DETAIL d on b.FBM_BOOK_ID = d.FBD_BOOK_MSTR_ID
		left join ProductSmallClass s on d.SmallClassId = s.Id
		left join F_CUST_INQUIRY_DETAIL id on d.InquiryItemId = id.FID_DETAIL_ID
		left join F_CUST_QUOTATION_DETAIL qd on qd.FQD_INQ_DETAIL_ID = id.FID_DETAIL_ID
	where b.FBM_BOOK_STATE not in (3,8,9)  and d.FBD_UNIT_TAX_PRICE > 0 and b.AuditTime > @StartDate
	) t 
where t.RowIndex = 1";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<SalesFullHistoryPrice>(sql, new SugarParameter("@StartDate", startDate)).ToList();

            foreach (var data in datas)
            {
                data.PriceSource = PriceSource.minhistory;
                _mymoooContext.RedisCache.HashSet(data);
            }
        }

        public ResponseMessage<string> UpdateMinCache(UpdateSalesHistoryCacheRequest request)
        {
            ResponseMessage<string> response = new();
            var sql = @"select *
from (
	select isnull(pc.Code,c.Code) CompanyCode,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel,c.Name AS CompanyName,b.AuditTime,lower(replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '')) Id,d.FBD_PRD_CODE ProductNumber,isnull(d.SmallClassId,0) SmallId,isnull(s.Code,'') SmallCode,isnull(s.Name,'') SmallName
	,b.FBM_BOOK_CODE SalesOrderNo,id.ProductTypeId,d.FBD_PRD_ID ProductId,isnull(qd.QtyDiscount,100) as QtyDiscount,isnull(qd.LevelDiscount,100) as LevelDiscount,d.FBD_NUM AS Qty
	,isnull(qd.AutoDeliverySource,9) DeliverySource,isnull(qd.FQD_DELIVE_DAYS, -1) DeliveDays,qd.OriginalUnitPriceWithTax OriginalPrice,d.FBD_UNIT_TAX_PRICE TaxPrice
	,row_number() over (partition by replace(replace(d.FBD_PRD_CODE,'-',''), ' ', '') order by d.FBD_UNIT_TAX_PRICE) RowIndex
	from F_CUST_BOOK_MSTR b
		inner join Company c on b.CompanyId = c.Id
		left join Grade e on c.GradeLevel = e.Id
		left join GradeRule f on c.OccupancyId = f.Id
		left join GradeRule g on c.SensitivityId = g.Id
		left join SubAndParentCompany sc on c.Id = sc.CompanyId
		left join Company pc on sc.ParentCompanyId = pc.Id
		inner join F_CUST_BOOK_DETAIL d on b.FBM_BOOK_ID = d.FBD_BOOK_MSTR_ID
		left join ProductSmallClass s on d.SmallClassId = s.Id
		left join F_CUST_INQUIRY_DETAIL id on d.InquiryItemId = id.FID_DETAIL_ID
		left join F_CUST_QUOTATION_DETAIL qd on qd.FQD_INQ_DETAIL_ID = id.FID_DETAIL_ID
	where b.FBM_BOOK_STATE not in (3,8,9)  and d.FBD_UNIT_TAX_PRICE > 0 and b.FBM_BOOK_CODE = @FBM_BOOK_CODE
	) t 
where t.RowIndex = 1";
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<SalesFullHistoryPrice>(sql, new SugarParameter("@FBM_BOOK_CODE", request.SalesOrderNo)).ToList();

            foreach (var data in datas)
            {
                data.PriceSource = PriceSource.minhistory;
                _mymoooContext.RedisCache.HashSet(data);
            }
            response.Code = ResponseCode.Success;
            return response;
        }

        private void SalesHistoryCache(SalesHistoryPrice data)
		{
			_mymoooContext.RedisCache.HashSet(data);
			//ProductSelectionPriceIndex priceIndex = new()
			//{
			//	PorductNumber = data.ProductNumber,
			//	CompanyCode = data.CompanyCode,
			//	PriceSource = data.PriceSource,
			//	SmallId = data.SmallId,
			//	TargetPriceSource = PriceSource.currentCustomer,
			//	DeliverySource = data.DeliverySource,
			//	LadderPrices =
			//	   [
			//		   new()
			//		   {
			//			   Price = data.OriginalPrice,
			//			   SalesPrice = data.TaxPrice,
			//			   DeliveryDay = data.DeliveDays,
			//			   NumberLimit = 999999,
			//			   QuantityDiscount = data.QtyDiscount,
			//			   LevelDiscount = data.LevelDiscount
			//		   }
			//	   ]
			//};
			////判断当前缓存的
			//var currentCustomer = _mymoooContext.RedisCache.HashGet(new ProductSelectionPriceIndex() { Id = data.Id, CompanyCode = data.CompanyCode, TargetPriceSource = PriceSource.currentCustomer });
			//var currentCommon = _mymoooContext.RedisCache.HashGet(new ProductSelectionPriceIndex() { Id = data.Id, CompanyCode = data.CompanyCode, TargetPriceSource = PriceSource.currentCommon });
			//if (currentCommon != null && (priceIndex.DeliverySource == DeliverySource.none || priceIndex.DeliverySource == DeliverySource.Stock || priceIndex.DeliverySource == DeliverySource.Suplier))
			//{
			//	priceIndex.DeliverySource = DeliverySource.cacheCommon;
			//	priceIndex.LadderPrices[0].DeliveryDay = currentCommon.LadderPrices.FirstOrDefault()?.DeliveryDay ?? -1;
			//}
			//if (currentCustomer != null)
			//{
			//	if (currentCustomer.PriceSource != PriceSource.cacheCustomer)
			//	{
			//		_mymoooContext.RedisCache.HashSet(priceIndex);
			//	}
			//}
			//else
			//{
			//	_mymoooContext.RedisCache.HashSet(priceIndex);
			//}
		}

	}
}
