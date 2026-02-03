using System;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.SalesManagement;

namespace Kingdee.Mymooo.Core.BaseManagement
{
	public class MaterialInfo : BaseInfo
	{
		public MaterialInfo(string code, string name)
		{
			this.NumberFieldName = "FNumber";
			this.DocumentStatusFieldName = "FDocumentStatus";
			this.ForbidStatusFieldName = "FForbidStatus";
			this.IdFieldNmber = "FMATERIALID";
			this.NumberKDDbType = KDDbType.String;
			this.OrgNumberFieldName = "FUseOrgId";
			this.MasterFieldName = "FMASTERID";
			this.Code = code;
			this.Name = name;
			this.FormId = "BD_MATERIAL";

			this.ErpClsID = 1;
			this.FBaseUnitId = "Pcs";
			this.FStoreUnitID = "Pcs";
			this.FPurchaseUnitId = "Pcs";
			this.FPurchasePriceUnitId = "Pcs";
			this.FSaleUnitId = "Pcs";
		}

		public string CustomerMaterialId { get; set; }

		public string UnitNumber { get; set; }
		public string UnitName { get; set; }
		public long ProductId { get; set; }

		/// <summary>
		/// 物料属性 1外购2自制
		/// </summary>
		public int ErpClsID { get; set; }
		/// <summary>
		/// 是否BOM子项
		/// </summary>
		public bool IsBom { get; set; } = false;

		/// <summary>
		/// 基本单位
		/// </summary>
		public string FBaseUnitId { get; set; }
		/// <summary>
		/// 库存单位
		/// </summary>
		public string FStoreUnitID { get; set; }
		/// <summary>
		/// 采购单位
		/// </summary>
		public string FPurchaseUnitId { get; set; }
		/// <summary>
		/// 采购计价单位
		/// </summary>
		public string FPurchasePriceUnitId { get; set; }
		/// <summary>
		/// 销售单位
		/// </summary>
		public string FSaleUnitId { get; set; }

		public SalesOrderBillRequest.Productsmallclass ProductSmallClass { get; set; }
		public string ShortNumber { get; set; }
		public string PriceType { get; set; }
		/// <summary>
		/// 长
		/// </summary>
		public decimal Length { get; set; } = 0;

		/// <summary>
		/// 宽
		/// </summary>
		public decimal Width { get; set; } = 0;

		/// <summary>
		/// 高
		/// </summary>
		public decimal Height { get; set; } = 0;

		/// <summary>
		/// 净重
		/// </summary>
		public decimal Weight { get; set; } = 0;

		/// <summary>
		/// 客户物料编码
		/// </summary>
		public string CustomerMaterialNumber { get; set; }

		/// <summary>
		/// 客户物料名称
		/// </summary>
		public string CustomerMaterialName { get; set; }
		/// <summary>
		/// 材质
		/// </summary>
		public string Textures { get; set; }
		/// <summary>
		/// 材型
		/// </summary>
		public string MaterialType { get; set; } = "";

		/// <summary>
		/// 体积
		/// </summary>
		public decimal Volume { get; set; }
		/// <summary>
		/// 重量单位
		/// </summary>
		public string WeightUnitid { get; set; } = "0";
		/// <summary>
		/// 尺寸单位
		/// </summary>
		public string VolumeUnitid { get; set; } = "0";
		/// <summary>
		/// 型号规格
		/// </summary>
		public string Specs { get; set; } = "";
	}

	public class MaterialUnitconvert : BaseInfo
	{
		public MaterialUnitconvert(string materialNumber)
		{
			this.NumberFieldName = "FMaterialId";
			this.DocumentStatusFieldName = "FDocumentStatus";
			this.ForbidStatusFieldName = "FForbidStatus";
			this.IdFieldNmber = "FUNITCONVERTRATEID";
			this.NumberKDDbType = KDDbType.String;
			this.OrgNumberFieldName = "FUseOrgId";
			this.Code = materialNumber;
			this.FormId = "BD_MATERIALUNITCONVERT";
		}
	}

	public class MaterialSafeStock
	{
		public Int64 MaterialId { get; set; }
		public string MaterialNumber { get; set; }
		public Int64 SafeStock { get; set; }
		public Int64 ReOrderGood { get; set; }
		public Int64 EconReOrderQty { get; set; }
		public Int64 MaxStock { get; set; }
		public Int64 MinPOQty { get; set; }
		public Int64 IncreaseQty { get; set; }
		public int OrderPolicy { get; set; }
	}
}
