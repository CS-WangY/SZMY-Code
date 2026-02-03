using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;

namespace Kingdee.Mymooo.ServicePlugIn.StkCountLoss
{
	/// <summary>
	/// 库存盘亏单审核验证可用库存
	/// </summary>
	public class AuditValidator : AbstractValidator
	{
		public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
		{
			//明细集合
			var detList = new List<GroupStock>();
			//已用的库存数量
			List<UsedGroupStock> usedStockList = new List<UsedGroupStock>();
			//先获取库存订单数据汇总
			foreach (var data in dataEntities)
			{
				var dynamicObject = data.DataEntity as DynamicObject;
				var entrys = data["BillEntry"] as DynamicObjectCollection;
				long orgId = Convert.ToInt64(dynamicObject["StockOrgId_Id"].ToString());
				//华东五部盘亏单不验证可用库存（add241016）
				if (orgId != 7401803)
				{
					foreach (var item in entrys)
					{
						detList.Add(new GroupStock
						{
							BillNO = Convert.ToString(data["BILLNO"]),
							StockOrgId = orgId,
							MasterID = Convert.ToInt64(((DynamicObject)item["MaterialId"])["msterID"]),
							MaterialCode = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]),
							StockID = Convert.ToInt64(((DynamicObject)item["StockId"])["id"]),
							StockName = Convert.ToString(((DynamicObject)item["StockId"])["Name"]),
							Qty = decimal.Parse(item["LossQty"].ToString()),//取盘亏数量
							DataEntity = data
						});
					}
				}
			}
			//验证库存
			//1.根据组织获取对应的库存数据。
			//2.循环出库订单一笔笔验证库存
			foreach (var orgId in detList.Select(o => o.StockOrgId).Distinct())
			{
				//根据物料ID和仓库ID分组汇总出库数量
				var groupList = detList.Where(x => x.StockOrgId == orgId).GroupBy(p => new { p.BillNO, p.MasterID, p.StockID }).Select(t => new
				{
					BillNO = t.Key.BillNO,
					MasterID = t.Key.MasterID,
					StockID = t.Key.StockID,
					MaterialCode = t.FirstOrDefault().MaterialCode,
					StockName = t.FirstOrDefault().StockName,
					QtySum = t.Sum(s => s.Qty)
				}).ToList();
				//获取物料可用库存(一个物料可能存在多个仓库)
				var stockQuantityList = StockQuantityServiceHelper.InventoryQtyVStatus(ctx, orgId, groupList.Select(x => x.MasterID).ToList());

				//循环订单号进行验证
				foreach (var billNO in detList.Where(x => x.StockOrgId == orgId).Select(o => o.BillNO).Distinct())
				{
					//循环验证库存
					foreach (var item in groupList.Where(x => x.BillNO == billNO))
					{
						//查询的可用库存
						var stock = stockQuantityList.FirstOrDefault(o => Convert.ToInt64(o.ItemMasterID).Equals(item.MasterID) && Convert.ToInt64(o.StockID).Equals(item.StockID));
						//批量循环已使用的可用库存
						var usedStock = usedStockList.FirstOrDefault(o => Convert.ToInt64(o.StockOrgId).Equals(orgId) && Convert.ToInt64(o.MasterID).Equals(item.MasterID) && Convert.ToInt64(o.StockID).Equals(item.StockID));
						var stockQty = stock == null ? 0 : stock.Qty;
						var usedQty = usedStock == null ? 0 : usedStock.Qty;
						if (stock == null || stockQty - usedQty < item.QtySum)
						{
							foreach (var item2 in detList.Where(x => (x.BillNO == billNO) && (x.MasterID == item.MasterID)).ToList())
							{
								validateContext.AddError(item2.DataEntity, new ValidationErrorInfo(
															 string.Empty,
															 item2.DataEntity["Id"].ToString(),
															 item2.DataEntity.DataEntityIndex,
															 item2.DataEntity.RowIndex,
															 item2.DataEntity["Id"].ToString(),
															 $"订单号[{billNO}]：物料编号[{item.MaterialCode}]库存不足,出库数合计[{item.QtySum.ToString("0.##")}],当前可用库存数量[{(stockQty - usedQty).ToString("0.##")}][{item.StockName}]",
															 $"可用库存验证",
															 ErrorLevel.FatalError));
							}
						}
						else
						{
							//记录累计使用库存
							if (usedStock == null)
							{
								usedStockList.Add(new UsedGroupStock
								{
									StockOrgId = orgId,
									MasterID = item.MasterID,
									StockID = item.StockID,
									Qty = item.QtySum
								});
							}
							else
							{
								usedStock.Qty += item.QtySum;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 选中的全部订单数量汇总
		/// </summary>
		private class GroupStock
		{

			/// <summary>
			/// 库存组织ID
			/// </summary>
			public long StockOrgId { get; set; }

			/// <summary>
			/// 订单号
			/// </summary>
			public string BillNO { get; set; }

			public ExtendedDataEntity DataEntity;

			/// <summary>
			/// 物料masterID
			/// </summary>
			public long MasterID { get; set; }

			/// <summary>
			/// 物料编号
			/// </summary>
			public string MaterialCode { get; set; }

			/// <summary>
			/// 库存ID
			/// </summary>
			public long StockID { get; set; }

			/// <summary>
			/// 库存名称
			/// </summary>
			public string StockName { get; set; }

			/// <summary>
			/// 数量
			/// </summary>
			public decimal Qty { get; set; } = 0;


		}

		/// <summary>
		/// 已用数量汇总
		/// </summary>
		private class UsedGroupStock
		{
			/// <summary>
			/// 库存组织ID
			/// </summary>
			public long StockOrgId { get; set; }
			/// <summary>
			/// 物料masterID
			/// </summary>
			public long MasterID { get; set; }

			/// <summary>
			/// 库存ID
			/// </summary>
			public long StockID { get; set; }

			/// <summary>
			/// 数量
			/// </summary>
			public decimal Qty { get; set; } = 0;
		}
	}
}
