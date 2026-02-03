using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Core.Warn;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.App.Core.BaseManagement
{
	public class MaterialService : IMaterialService
	{
		//FMaterialGroup 物料分组//T_BD_MasterialGroupTaxCode 事业部
		public MaterialSmallBusinessDivision GetMaterialSmallBusinessDivision(Context ctx, long materialId)
		{
			var sql = @"select ms.FMaterialGroup,g.FPARENTID,tax.FTAXCODEID,tax.FBUSINESSDIVISION,tax.FSupplyOrgId
                        from T_BD_MATERIAL ms
	                        left join T_BD_MATERIALGROUP g on ms.FMaterialGroup = g.FID
	                        left join T_BD_MasterialGroupTaxCode tax on ms.FMaterialGroup = tax.FMATERIALGROUP
                        where ms.FMATERIALID = @FMATERIALID";

			MaterialSmallBusinessDivision materialSmallBusinessDivision = new MaterialSmallBusinessDivision() { MaterialId = materialId };
			using (var dr = DBUtils.ExecuteReader(ctx, sql, new SqlParam("@FMATERIALID", KDDbType.Int64, materialId)))
			{
				while (dr.Read())
				{
					materialSmallBusinessDivision.SmallId = dr.GetValue<long>("FMaterialGroup");
					materialSmallBusinessDivision.ParentSmallId = dr.GetValue<long>("FPARENTID");
					materialSmallBusinessDivision.TaxCodeId = dr.GetValue<long>("FTAXCODEID");
					materialSmallBusinessDivision.BusinessDivision = dr.GetValue<string>("FBUSINESSDIVISION");
					materialSmallBusinessDivision.SupplyOrgId = dr.GetValue<long>("FSupplyOrgId");
				}
			}

			return materialSmallBusinessDivision;
		}

		public MaterialInfo[] TryGetOrAdds(Context ctx, MaterialInfo[] materials, List<long> useorgid)
		{
			List<MaterialInfo> materialInfos = new List<MaterialInfo>();
			foreach (var material in materials)
			{
				materialInfos.Add(TryGetOrAdd(ctx, material, useorgid));
			}
			return materialInfos.ToArray();
		}

		private static object lockObj = new object();
		public MaterialInfo TryGetOrAdd(Context ctx, MaterialInfo material, List<long> useorgid, bool IsAllocate = true)
		{
			lock (lockObj)
			{
				var orgId = material.UseOrgId;
				material.UseOrgId = 1;
				material = FormMetadataUtils.GetIdForNumber(ctx, material);
				material.UseOrgId = orgId;
				FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_MATERIAL") as FormMetadata;
				if (string.IsNullOrWhiteSpace(material.Specs))
				{
					material.Specs = material.Code;
				}
				if (material.MasterId == 0)
				{
					var billView = FormMetadataUtils.CreateBillView(ctx, "BD_MATERIAL");
					billView.Model.SetValue("FCreateOrgId", 1);
					billView.Model.SetValue("FNumber", material.Code);
					billView.Model.SetValue("FShortNumber", material.ShortNumber);
					billView.Model.SetValue("FProductId", material.ProductId);
					billView.Model.SetValue("FMymoooPriceType", material.PriceType);
					billView.Model.SetValue("FName", material.Name);
					billView.Model.SetValue("FSpecification", material.Specs);
					billView.Model.SetValue("FErpClsID", material.ErpClsID);
					billView.Model.SetValue("FIsProduce", true);
					billView.Model.SetValue("FIsPurchase", true);
					billView.Model.SetValue("FIsSubContract", true);
					billView.Model.SetValue("FIsAsset", true);
					billView.Model.SetValue("FIsMainPrd", true);
					//华南三部的物料
					//if (IsOrgHNTH(ctx, material.ProductSmallClass.Id))
					//{
					//	billView.Model.SetValue("FAgentSalReduceRate", 5);//代理销售减价比例(%) 默认5%  
					//}
					//else
					//{
					billView.Model.SetValue("FAgentSalReduceRate", 6);//代理销售减价比例(%) 默认6%  组织间结算的时候默认按终端销售价90%计算

					billView.Model.SetValue("FTaxRateId", 234); //默认税率 为 13%
																//来料校验
					billView.Model.SetValue("FCheckIncoming", true);
					billView.Model.SetItemValueByNumber("FBaseUnitId", material.FBaseUnitId, 0);
					billView.Model.SetItemValueByNumber("FStoreUnitID", material.FStoreUnitID, 0);
					billView.Model.SetItemValueByNumber("FPurchaseUnitId", material.FPurchaseUnitId, 0);
					billView.Model.SetItemValueByNumber("FSaleUnitId", material.FSaleUnitId, 0);
					billView.Model.SetItemValueByNumber("FPurchasePriceUnitId", material.FPurchasePriceUnitId, 0);

					billView.Model.SetItemValueByID("FWeightUnitId", material.WeightUnitid, 0);
					billView.Model.SetItemValueByID("FVolumeUnitId", material.VolumeUnitid, 0);

					//billView.Model.SetItemValueByNumber("FAssistantMaterialType", material.MaterialType, 0);
					billView.Model.SetValue("FTextures", material.Textures, 0);
					billView.Model.SetValue("FVolume", material.Volume);
					billView.Model.SetValue("FLENGTH", material.Length);
					billView.Model.SetValue("FWIDTH", material.Width);
					billView.Model.SetValue("FHEIGHT", material.Height);
					billView.Model.SetValue("FNETWEIGHT", material.Weight);
					if (material.ProductSmallClass != null)
					{
						if (material.ProductSmallClass.Id > 0)
						{
							billView.Model.SetValue("FMaterialGroup", material.ProductSmallClass.Id);
							billView.Model.SetValue("FSalGroup", material.ProductSmallClass.Id);
						}
					}
					this.SaveMaterial(ctx, material, meta, billView);
					if (IsAllocate)
					{
						this.MaterialAllocateUseOrg(ctx, material, meta, useorgid);
					}
					//清除释放网控
					billView.CommitNetworkCtrl();
					billView.InvokeFormOperation(FormOperationEnum.Close);
					billView.Close();
				}
				else
				{
					var request = new
					{
						MasterId = material.MasterId,
						MaterialId = material.Id,
						Code = material.Code,
						Name = material.Name,
						ShortNumber = material.ShortNumber,
						ProductId = material.ProductId,
						PriceType = material.PriceType,
						MaterialGroupId = material.ProductSmallClass.Id,
						Specification = material.Specs,
						Length = material.Length,
						Width = material.Width,
						Height = material.Height,
						Weight = material.Weight,
						Textures = material.Textures,
						MaterialType = material.MaterialType,
						Volume = material.Volume,
						WeightUnitid = material.WeightUnitid,
						VolumeUnitid = material.VolumeUnitid
					};
					var result = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Material/UpdateMaterials", JsonConvertUtils.SerializeObject(request));
					//var returninfo = JsonConvertUtils.DeserializeObject<ResponseMessage<DynamicObject>>(result);
					//if (!returninfo.Code.EqualsIgnoreCase("success"))
					//{
					//	throw new Exception("更新物料失败：" + returninfo.ErrorMessage);
					//}
				}

				//保存成功后在获取一次。
				material = FormMetadataUtils.GetIdForNumber(ctx, material);
				return material;
			}
		}
		public MaterialInfo TryBomGetOrAdd(Context ctx, MaterialInfo material)
		{
			var orgId = material.UseOrgId;
			material.UseOrgId = 1;
			material = FormMetadataUtils.GetIdForNumber(ctx, material);
			material.UseOrgId = orgId;
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_MATERIAL") as FormMetadata;
			if (material.MasterId == 0)
			{
				var billView = FormMetadataUtils.CreateBillView(ctx, "BD_MATERIAL");
				billView.Model.SetValue("FCreateOrgId", 1);
				billView.Model.SetValue("FNumber", material.Code);
				billView.Model.SetValue("FShortNumber", material.ShortNumber);
				billView.Model.SetValue("FProductId", material.ProductId);
				billView.Model.SetValue("FMymoooPriceType", material.PriceType);
				billView.Model.SetValue("FName", material.Name);
				billView.Model.SetValue("FSpecification", material.Code);
				billView.Model.SetValue("FErpClsID", material.ErpClsID);
				billView.Model.SetValue("FIsProduce", true);
				billView.Model.SetValue("FIsPurchase", true);
				billView.Model.SetValue("FIsSubContract", true);
				billView.Model.SetValue("FIsAsset", true);
				billView.Model.SetValue("FIsMainPrd", true);
				billView.Model.SetValue("FTextures", material.Textures);
				billView.Model.SetValue("FVolume", material.Volume);
				billView.Model.SetValue("FLENGTH", material.Length);
				billView.Model.SetValue("FWIDTH", material.Width);
				billView.Model.SetValue("FHEIGHT", material.Height);
				billView.Model.SetValue("FNETWEIGHT", material.Weight);
				//华南三部的物料
				//if (IsOrgHNTH(ctx, material.ProductSmallClass.Id))
				//{
				//	billView.Model.SetValue("FAgentSalReduceRate", 5);//代理销售减价比例(%) 默认5%  
				//}
				//else
				//{
				billView.Model.SetValue("FAgentSalReduceRate", 6);//代理销售减价比例(%) 默认6%  组织间结算的时候默认按终端销售价90%计算
																  //}

				billView.Model.SetValue("FTaxRateId", 234); //默认税率 为 13%
															//来料校验
				billView.Model.SetValue("FCheckIncoming", true);
				billView.Model.SetItemValueByNumber("FBaseUnitId", material.FBaseUnitId, 0);
				billView.Model.SetItemValueByNumber("FStoreUnitID", material.FStoreUnitID, 0);
				billView.Model.SetItemValueByNumber("FPurchaseUnitId", material.FPurchaseUnitId, 0);
				billView.Model.SetItemValueByNumber("FSaleUnitId", material.FSaleUnitId, 0);
				billView.Model.SetItemValueByNumber("FPurchasePriceUnitId", material.FPurchasePriceUnitId, 0);

				billView.Model.SetItemValueByID("FWeightUnitId", material.WeightUnitid, 0);
				billView.Model.SetItemValueByID("FVolumeUnitId", material.VolumeUnitid, 0);

				billView.Model.SetValue("FTextures", material.Textures);

				if (material.ProductSmallClass != null)
				{
					billView.Model.SetValue("FMaterialGroup", material.ProductSmallClass.Id);
					billView.Model.SetValue("FSalGroup", material.ProductSmallClass.Id);
				}
				this.SaveMaterial(ctx, material, meta, billView);
				billView.CommitNetworkCtrl();
				billView.InvokeFormOperation(FormOperationEnum.Close);
				billView.Close();
			}
			else
			{
				List<SqlObject> list = new List<SqlObject>()
				{
					new SqlObject("/*dialect*/ update T_BD_MATERIAL set FSHORTNUMBER = @FSHORTNUMBER,FPRODUCTID = @FPRODUCTID, FMymoooPriceType = @FMymoooPriceType where FMASTERID = @FMASTERID",
						new List<SqlParam>(){ new SqlParam("@FSHORTNUMBER", KDDbType.String, material.ShortNumber??"")
							,new SqlParam("@FPRODUCTID", KDDbType.Int64, material.ProductId)
							,new SqlParam("@FMymoooPriceType", KDDbType.String, material.PriceType??"0")
							,new SqlParam("@FMASTERID", KDDbType.Int64, material.MasterId)})
					, new SqlObject("/*dialect*/ update ml set FNAME = @FNAME from T_BD_MATERIAL_L ml inner join T_BD_MATERIAL m on ml.FMATERIALID = m.FMATERIALID where m.FMASTERID = @FMASTERID",
						new List<SqlParam>(){ new SqlParam("@FNAME", KDDbType.String, material.Name)
							,new SqlParam("@FMASTERID", KDDbType.Int64, material.MasterId)})
					 , new SqlObject("/*dialect*/ update mb set FLENGTH = @FLENGTH,FWIDTH = @FWIDTH,FHEIGHT = @FHEIGHT,FNETWEIGHT = @FNETWEIGHT,FVOLUME = @FVOLUME,FWeightUnitid = @FWEIGHTUNITID,FVolumeUnitid = @FVOLUMEUNITID from t_BD_MaterialBase mb inner join T_BD_MATERIAL m on mb.FMATERIALID = m.FMATERIALID where m.FMASTERID = @FMASTERID",
						new List<SqlParam>(){ new SqlParam("@FLENGTH", KDDbType.Decimal, material.Length)
						,new SqlParam("@FWIDTH", KDDbType.Decimal, material.Width)
						,new SqlParam("@FHEIGHT", KDDbType.Decimal, material.Height)
						,new SqlParam("@FNETWEIGHT", KDDbType.Decimal, material.Weight)
						,new SqlParam("@FVOLUME", KDDbType.Decimal, material.Volume)
						,new SqlParam("@FVOLUMEUNITID", KDDbType.Decimal, material.VolumeUnitid)
						,new SqlParam("@FWEIGHTUNITID", KDDbType.Decimal, material.WeightUnitid)
						,new SqlParam("@FMASTERID", KDDbType.Int64, material.MasterId)
						})
				};
				if (!string.IsNullOrWhiteSpace(material.Textures))
				{
					list.Add(new SqlObject("/*dialect*/ update T_BD_MATERIAL set FTextures=@FTextures where FMASTERID = @FMASTERID",
						new List<SqlParam>(){ new SqlParam("@FTextures", KDDbType.String, material.Textures)
							,new SqlParam("@FMASTERID", KDDbType.Int64, material.MasterId)}));
				}
				if (material.ProductSmallClass != null)
				{
					list.Add(new SqlObject("/*dialect*/ update ms set FSALGROUP = @FSALGROUP from T_BD_MATERIALSALE ms inner join T_BD_MATERIAL m on ms.FMATERIALID = m.FMATERIALID where m.FMASTERID = @FMASTERID",
						new List<SqlParam>(){ new SqlParam("@FSALGROUP", KDDbType.String, material.ProductSmallClass.Id)
							,new SqlParam("@FMASTERID", KDDbType.Int64, material.MasterId)}));

					list.Add(new SqlObject("/*dialect*/ update T_BD_MATERIAL set FMATERIALGROUP = @FMATERIALGROUP where FMASTERID = @FMASTERID",
						new List<SqlParam>(){ new SqlParam("@FMATERIALGROUP", KDDbType.Int64, material.ProductSmallClass.Id)
							,new SqlParam("@FMASTERID", KDDbType.Int64, material.MasterId)}));
				}
				DBUtils.ExecuteBatch(ctx, list);
			}
			//this.MaterialAllocate(ctx, material, meta);

			//保存成功后在获取一次。
			material = FormMetadataUtils.GetIdForNumber(ctx, material);
			return material;
		}

		private void SaveMaterial(Context ctx, MaterialInfo material, FormMetadata meta, IBillView billView)
		{
			SaveService saveService = new SaveService();
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{
				var oper = saveService.Save(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
				if (!oper.IsSuccess)
				{
					if (oper.ValidationErrors.Count > 0)
					{
						throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
					}
					else
					{
						throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
					}
				}
				material.Id = Convert.ToInt64(billView.Model.DataObject["Id"]);
				material.MasterId = material.Id;
				//新增物料单位换算库存单位与采购单位不一致
				if (material.FStoreUnitID != material.FPurchaseUnitId)
				{
					//1.两个单位不一致的情况下,需要判断2个单位是否在一个单位组,如果是在一个单位组,是不需要创建浮动换算关系的,
					//2.两个单位不一致的情况下,而且不在一个单位组,判断2个单位是否都是基本单位,如果是,则创建浮动单位换算关系,
					//如果不是,则根据单位找到对应的基本单位来创建浮动换算.
					var sql = $"select FISBASEUNIT,FUNITGROUPID from T_BD_UNIT where FNUMBER in ('{material.FStoreUnitID}','{material.FPurchaseUnitId}')";
					var unit = DBUtils.ExecuteDynamicObject(ctx, sql);
					if (unit.Count < 2)
					{
						throw new Exception("库存单位或者采购单位在系统中不存在!");
					}
					if (Convert.ToInt64(unit[0]["FUNITGROUPID"]) != Convert.ToInt64(unit[1]["FUNITGROUPID"]))
					{
						if (Convert.ToString(unit[0]["FISBASEUNIT"]) != "1")
						{
							//throw new Exception("库存单位或者采购单位在系统中不是基本单位!");
							material.FStoreUnitID = GetUnitGroup(ctx, material.FStoreUnitID);
						}
						if (Convert.ToString(unit[1]["FISBASEUNIT"]) != "1")
						{
							material.FPurchaseUnitId = GetUnitGroup(ctx, material.FPurchaseUnitId);
						}
						AddMaterialUnitconvert(ctx, material);
					}
				}
				//提交审核
				SubmitService submitService = new SubmitService();
				var submitResult = submitService.Submit(ctx, billView.BusinessInfo, new string[] { material.Id.ToString() }, "Submit", operateOption);
				if (!submitResult.IsSuccess)
				{
					if (submitResult.ValidationErrors.Count > 0)
					{
						throw new Exception(string.Join(";", submitResult.ValidationErrors.Select(p => p.Message)));
					}
					else
					{
						throw new Exception(string.Join(";", submitResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
					}
				}
				else
				{
					AuditService auditService = new AuditService();
					var auditResult = auditService.Audit(ctx, billView.BusinessInfo, new string[] { material.Id.ToString() }, operateOption);
					if (!auditResult.IsSuccess)
					{
						if (auditResult.ValidationErrors.Count > 0)
						{
							throw new Exception(string.Join(";", auditResult.ValidationErrors.Select(p => p.Message)));
						}
						else
						{
							throw new Exception(string.Join(";", auditResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
						}
					}
				}
				cope.Complete();
			}
		}

		private void MaterialAllocateUseOrg(Context ctx, MaterialInfo material, FormMetadata meta, List<long> orglistid)
		{
			ViewService viewService = new ViewService();
			List<DynamicObject> dynamicObjects = new List<DynamicObject>();

			AllocateService allocateService = new AllocateService();

			foreach (var useorgid in orglistid)
			{
				if (useorgid == 0)
				{
					continue;
				}
				AllocateParameter allocateParameter = new AllocateParameter(meta.BusinessInfo, meta.InheritPath, 1, BOSEnums.Enu_AllocateType.Allocate, OperationNumberConst.OperationNumber_Allocate)
				{
					PkId = new List<object>() { material.MasterId },
					AutoSubmitAndAudit = true,
					AllocateUserId = ctx.UserId,
					DestOrgId = useorgid
				};
				var oper = allocateService.Allocate(ctx, allocateParameter);
				if (!oper.IsSuccess)
				{
					if (oper.ValidationErrors.Count > 0)
					{
						throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
					}
					else
					{
						throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
					}
				}
			}
			//Task.Run(() => MaterialAllocateAll(ctx, material, meta)).Wait();
			//await Task.WhenAll(MaterialAllocateAll(ctx, material, meta)).ConfigureAwait(false); // 等待所有任务完成
		}
		public ResponseMessage<dynamic> MaterialAllocate(Context ctx, List<long> materiallist)
		{
			ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();
			try
			{
				foreach (var materialid in materiallist)
				{
					List<DynamicObject> dynamicObjects = new List<DynamicObject>();
					var editBillView = FormMetadataUtils.CreateBillView(ctx, "BD_MATERIAL");
					//查找需要分配的组织
					var sql = @"select e.FTARGETORGID,ol.FNAME,org.FNUMBER 
from T_ORG_BDCtrlPolicy o
	inner join T_ORG_BDCtrlTOrgEntry e on o.FPOLICYID = e.FPOLICYID
    inner join T_ORG_ORGANIZATIONS org on e.FTARGETORGID=org.FORGID
	inner join T_ORG_ORGANIZATIONS_L ol on e.FTARGETORGID = ol.FORGID and ol.FLOCALEID = 2052
where o.FBASEDATATYPEID = 'BD_MATERIAL' and not exists (select 1 from T_BD_MATERIAL m where e.FTARGETORGID = m.FUSEORGID and m.FMASTERID = @FMASTERID)";
					var allocateOrgs = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FMASTERID", KDDbType.Int64, materialid));
					AllocateService allocateService = new AllocateService();
					foreach (var allocateOrg in allocateOrgs)
					{
						AllocateParameter allocateParameter = new AllocateParameter(editBillView.BusinessInfo, "", 1, BOSEnums.Enu_AllocateType.Allocate, OperationNumberConst.OperationNumber_Allocate)
						{
							PkId = new List<object>() { materialid },
							AutoSubmitAndAudit = true,
							AllocateUserId = ctx.UserId,
							DestOrgId = Convert.ToInt64(allocateOrg["FTARGETORGID"]),
							DestOrgName = Convert.ToString(allocateOrg["FNAME"])
						};
						var oper = allocateService.Allocate(ctx, allocateParameter);
						if (!oper.IsSuccess)
						{
							if (oper.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
					}
				}

				result.Code = ResponseCode.Success;
				result.Message = "物料分配成功";
			}
			catch (Exception ex)
			{
				result.Code = ResponseCode.Exception;
				result.Message = ex.Message;
			}
			return result;
		}

		public void MaterialAllocateToAll(Context ctx, List<long> materiallist)
		{
			KafkaProducerService kafkaProducer = new KafkaProducerService();
			List<RabbitMQMessage> messages = new List<RabbitMQMessage>
				{
					new RabbitMQMessage()
					{
						Exchange = "materialManagement",
						Routingkey = "AllocateMaterial",
						Keyword = "",
						Message = JsonConvertUtils.SerializeObject(materiallist)
					}
				};
			kafkaProducer.AddMessage(ctx, messages.ToArray());
		}

		public void AddMaterialUnitconvert(Context ctx, MaterialInfo material)
		{
			var UnitConvert = FormMetadataUtils.GetIdForNumber(ctx, new MaterialUnitconvert(material.Id.ToString()));
			if (UnitConvert.Id == 0)
			{
				SaveService saveService = new SaveService();
				var operateOption = OperateOption.Create();
				operateOption.SetIgnoreWarning(true);

				var unitcv = FormMetadataUtils.CreateBillView(ctx, "BD_MATERIALUNITCONVERT");
				unitcv.Model.SetValue("FMaterialId", material.Id);
				unitcv.Model.SetItemValueByNumber("FCurrentUnitId", material.FPurchaseUnitId, 0);
				unitcv.Model.SetItemValueByNumber("FDestUnitId", material.FStoreUnitID, 0);
				unitcv.Model.SetValue("FConvertType", 1);
				var oper = saveService.SaveAndAudit(ctx, unitcv.BusinessInfo, new DynamicObject[] { unitcv.Model.DataObject }, operateOption);
				if (!oper.IsSuccess)
				{
					if (oper.ValidationErrors.Count > 0)
					{
						throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
					}
					else
					{
						throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
					}
				}
			}
		}
		public string GetUnitGroup(Context ctx, string UnitID)
		{
			var unitNumber = "";
			var sql = $@"select t1.* from T_BD_UNIT t1 LEFT JOIN T_BD_UNIT t2 ON t1.FUNITGROUPID=t2.FUNITGROUPID
                        WHERE t2.FNUMBER='{UnitID}' AND t1.FISBASEUNIT=1";
			var dr = DBUtils.ExecuteDynamicObject(ctx, sql);
			foreach (var item in dr)
			{
				unitNumber = Convert.ToString(item["FNUMBER"]);
			}

			if (unitNumber.IsNullOrEmptyOrWhiteSpace())
			{
				throw new Exception("未能找到相应单位组的基本单位!");
			}
			else
			{
				return unitNumber;
			}
		}


		public ResponseMessage<dynamic> GroupSave(Context ctx, List<SalesOrderBillRequest.Productsmallclass> datas)
		{
			ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();
			try
			{
				var operateOption = OperateOption.Create();
				operateOption.SetIgnoreWarning(true);
				MymoooBusinessDataService service = new MymoooBusinessDataService();

				//更新物料的小类
				var msterialGroupSql = @"/*dialect*/
                MERGE INTO T_BD_MATERIALGROUP as T
                USING (SELECT @FID AS FID, @FNUMBER AS FNUMBER,@FGROUPID as FGROUPID,@FPARENTID as FPARENTID) AS S
                ON T.FID = S.FID
                WHEN MATCHED THEN
                    UPDATE SET T.FNUMBER = S.FNUMBER, T.FPARENTID = S.FPARENTID
                WHEN NOT MATCHED THEN
                    INSERT(FID, FNUMBER,FPARENTID,FGROUPID,FShowPOS,FShowBigScreen) VALUES(S.FID, S.FNUMBER,s.FPARENTID,s.FGROUPID,'1','1');";
				var msterialGrouplSql = @"/*dialect*/
                MERGE INTO T_BD_MATERIALGROUP_L as T
                USING (SELECT @FPKID as FPKID,@FID AS FID, @FNAME AS FNAME) AS S
                ON T.FPKID = S.FPKID
                WHEN MATCHED THEN
                    UPDATE SET T.FNAME = S.FNAME
                WHEN NOT MATCHED THEN
                    INSERT(FID, FNAME,FPKID,FLOCALEID) VALUES(S.FID, S.FNAME,s.FPKID,2052);";
				var salesGroupSql = @"/*dialect*/
                MERGE INTO T_BD_MATERIALSALGROUP as T
                USING (SELECT @FID AS FID, @FNUMBER AS FNUMBER,@FGROUPID as FGROUPID,@FPARENTID as FPARENTID) AS S
                ON T.FID = S.FID
                WHEN MATCHED THEN
                    UPDATE SET T.FNUMBER = S.FNUMBER, T.FPARENTID = S.FPARENTID
                WHEN NOT MATCHED THEN
                    INSERT(FID, FNUMBER,FPARENTID,FGROUPID,FShowPOS,FShowBigScreen) VALUES(S.FID, S.FNUMBER,s.FPARENTID,s.FGROUPID,'1','1');";
				var salesGrouplSql = @"/*dialect*/
                MERGE INTO T_BD_MATERIALSALGROUP_L as T
                USING (SELECT @FPKID as FPKID,@FID AS FID, @FNAME AS FNAME ) AS S
                ON T.FPKID = S.FPKID
                WHEN MATCHED THEN
                    UPDATE SET T.FNAME = S.FNAME
                WHEN NOT MATCHED THEN
                    INSERT(FID, FNAME,FPKID,FLOCALEID) VALUES(S.FID, S.FNAME,s.FPKID,2052);";
				List<SqlObject> sqlObjects = new List<SqlObject>();
				foreach (var small in datas)
				{
					sqlObjects.Add(new SqlObject(msterialGroupSql, new List<SqlParam>()
				{
					new SqlParam("@FID", KDDbType.Int64, small.Id),
					new SqlParam("@FNUMBER", KDDbType.String, small.Code),
					new SqlParam("@FGROUPID", KDDbType.String, "d82dac3f-43fa-4fce-9b7f-9e3280abd0d8"),
					new SqlParam("@FPARENTID", KDDbType.Int64, small.ParentId)
				}));
					sqlObjects.Add(new SqlObject(msterialGrouplSql, new List<SqlParam>()
				{
					new SqlParam("@FID", KDDbType.Int64, small.Id),
					new SqlParam("@FPKID", KDDbType.Int64, small.Id),
					new SqlParam("@FNAME", KDDbType.String, small.Name)
				}));
					if (small.IsPublish)
					{
						sqlObjects.Add(new SqlObject(salesGroupSql, new List<SqlParam>()
					{
						new SqlParam("@FID", KDDbType.Int64, small.Id),
						new SqlParam("@FNUMBER", KDDbType.String, small.Code),
						new SqlParam("@FGROUPID", KDDbType.String, "9ee59d7c60b84bb393795c6f2a8f5657"),
						new SqlParam("@FPARENTID", KDDbType.Int64, small.ParentId)
					}));
						sqlObjects.Add(new SqlObject(salesGrouplSql, new List<SqlParam>()
					{
						new SqlParam("@FID", KDDbType.Int64, small.Id),
						new SqlParam("@FPKID", KDDbType.Int64, small.Id),
						new SqlParam("@FNAME", KDDbType.String, small.Name)
					}));
					}
				}
				DBUtils.ExecuteBatch(ctx, sqlObjects);

				//更新产品小类对应税收分类编码和供货组织对应事业部
				foreach (var small in datas.Where(x => (x.IsPublish == true && x.ParentId != 0)))
				{
					//1.更新产品小类对应税收分类编码
					List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@MATERIALGROUP", KDDbType.Int64, small.Id) };
					string sql = @"/*dialect*/select top 1 FID from T_BD_MasterialGroupTaxCode where FMATERIALGROUP=@MATERIALGROUP ";
					long masterialGroupTaxCodeID = DBServiceHelper.ExecuteScalar(ctx, sql, 0, paramList: pars.ToArray());
					//新增
					if (masterialGroupTaxCodeID == 0)
					{
						var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_BD_MasterialGroupTaxCode");
						List<DynamicObject> dynamicObjects = new List<DynamicObject>();
						billView.Model.SetValue("FMaterialGroup", small.Id);
						billView.Model.SetValue("FBusinessDivision", small.BusinessDivisionId);
						billView.Model.SetValue("FSupplyOrgId", small.SupplyOrgId);
						dynamicObjects.Add(billView.Model.DataObject);
						var oper = service.SaveAndAuditBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
						billView.CommitNetworkCtrl();
						billView.InvokeFormOperation(FormOperationEnum.Close);
						billView.Close();

						if (!oper.IsSuccess)
						{
							if (oper.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
					}
					else
					{
						//修改
						var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_BD_MasterialGroupTaxCode", masterialGroupTaxCodeID);
						List<DynamicObject> dynamicObjects = new List<DynamicObject>();
						billView.Model.SetValue("FBusinessDivision", small.BusinessDivisionId);
						billView.Model.SetValue("FSupplyOrgId", small.SupplyOrgId);
						dynamicObjects.Add(billView.Model.DataObject);
						var oper = service.SaveBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
						billView.CommitNetworkCtrl();
						billView.InvokeFormOperation(FormOperationEnum.Close);
						billView.Close();
						if (!oper.IsSuccess)
						{
							if (oper.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
					}

					//2.新增或者修改供货组织对应事业部
					foreach (var supply in small.SupplyOrgs)
					{
						//2.更新供货组织对应事业部
						List<SqlParam> pars2 = new List<SqlParam>() { new SqlParam("@SUPPLYORGID", KDDbType.Int64, supply.SupplyOrgId) };
						string sql2 = @"/*dialect*/select top 1 FID from T_BD_SupplyOrgToBD where FSUPPLYORGID=@SUPPLYORGID ";
						long supplyOrgToBDId = DBServiceHelper.ExecuteScalar(ctx, sql2, 0, paramList: pars2.ToArray());
						//新增
						if (supplyOrgToBDId == 0)
						{
							var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_BD_SupplyOrgBusinessDivision");
							List<DynamicObject> dynamicObjects = new List<DynamicObject>();
							billView.Model.SetValue("FBusinessDivision", supply.BusinessDivisionId);
							billView.Model.SetValue("FSupplyOrgId", supply.SupplyOrgId);
							billView.Model.SetValue("FIsDefault", supply.IsDefault);
							dynamicObjects.Add(billView.Model.DataObject);
							var oper = service.SaveBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
							billView.CommitNetworkCtrl();
							billView.InvokeFormOperation(FormOperationEnum.Close);
							billView.Close();
							if (!oper.IsSuccess)
							{
								if (oper.ValidationErrors.Count > 0)
								{
									throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
								}
								else
								{
									throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
								}
							}
						}
						else
						{
							//修改
							var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_BD_SupplyOrgBusinessDivision", supplyOrgToBDId);
							List<DynamicObject> dynamicObjects = new List<DynamicObject>();
							billView.Model.SetValue("FBusinessDivision", supply.BusinessDivisionId);
							billView.Model.SetValue("FIsDefault", supply.IsDefault);
							dynamicObjects.Add(billView.Model.DataObject);
							var oper = service.SaveBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
							billView.CommitNetworkCtrl();
							billView.InvokeFormOperation(FormOperationEnum.Close);
							billView.Close();
							if (!oper.IsSuccess)
							{
								if (oper.ValidationErrors.Count > 0)
								{
									throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
								}
								else
								{
									throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
								}
							}
						}
					}
				}
				result.Code = ResponseCode.Success;
			}
			catch (Exception ex)
			{
				result.Code = ResponseCode.Exception;
				result.Message = ex.Message;
			}
			return result;
		}

		public ResponseMessage<dynamic> CreateBOM(Context ctx, DispatchInfo request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();
				var sqlIsExist = @"SELECT * FROM dbo.T_ENG_BOM t1 INNER JOIN dbo.T_BD_MATERIAL t2 
                                ON t1.FMATERIALID=t2.FMATERIALID 
                                WHERE t2.FNUMBER = '" + request.part.drawNum + "'";
				var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sqlIsExist);
				if (datas.Count > 0)
				{
					response.Code = ResponseCode.ExistsData;
					response.Message = "当前BOM关系已存在";
					response.Data = string.Empty;
					return response;
				}

				FormMetadata meta = MetaDataServiceHelper.Load(ctx, "ENG_BOM") as FormMetadata;
				var billView = FormMetadataUtils.CreateBillView(ctx, "ENG_BOM");
				billView.Model.SetValue("FCreateOrgId", 1);
				billView.Model.SetItemValueByNumber("FMATERIALID", request.part.drawNum, 0);
				billView.Model.SetItemValueByNumber("FUNITID", "Pcs", 0);
				billView.Model.SetItemValueByNumber("FMATERIALIDCHILD", request.bom.specification, 0);
				billView.Model.SetValue("FCHILDITEMNAME", request.bom.name);

				SaveService saveService = new SaveService();
				var operateOption = OperateOption.Create();
				operateOption.SetIgnoreWarning(true);
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{

					var oper = saveService.SaveAndAudit(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
					if (!oper.IsSuccess)
					{
						if (oper.ValidationErrors.Count > 0)
						{
							throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
						}
						else
						{
							throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
						}
					}
					//查找需要分配的组织
					var sql = @"select e.FTARGETORGID,ol.FNAME 
                            from T_ORG_BDCtrlPolicy o
                            inner join T_ORG_BDCtrlTOrgEntry e on o.FPOLICYID = e.FPOLICYID
                            inner join T_ORG_ORGANIZATIONS_L ol on e.FTARGETORGID = ol.FORGID and ol.FLOCALEID = 2052
                            where o.FBASEDATATYPEID = 'ENG_BOM'";
					var allocateOrgs = DBUtils.ExecuteDynamicObject(ctx, sql);
					AllocateService allocateService = new AllocateService();
					foreach (var allocateOrg in allocateOrgs)
					{
						AllocateParameter allocateParameter = new AllocateParameter(billView.BusinessInfo, meta.InheritPath, 1, BOSEnums.Enu_AllocateType.Allocate, OperationNumberConst.OperationNumber_Allocate)
						{
							PkId = new List<object>() { Convert.ToInt64(billView.Model.DataObject["Id"]) },
							AutoSubmitAndAudit = true,
							AllocateUserId = ctx.UserId,
							DestOrgId = Convert.ToInt64(allocateOrg["FTARGETORGID"]),
							DestOrgName = Convert.ToString(allocateOrg["FNAME"])
						};
						oper = allocateService.Allocate(ctx, allocateParameter);
						if (!oper.IsSuccess)
						{
							if (oper.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
					}

					cope.Complete();
				}
				response.Code = ResponseCode.Success;
				response.Message = string.Empty;
				response.Data = string.Empty;
				return response;
			}
			catch (Exception e)
			{
				response.Code = ResponseCode.Exception;
				response.Message = e.Message;
				response.Data = string.Empty;
				return response;
			}

		}

		public MaterialInfo[] TryGetOrAddCustMsterials(Context ctx, CustomerInfo customer, params MaterialInfo[] materials)
		{
			var sql = "select FID from T_SAL_CUSTMATMAPPING where FCUSTOMERID = @FCUSTOMERID";
			var id = DBUtils.ExecuteScalar<long>(ctx, sql, 0, new SqlParam("@FCUSTOMERID", KDDbType.Int64, customer.Id));
			SequenceReader sequenceReader = new SequenceReader(ctx);
			List<MaterialInfo> notExistsmaterials = new List<MaterialInfo>();
			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{
				if (id == 0)
				{
					id = sequenceReader.GetSequence<long>("T_SAL_CUSTMATMAPPING", 1).FirstOrDefault();
					var insertSql = @"insert into T_SAL_CUSTMATMAPPING(FID,FBILLNO,FSALEORGID,FCUSTOMERID,FCREATORID,FCREATEDATE,FMODIFIERID,FMODIFYDATE,FDOCUMENTSTATUS,FUSEORGID,FISOLDVERSION)
	values(@FID,@FBILLNO,1,@FCUSTOMERID,100457,getdate(),100457,getdate(),'A',1,'1')";
					List<SqlParam> insertParams = new List<SqlParam>()
					{
						new SqlParam("@FID", KDDbType.Int64, id),
						new SqlParam("@FBILLNO", KDDbType.String, customer.Code),
						new SqlParam("@FCUSTOMERID", KDDbType.Int64, customer.Id)
					};
					DBUtils.Execute(ctx, insertSql, insertParams);

					insertSql = @"insert into T_SAL_CUSTMATMAPPING_L(FPKID,FID,FLOCALEID,FNAME) values(@FPKID,@FID,2052,N'客户型号对照表')";
					insertParams = new List<SqlParam>()
					{
						new SqlParam("@FID", KDDbType.Int64, id),
						new SqlParam("@FPKID", KDDbType.Int64, sequenceReader.GetSequence<long>("T_SAL_CUSTMATMAPPING_L", 1).FirstOrDefault())
					};
					DBUtils.Execute(ctx, insertSql, insertParams);
					//只分配深圳蚂蚁  华南五部  华东三部
					insertSql = @"/*dialect*/
insert into T_SAL_CUSTMATMAPPING_ISSUE(FISSUEPKID,FID,FISSUEORGID,FISSUEMAN,FISSUEDATE) 
select NEWID(),@FID,e.FTARGETORGID,152855,getdate()
from T_ORG_BDCtrlPolicy o
	inner join T_ORG_BDCtrlTOrgEntry e on o.FPOLICYID = e.FPOLICYID
    inner join T_ORG_ORGANIZATIONS org on e.FTARGETORGID=org.FORGID
	inner join T_ORG_ORGANIZATIONS_L ol on e.FTARGETORGID = ol.FORGID and ol.FLOCALEID = 2052
where o.FBASEDATATYPEID = 'BD_MATERIAL'";
					DBUtils.Execute(ctx, insertSql, new SqlParam("@FID", KDDbType.Int64, id));
				}
				sql = "select FID from T_V_SAL_CUSTMATMAPPING where fcustomerid = @fcustomerid and FMATERIALID = @FMATERIALID and fnumber = @fnumber and FUSEORGID = @FUSEORGID";
				foreach (var material in materials)
				{
					//if (material.Code == material.CustomerMaterialNumber && material.Name == material.CustomerMaterialName)
					//{
					//    continue;
					//}
					List<SqlParam> sqlParams = new List<SqlParam>()
					{
						new SqlParam("@FMATERIALID", KDDbType.Int64, material.Id),
						new SqlParam("@fnumber", KDDbType.String, material.CustomerMaterialNumber),
						new SqlParam("@FCUSTOMERID", KDDbType.Int64, customer.Id),
						new SqlParam("@FUSEORGID", KDDbType.Int64, material.UseOrgId)
					};

					material.CustomerMaterialId = DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, sqlParams.ToArray());

					if (string.IsNullOrWhiteSpace(material.CustomerMaterialId))
					{
						notExistsmaterials.Add(material);
					}
				}

				if (notExistsmaterials.Count > 0)
				{
					var entryIds = sequenceReader.GetSequence<long>("t_Sal_CustMatMappingEntry", notExistsmaterials.Count).ToArray();
					var entryPKIds = sequenceReader.GetSequence<long>("t_Sal_CustMatMappingEntry_L", notExistsmaterials.Count).ToArray();
					List<SqlObject> list = new List<SqlObject>();
					int index = 0;
					foreach (var notExistsmaterial in notExistsmaterials)
					{
						list.Add(new SqlObject(@"insert into t_Sal_CustMatMappingEntry(FENTRYID,FID,FCUSTMATNO,FMATERIALID,FEFFECTIVE)
	values(@FENTRYID,@FID,@FCUSTMATNO,(select FMASTERID from T_BD_MATERIAL where FMATERIALID = @FMATERIALID),1)", new List<SqlParam>()
					{
						 new SqlParam("@FENTRYID",KDDbType.Int64, entryIds[index]),
						 new SqlParam("@FID",KDDbType.Int64, id),
						 new SqlParam("@FCUSTMATNO",KDDbType.String, notExistsmaterial.CustomerMaterialNumber),
						 new SqlParam("@FMATERIALID",KDDbType.Int64, notExistsmaterial.MasterId)
					}));
						list.Add(new SqlObject(@"insert into t_Sal_CustMatMappingEntry_L(FPKID,FENTRYID,FLOCALEID,FCUSTMATNAME) values(@FPKID,@FENTRYID,2052,@FCUSTMATNAME)"
						, new List<SqlParam>()
						{
						 new SqlParam("@FPKID",KDDbType.Int64, entryPKIds[index]),
						 new SqlParam("@FENTRYID",KDDbType.Int64, entryIds[index]),
						 new SqlParam("@FCUSTMATNAME",KDDbType.String, notExistsmaterial.CustomerMaterialName)
						}));

						index++;
					}
					DBUtils.ExecuteBatch(ctx, list);
					string entryId = string.Join(",", entryIds);

					DBUtils.Execute(ctx, $@"/*dialect*/
Insert into T_V_SAL_CUSTMATMAPPING (fid,fheadfid,fnumber,FAUXPROPID,fcreateorgid,FUSEORGID,fcustomerid,FCREATORID,FCREATEDATE,FMODIFIERID,FMODIFYDATE,fdocumentstatus,fforbidstatus,FMATERIALID,FEFFECTIVE,FDEFCARRY,FISOLDVERSION,FENTRYID)
SELECT ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), HEAD.FUSEORGID)) fid, HEAD.FID fheadfid, ENTRY.FCUSTMATNO fnumber, ENTRY.FAUXPROPID, HEAD.FSALEORGID fcreateorgid, 
HEAD.FUSEORGID, ISNULL(CUST.FMASTERID, 0) fcustomerid, HEAD.FCREATORID, HEAD.FCREATEDATE, HEAD.FMODIFIERID, HEAD.FMODIFYDATE, 'C' fdocumentstatus, 'A' fforbidstatus, ENTRY.FMATERIALID, ENTRY.FEFFECTIVE, ENTRY.FDEFCARRY, HEAD.FISOLDVERSION , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPING HEAD 
INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON HEAD.FID = ENTRY.FID 
INNER JOIN T_BD_CUSTOMER CUST ON CUST.FCUSTID = HEAD.FCUSTOMERID 
WHERE HEAD.FDOCUMENTSTATUS = 'A' and ENTRY.FENTRYID in ({entryId})
UNION ALL 
SELECT ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), ISSUE.FISSUEORGID)) fid, HEAD.FID fheadfid, ENTRY.FCUSTMATNO fnumber, ENTRY.FAUXPROPID, HEAD.FSALEORGID fcreateorgid, 
ISSUE.FISSUEORGID fuseorgid, CUST.FMASTERID fcustomerid, HEAD.FCREATORID, HEAD.FCREATEDATE, HEAD.FMODIFIERID, HEAD.FMODIFYDATE, 'C'fdocumentstatus, 'A' fforbidstatus, MMASTER.FMATERIALID, ENTRY.FEFFECTIVE, ENTRY.FDEFCARRY, HEAD.FISOLDVERSION , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPING_ISSUE ISSUE 
	INNER JOIN T_SAL_CUSTMATMAPPING HEAD ON ISSUE.FID = HEAD.FID 
	INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON HEAD.FID = ENTRY.FID 
	INNER JOIN T_BD_MATERIAL M ON M.FMATERIALID = ENTRY.FMATERIALID 
	INNER JOIN T_BD_MATERIAL MMASTER ON (MMASTER.FMASTERID = M.FMASTERID AND (ISSUE.FISSUEORGID = MMASTER.FUSEORGID ) )
	INNER JOIN T_BD_CUSTOMER CUST ON CUST.FCUSTID = HEAD.FCUSTOMERID 
WHERE HEAD.FDOCUMENTSTATUS = 'A'  and ENTRY.FENTRYID in ({entryId})");
					DBUtils.Execute(ctx, $@"/*dialect*/
Insert into T_V_SAL_CUSTMATMAPPING_L (fid,FLOCALEID,fname,fdescription,FENTRYID)
SELECT  ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), HEAD.FUSEORGID)) fid, L.FLOCALEID, L.FCUSTMATNAME fname, ' ' fdescription , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPINGENTRY_L L 
INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON L.FENTRYID = ENTRY.FENTRYID 
INNER JOIN T_SAL_CUSTMATMAPPING HEAD ON ENTRY.FID = HEAD.FID 
where  HEAD.FDOCUMENTSTATUS = 'A' and ENTRY.FENTRYID in ({entryId})
UNION ALL 
SELECT ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), ISSUE.FISSUEORGID)) fid, L.FLOCALEID, L.FCUSTMATNAME fname, ' ' fdescription , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPINGENTRY_L L 
INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON L.FENTRYID = ENTRY.FENTRYID 
INNER JOIN T_SAL_CUSTMATMAPPING HEAD ON ENTRY.FID = HEAD.FID 
INNER JOIN T_SAL_CUSTMATMAPPING_ISSUE ISSUE ON ISSUE.FID = HEAD.FID 
where  HEAD.FDOCUMENTSTATUS = 'A' and ENTRY.FENTRYID in ({entryId})");

					cope.Complete();
				}
			}
			foreach (var notExistsmaterial in notExistsmaterials)
			{
				List<SqlParam> sqlParams = new List<SqlParam>()
				{
					new SqlParam("@FMATERIALID", KDDbType.Int64, notExistsmaterial.Id),
					new SqlParam("@fnumber", KDDbType.String, notExistsmaterial.CustomerMaterialNumber),
					new SqlParam("@FCUSTOMERID", KDDbType.Int64, customer.Id),
					new SqlParam("@FUSEORGID", KDDbType.Int64, notExistsmaterial.UseOrgId)
				};
				notExistsmaterial.CustomerMaterialId = DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, sqlParams.ToArray());
			}
			return materials;
		}

		/// <summary>
		/// 根据小类判断是否华南三部
		/// </summary>
		/// <returns></returns>
		private bool IsOrgHNTH(Context ctx, long productSmallClassId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@ProductSmallClassId", KDDbType.Int64, productSmallClassId) };
			string sql = @"/*dialect*/select top 1 t1.FID from T_BD_MasterialGroupTaxCode t1
                            inner join T_BD_SupplyOrgToBD t2 on t1.FBUSINESSDIVISION=t2.FBUSINESSDIVISION
                            where t1.FDOCUMENTSTATUS='C' and t2.FSUPPLYORGID=7207688 and FMATERIALGROUP=@ProductSmallClassId ";
			return DBServiceHelper.ExecuteScalar(ctx, sql, 0, paramList: pars.ToArray()) > 0 ? true : false;

		}

	}
}
