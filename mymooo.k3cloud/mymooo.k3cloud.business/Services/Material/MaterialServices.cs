using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.Inventory;
using mymooo.k3cloud.core.MaterialModel;
using mymooo.k3cloud.core.ProductionModel;
using static Community.CsharpSqlite.Sqlite3;

namespace mymooo.k3cloud.business.Services.Material
{
	/// <summary>
	/// 物料服务
	/// </summary>
	/// <param name="kingdeeContent"></param>
	[AutoInject(InJectType.Scope)]
	public class MaterialServices(KingdeeContent kingdeeContent)
	{
		public ResponseMessage<MaterialInfoRequest> UpdateMaterials(MaterialInfoRequest request)
		{
			ResponseMessage<MaterialInfoRequest> result = new() { Data = null };
			if (!string.IsNullOrEmpty(request.Textures))
			{
				kingdeeContent.SqlSugar.Ado.ExecuteCommand("/*dialect*/ update T_BD_MATERIAL set FSHORTNUMBER = @FSHORTNUMBER,FPRODUCTID = @FPRODUCTID, FMymoooPriceType = @FMymoooPriceType,FTextures=@FTextures where FMASTERID = @FMASTERID",
					new
					{
						FMASTERID = request.MasterId,
						FSHORTNUMBER = request.ShortNumber,
						FPRODUCTID = request.ProductId,
						FMymoooPriceType = request.PriceType,
						FTextures = request.Textures
					});
			}
			kingdeeContent.SqlSugar.Ado.ExecuteCommand("/*dialect*/ update ml set FNAME = @FNAME,FSpecification=@FSpecification from T_BD_MATERIAL_L ml inner join T_BD_MATERIAL m on ml.FMATERIALID = m.FMATERIALID where m.FMASTERID = @FMASTERID",
				new
				{
					FMASTERID = request.MasterId,
					FNAME = request.Name,
					FSpecification = request.Specification
				});
			if ((request.Length) > 0 && request.Width > 0 && request.Height > 0)
			{
				kingdeeContent.SqlSugar.Ado.ExecuteCommand("/*dialect*/ update mb set FLENGTH = @FLENGTH,FWIDTH = @FWIDTH,FHEIGHT = @FHEIGHT,FNETWEIGHT = @FNETWEIGHT,FVOLUME = @FVOLUME,FWeightUnitid = @FWEIGHTUNITID,FVolumeUnitid = @FVOLUMEUNITID from t_BD_MaterialBase mb inner join T_BD_MATERIAL m on mb.FMATERIALID = m.FMATERIALID where m.FMASTERID = @FMASTERID",
				new
				{
					FMASTERID = request.MasterId,
					FLENGTH = request.Length,
					FWIDTH = request.Width,
					FHEIGHT = request.Height,
					FNETWEIGHT = request.Weight,
					FVOLUME = request.Volume,
					FWEIGHTUNITID = request.WeightUnitid,
					FVOLUMEUNITID = request.VolumeUnitid
				});
			}
			if (request.MaterialGroupId != 0)
			{
				kingdeeContent.SqlSugar.Ado.ExecuteCommand("/*dialect*/ update ms set FSALGROUP = @FSALGROUP from T_BD_MATERIALSALE ms inner join T_BD_MATERIAL m on ms.FMATERIALID = m.FMATERIALID where m.FMASTERID = @FMASTERID",
					new
					{
						FMASTERID = request.MasterId,
						FSALGROUP = request.MaterialGroupId
					});

				kingdeeContent.SqlSugar.Ado.ExecuteCommand("/*dialect*/ update T_BD_MATERIAL set FMATERIALGROUP = @FMATERIALGROUP where FMASTERID = @FMASTERID",
					new
					{
						FMASTERID = request.MasterId,
						FMATERIALGROUP = request.MaterialGroupId
					});
			}


			result.Data = request;
			result.Code = ResponseCode.Success;
			return result;
		}
	}
}
