using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using mymooo.redis;
using mymooo.redis.Attributes;
using System.Diagnostics;
using System;

namespace Kingdee.Mymooo.ServicePlugIn.STK.StkUnAudit
{
	public class UnAuditValidator : AbstractValidator
	{
		public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
		{
			SalesHistoryPrice salesHistoryPrice = new SalesHistoryPrice();
			salesHistoryPrice.Id = "lyeklhd10";
			salesHistoryPrice.CompanyCode = "e15283";
			var result = RedisCache.HashGet(salesHistoryPrice);
			foreach (var data in dataEntities)
			{
			}
		}

		[RedisKey("mymooo-product-number", 1)]
		public class SalesHistoryPrice
		{
			private string productNumber = string.Empty;

			[RedisMainField]
			public string CompanyCode { get; set; } = string.Empty;
			[RedisMainField(2)]
			public string PriceSource { get; } = "history";
			[RedisPrimaryKey]
			public string Id { get; set; } = string.Empty;
			public string ProductNumber
			{
				get => productNumber;
				set
				{
					productNumber = value;
					this.Id = productNumber.Replace("-", "").Trim().ToLower();
				}
			}
			public DateTime AuditTime { get; set; }
			public long SmallId { get; set; }
			public string SmallCode { get; set; } = string.Empty;
			public string SmallName { get; set; } = string.Empty;
			public string SalesOrderNo { get; set; } = string.Empty;
			public long ProductId { get; set; }
			public long ProductTypeId { get; set; }
			public decimal OriginalPrice { get; set; }
			public decimal TaxPrice { get; set; }
			public int DeliveDays { get; set; }
			public decimal LevelDiscount { get; set; }
			public decimal QtyDiscount { get; set; }
		}
	}
}
