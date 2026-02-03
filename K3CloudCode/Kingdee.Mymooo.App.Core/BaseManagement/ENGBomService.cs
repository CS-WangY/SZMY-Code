using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Util;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.Mymooo.Contracts.BaseManagement;
using System.Security.Cryptography;
using System.Dynamic;
using Kingdee.BOS.Core.Bill;
using DynamicObject = Kingdee.BOS.Orm.DataEntity.DynamicObject;
using System.Xml.Linq;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.BOS.Core.DynamicForm;
using System.Collections;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.BOS.Contracts;

namespace Kingdee.Mymooo.App.Core.BaseManagement
{
	public class ENGBomService : IENGBomService
	{
		public ENGBomInfo[] TryGetOrAdds(Context ctx, ENGBomInfo[] data)
		{
			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{
				List<ENGBomInfo> bomInfos = new List<ENGBomInfo>();
				foreach (var item in data)
				{
					bomInfos.Add(TryGetOrAdd(ctx, item));
				}
				cope.Complete();
				return bomInfos.ToArray();
			}
		}
		public ENGBomInfo[] TryGetOrAddsOrg(Context ctx, ENGBomInfo[] data, long[] orgid)
		{
			List<ENGBomInfo> bomInfos = new List<ENGBomInfo>();
			foreach (var item in data)
			{
				bomInfos.Add(TryGetOrAddOrg(ctx, item, orgid));
			}
			return bomInfos.ToArray();
		}
		public ENGBomInfo TryGetOrAdd(Context ctx, ENGBomInfo bom)
		{
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, "ENG_BOM") as FormMetadata;
			var billView = FormMetadataUtils.CreateBillView(ctx, "ENG_BOM");
			billView.Model.SetValue("FCreateOrgId", 1);
			billView.Model.SetValue("FNAME", bom.FMATERIALID, 0);
			billView.Model.SetValue("FAutoCraft", bom.AutoCraft, 0);
			billView.Model.SetItemValueByNumber("FMATERIALID", bom.FMATERIALID, 0);
			billView.Model.SetValue("FPENYProcessID", bom.ProcessID, 0);
			//MES项目专用
			if (!string.IsNullOrWhiteSpace(bom.FNUMBER))
			{
				billView.Model.SetValue("FNUMBER", bom.FNUMBER, 0);
			}


			billView.Model.DeleteEntryData("FTreeEntity");
			var rowcount = 0;
			foreach (var item in bom.Entity)
			{
				billView.Model.CreateNewEntryRow("FTreeEntity");
				billView.Model.SetItemValueByNumber("FMATERIALIDCHILD", item.FMATERIALIDCHILD, rowcount);
				billView.InvokeFieldUpdateService("FMATERIALIDCHILD", rowcount);
				//billView.Model.SetItemValueByNumber("FCHILDUNITID", item.FUnitNumber, rowcount);
				//billView.InvokeFieldUpdateService("FCHILDUNITID", rowcount);
				billView.Model.SetValue("FNUMERATOR", item.FNUMERATOR, rowcount);
				billView.Model.SetValue("FDENOMINATOR", item.FDENOMINATOR, rowcount);
				billView.Model.SetValue("FSCRAPRATE", item.FSCRAPRATE, rowcount);
				billView.Model.SetValue("FSendMes", item.SendMes, rowcount);
				billView.Model.SetValue("FMEMO", item.EntryNote, rowcount);
				billView.Model.SetValue("FPENYImportPrice", item.ImportAmount, rowcount);
				//billView.Model.SetValue("FPENYImportPrice", item.ImportAmount * item.FNUMERATOR, rowcount);
				if (!item.IsSueType)
				{
					billView.Model.SetValue("FIsSueType", 7, rowcount);
					billView.Model.SetValue("FIsMrpRun", false, rowcount);
				}

				billView.InvokeFieldUpdateService("FSCRAPRATE", rowcount);
				rowcount++;
			}

			SaveService saveService = new SaveService();
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);

			var oper = saveService.SaveAndAudit(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
			//清除释放网控
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
			bom.Id = Convert.ToInt64(billView.Model.DataObject["Id"]);
			bom.FNUMBER = Convert.ToString(billView.Model.DataObject["Number"]);
			//SendMQAllocate(ctx, bom);
			return bom;
		}
		public ENGBomInfo TryGetOrAddOrg(Context ctx, ENGBomInfo bom, long[] orgid)
		{
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, "ENG_BOM") as FormMetadata;
			var billView = FormMetadataUtils.CreateBillView(ctx, "ENG_BOM");
			billView.Model.SetValue("FCreateOrgId", 1);
			billView.Model.SetValue("FNAME", bom.FMATERIALID, 0);
			billView.Model.SetValue("FAutoCraft", bom.AutoCraft, 0);
			billView.Model.SetItemValueByNumber("FMATERIALID", bom.FMATERIALID, 0);
			billView.Model.SetValue("FPENYProcessID", bom.ProcessID, 0);
			//MES项目专用
			if (!string.IsNullOrWhiteSpace(bom.FNUMBER))
			{
				billView.Model.SetValue("FNUMBER", bom.FNUMBER, 0);
			}


			billView.Model.DeleteEntryData("FTreeEntity");
			var rowcount = 0;
			foreach (var item in bom.Entity)
			{
				billView.Model.CreateNewEntryRow("FTreeEntity");
				billView.Model.SetItemValueByNumber("FMATERIALIDCHILD", item.FMATERIALIDCHILD, rowcount);
				billView.InvokeFieldUpdateService("FMATERIALIDCHILD", rowcount);
				//billView.Model.SetItemValueByNumber("FCHILDUNITID", item.FUnitNumber, rowcount);
				//billView.InvokeFieldUpdateService("FCHILDUNITID", rowcount);
				billView.Model.SetValue("FNUMERATOR", item.FNUMERATOR, rowcount);
				billView.Model.SetValue("FDENOMINATOR", item.FDENOMINATOR, rowcount);
				billView.Model.SetValue("FSCRAPRATE", item.FSCRAPRATE, rowcount);
				billView.Model.SetValue("FSendMes", item.SendMes, rowcount);
				billView.Model.SetValue("FMEMO", item.EntryNote, rowcount);
				if (!item.IsSueType)
				{
					billView.Model.SetValue("FIsSueType", 7, rowcount);
					billView.Model.SetValue("FIsMrpRun", false, rowcount);
				}

				billView.InvokeFieldUpdateService("FSCRAPRATE", rowcount);
				rowcount++;
			}

			SaveService saveService = new SaveService();
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);

			var oper = saveService.SaveAndAudit(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
			//清除释放网控
			//billView.CommitNetworkCtrl();
			//billView.InvokeFormOperation(FormOperationEnum.Close);
			//billView.Close();
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
			bom.Id = Convert.ToInt64(billView.Model.DataObject["Id"]);
			bom.MasterId = Convert.ToInt64(billView.Model.DataObject["msterID"]);
			bom.FNUMBER = Convert.ToString(billView.Model.DataObject["Number"]);
			//分配供货组织
			AllocateService allocateService = new AllocateService();
			foreach (var itemorg in orgid)
			{
				AllocateParameter allocateParameter = new AllocateParameter(billView.BusinessInfo, meta.InheritPath, 1, BOSEnums.Enu_AllocateType.Allocate, OperationNumberConst.OperationNumber_Allocate)
				{
					PkId = new List<object>() { Convert.ToInt64(billView.Model.DataObject["Id"]) },
					AutoSubmitAndAudit = true,
					AllocateUserId = ctx.UserId,
					DestOrgId = itemorg,
					DestOrgName = ""
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
			

			return bom;
		}
		public ResponseMessage<dynamic> BomAllocate(Context ctx, List<ENGBomInfo> bomlist)
		{
			ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();
			try
			{
				foreach (var bomInfo in bomlist)
				{
					var billView = FormMetadataUtils.CreateBillView(ctx, "ENG_BOM", bomInfo.Id);
					FormMetadata meta = MetaDataServiceHelper.Load(ctx, "ENG_BOM") as FormMetadata;
					List<DynamicObject> dynamicObjects = new List<DynamicObject>();

					//查找需要分配的组织
					var sql = @"select e.FTARGETORGID,ol.FNAME 
                                from T_ORG_BDCtrlPolicy o
                                inner join T_ORG_BDCtrlTOrgEntry e on o.FPOLICYID = e.FPOLICYID
                                inner join T_ORG_ORGANIZATIONS_L ol on e.FTARGETORGID = ol.FORGID and ol.FLOCALEID = 2052
                                where o.FBASEDATATYPEID = 'ENG_BOM'
                            and not exists (select 1 from T_ENG_BOM m where e.FTARGETORGID = m.FUSEORGID and m.FMASTERID = @FMASTERID)";
					var allocateOrgs = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FMASTERID", KDDbType.Int64, bomInfo.Id));
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

				result.Data = bomlist;
				result.Code = ResponseCode.Success;
				result.Message = "BOM分配成功";
			}
			catch (Exception ex)
			{
				result.Code = ResponseCode.Exception;
				result.Message = ex.Message;
			}
			return result;
		}
		public ResponseMessage<dynamic> BomAllocateByID(Context ctx, long[] bomlist)
		{
			ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();
			try
			{
				foreach (var bomid in bomlist)
				{
					var billView = FormMetadataUtils.CreateBillView(ctx, "ENG_BOM", bomid);
					FormMetadata meta = MetaDataServiceHelper.Load(ctx, "ENG_BOM") as FormMetadata;
					//查找需要分配的组织
					var sql = @"select e.FTARGETORGID,ol.FNAME 
                                from T_ORG_BDCtrlPolicy o
                                inner join T_ORG_BDCtrlTOrgEntry e on o.FPOLICYID = e.FPOLICYID
                                inner join T_ORG_ORGANIZATIONS_L ol on e.FTARGETORGID = ol.FORGID and ol.FLOCALEID = 2052
                                where o.FBASEDATATYPEID = 'ENG_BOM'
                            and not exists (select 1 from T_ENG_BOM m where e.FTARGETORGID = m.FUSEORGID and m.FMASTERID = @FMASTERID)";
					var allocateOrgs = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FMASTERID", KDDbType.Int64, bomid));
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
				result.Message = "BOM分配成功";
			}
			catch (Exception ex)
			{
				result.Code = ResponseCode.Exception;
				result.Message = ex.Message;
			}
			return result;
		}
		public ResponseMessage<dynamic> MaterialAllocate(Context ctx, List<ENGBomInfo> bomlist)
		{
			ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();
			result.Data = bomlist;
			try
			{
				foreach (var bomInfo in bomlist)
				{
					var bominfo = QueryBomlevel(ctx, bomInfo.FNUMBER);
					//var billView = FormMetadataUtils.CreateBillView(ctx, "ENG_BOM", bomInfo.Id);
					//Allocate(ctx, Convert.ToInt64(billView.Model.DataObject["MATERIALID_Id"]), Convert.ToString(((DynamicObject)(billView.Model.DataObject["MATERIALID"]))["Number"]));
					var maxlevel = bominfo.Max(x => Convert.ToInt32(x["BOM层级"]));
					foreach (var item in bominfo)
					{
						var materialid = Convert.ToInt64(item["子项物料内码"]);
						var materialnumber = Convert.ToString(item["子项物料代码"]);
						var bomlevel = Convert.ToInt32(item["BOM层级"]);

						var ischild = QueryMaterialLevel(ctx, materialid);
						Allocate(ctx, materialid, materialnumber, bomlevel, maxlevel, ischild);
					}
				}
				result.Code = ResponseCode.Success;
				result.Message = "物料分配成功";
				//cope.Complete();
			}
			catch (Exception ex)
			{
				result.Code = ResponseCode.Exception;
				result.Message = ex.Message;
			}
			return result;
		}

		public void Allocate(Context ctx, long materialid, string materialnumber, int bomlevel, int maxlevel, bool ischild)
		{
			//FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_MATERIAL") as FormMetadata;
			List<DynamicObject> dynamicObjects = new List<DynamicObject>();
			var editBillView = FormMetadataUtils.CreateBillView(ctx, "BD_MATERIAL", materialid);

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
				//if (oper.ValidationErrors.Count > 0)
				//{
				//    throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
				//}
			}
			//修改供货组织物料属性为自制
			string sSql = $@"SELECT t1.FMATERIALID,t1.FUSEORGID,t2.FWORKSHOPID FROM dbo.T_BD_MATERIAL t1
INNER JOIN T_BD_MasterialGroupTaxCode t2 ON t1.FMATERIALGROUP=t2.FMATERIALGROUP
INNER JOIN T_BD_SupplyOrgToBD t3 ON t2.FBUSINESSDIVISION=t3.FBUSINESSDIVISION AND t1.FUSEORGID=t3.FSUPPLYORGID
WHERE t1.FNUMBER='{materialnumber}'";
			var datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
			dynamicObjects = new List<DynamicObject>();
			ViewService viewService = new ViewService();
			editBillView = FormMetadataUtils.CreateBillView(ctx, "BD_MATERIAL");
			foreach (var data in datas)
			{
				if (Convert.ToInt64(data["FUSEORGID"]) == 7207688)//华南三部
				{
					editBillView.Model.DataObject = viewService.LoadSingle(ctx, data["FMATERIALID"],
					editBillView.BusinessInfo.GetDynamicObjectType());
					editBillView.Model.SetValue("FErpClsID", 3);
					editBillView.Model.SetValue("FIsPurchase", true);
					editBillView.Model.SetValue("FIsSubContract", true);
					editBillView.Model.SetValue("FIsProduce", false);
					editBillView.Model.SetItemValueByID("FWorkShopId", data["FWORKSHOPID"], 0);
					dynamicObjects.Add(editBillView.Model.DataObject);
				}
				else
				{
					editBillView.Model.DataObject = viewService.LoadSingle(ctx, data["FMATERIALID"],
					editBillView.BusinessInfo.GetDynamicObjectType());


					//如果物料有子项BOM为自制，没有则为外购
					if (ischild)
					{
						//如果物料属性为委外则不管
						if (Convert.ToInt16(editBillView.Model.GetValue("FErpClsID")) == 3)
						{

						}
						else
						{
							editBillView.Model.SetValue("FErpClsID", 2);
						}
						switch (Convert.ToInt64(data["FUSEORGID"]))
						{
							//全国一部二部
							case 7401780:
								editBillView.Model.SetValue("FCheckProduct", true);
								if (bomlevel != 0)
								{
									editBillView.Model.SetItemValueByNumber("FStockId", "SFPQGON", 0);
								}
								break;
							case 7401781:
								editBillView.Model.SetValue("FCheckProduct", true);
								break;
						}
					}
					else
					{
						editBillView.Model.SetValue("FErpClsID", 1);
					}

					editBillView.Model.SetValue("FIsPurchase", true);
					editBillView.Model.SetValue("FIsSubContract", true);

					var groupclass = Convert.ToInt64(editBillView.Model.DataObject["MaterialGroup_Id"]);
					switch (groupclass)
					{

						case 12://导向轴
							editBillView.Model.SetItemValueByID("FWorkShopId", data["FWORKSHOPID"], 0);
							break;
						case 32://同步带轮
								//BM000366同步轮组装组,BM000365同步轮生产组
							if (maxlevel > 1)
							{
								if (bomlevel == 0)
								{
									editBillView.Model.SetItemValueByNumber("FWorkShopId", "BM000366", 0);
								}
								else
								{
									editBillView.Model.SetItemValueByNumber("FWorkShopId", "BM000365", 0);
								}
							}
							else
							{
								if (bomlevel == 0)
								{
									editBillView.Model.SetItemValueByNumber("FWorkShopId", "BM000365", 0);
								}
							}
							break;
					}

					dynamicObjects.Add(editBillView.Model.DataObject);
				}
				Save(ctx, editBillView, dynamicObjects);
			}
		}

		/// <summary>
		/// 获取范思德下BOM物料及层级关系
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="bomNumber"></param>
		/// <returns></returns>
		public DynamicObjectCollection QueryBomlevel(Context ctx, string bomNumber)
		{
			string sSql = $@"/*dialect*/;with cte as
                                (
	                                --1、定点（Anchor）子查询，用来查询最顶级的产品的BOM的
	                                select 
		                                0 as BOM层次,t1.fid as 最顶级BOM内码
		                                ,t1.FNUMBER as BOM版本,fxwl.FMATERIALID as 物料内码,t3.FUNITID as 单位,fxwl.FNUMBER as 父项物料代码,fxwl_L.FNAME as 父项物料名称,t3.FSEQ as 分录行号
		                                ,t3.FREPLACEGROUP as 项次,CAST(10000+t3.FREPLACEGROUP AS nvarchar) as 项次组合
		                                ,cast(CAST(t1.fid AS nvarchar)+'-'+CAST(10000+t3.FREPLACEGROUP AS nvarchar) as nvarchar(max)) as BOM内码和项次组合
		                                ,t3.FMATERIALID as 子项物料内码,zxwl.FNUMBER as 子项物料代码,zxwl_L.FNAME as 子项物料名称
		                                ,case when FMATERIALTYPE = 1 then '标准件'
				                                when FMATERIALTYPE = 2 then '返还件' 
				                                when FMATERIALTYPE = 3 then '替代件' 
				                                else '未知类型' end as 子项类型
		                                ,t3.FNUMERATOR as 分子,t3.FDENOMINATOR as 分母,t3.FFIXSCRAPQTY as 固定损耗,t3.FSCRAPRATE as 变动损耗,t3.FBOMID,t1.FUSEORGID
		                                ,0 as 是否有子项BOM版本

	                                from dbo.T_ENG_BOM t1
		                                join T_BD_MATERIAL fxwl			--用父项关联物料表
			                                on fxwl.FMATERIALID = t1.FMATERIALID
				                                and t1.FFORBIDSTATUS = 'A'	--只取未禁用状态的BOM
		                                join T_BD_MATERIAL_L fxwl_L		--用父项关联物料多语言表
			                                on fxwl.FMATERIALID = fxwl_l.FMATERIALID and fxwl_L.FLOCALEID =2052
		                                --join T_BD_MATERIALPRODUCE fxwl_P
			                            --    on fxwl_P.FMATERIALID = fxwl.FMATERIALID
		                                join T_ENG_BOMCHILD t3
			                                on t1.fid = t3.FID		
		                                join T_BD_MATERIAL zxwl			--用子项关联物料表
			                                on zxwl.FMATERIALID = t3.FMATERIALID
		                                join T_BD_MATERIAL_L zxwl_L		--用子项关联物料多语言表
			                                on zxwl.FMATERIALID = zxwl_L.FMATERIALID and zxwl_L.FLOCALEID =2052
	                                where 1=1
		                                --and fxwl_P.FISMAINPRD = 1		--物料-生产页签的'可为主产品'属性FISMAINPRD,等于1就意味着可以建立BOM 
		                                and t1.FNUMBER in ('{bomNumber}') --这里可以输入一个产品BOM版本,则只会查询一个产品的BOM多级展开;如果这一句注释掉了,就可以查询全部产品物料的多级展开;下面还有一个控制的条件要同步改,一共两个.

	                                union all

	                                --2、递归子查询，根据定点子查询的查询结果来关联展开它的所有下级的BOM
	                                select  
		                                p.BOM层次+1 as BOM层次,P.最顶级BOM内码 as 最顶级BOM内码
		                                ,t1.FNUMBER as BOM版本,fxwl.FMATERIALID as 物料内码,t3.FUNITID,fxwl.FNUMBER as 父项物料代码,fxwl_L.FNAME as 父项物料名称,t3.FSEQ as 分录行号
		                                ,t3.FREPLACEGROUP as 项次,cast(p.项次组合+'.'+CAST(10000+t3.FREPLACEGROUP AS nvarchar) as nvarchar) as 项次组合
		                                ,cast(p.BOM内码和项次组合 +'.'+ ( CAST(t1.FID AS nvarchar) + '-' +CAST(10000+t3.FREPLACEGROUP AS nvarchar) ) as nvarchar(max))  as BOM内码组合
		                                ,t3.FMATERIALID as 子项物料内码,zxwl.FNUMBER as 子项物料代码,zxwl_L.FNAME as 子项物料名称
		                                ,case when FMATERIALTYPE = 1 then '标准件'
				                                when FMATERIALTYPE = 2 then '返还件' 
				                                when FMATERIALTYPE = 3 then '替代件' 
				                                else '未知类型' end as 子项类型
		                                ,t3.FNUMERATOR as 分子,t3.FDENOMINATOR as 分母,t3.FFIXSCRAPQTY as 固定损耗,t3.FSCRAPRATE as 变动损耗,t3.FBOMID,t1.FUSEORGID
		                                ,case when p.FBOMID = t1.FID then 1 else 0 end as 是否有子项BOM版本
	                                from cte P		--调用递归CTE本身
		                                join dbo.T_ENG_BOM t1
			                                on t1.FMATERIALID = p.子项物料内码
		                                join T_BD_MATERIAL fxwl			--父项关联物料表
			                                on fxwl.FMATERIALID = t1.FMATERIALID
			                                and t1.FFORBIDSTATUS = 'A'
		                                join T_BD_MATERIAL_L fxwl_L		--父项关联物料多语言表
			                                on fxwl.FMATERIALID = fxwl_l.FMATERIALID and fxwl_L.FLOCALEID =2052
		                                join T_ENG_BOMCHILD t3
			                                on t1.fid = t3.FID		
		                                join T_BD_MATERIAL zxwl			--子项关联物料表
			                                on zxwl.FMATERIALID = t3.FMATERIALID
		                                join T_BD_MATERIAL_L zxwl_L		--子项关联物料多语言表
			                                on zxwl.FMATERIALID = zxwl_L.FMATERIALID and zxwl_L.FLOCALEID =2052
                                )
                                --select * from cte		----调试第一段CTE
                                ,cte2_ZuiXinZiXiangBom as		--这个cte2是用来取非0层的子项BOM的最新BOM版本的,然后和0层的父项信息union在一起
                                (
	                                select 
		                                t1.BOM层次 as BOM层级,t1.最顶级BOM内码,t1.BOM版本
		                                ,t1.物料内码,t1.单位,t1.父项物料代码 as 物料代码,t1.父项物料名称 as 物料名称,0 as 分录行号,0 as 项次,t1.项次组合 as 项次组合,BOM内码和项次组合,t1.物料内码 as 子项物料内码,'' as 子项物料代码,'' as 子项物料名称,'最顶层父项' as 子项类型,0 as 分子,0 as 分母,0 as 固定损耗,0 as 变动损耗,0 as BOM内码,t1.FUSEORGID,t1.是否有子项BOM版本
		                                ,dense_rank() over(partition by t1.最顶级BOM内码,t1.父项物料代码 order by t1.BOM版本 desc) as BOM版本号分区
	                                from cte t1
	                                where 1=1 
		                                and t1.BOM层次 = 0 and t1.项次组合 = '10001'		--这里是只显示0层的产品
		                                and t1.BOM版本 in ('{bomNumber}')	--这里可以输入一个产品BOM版本,则只会查询一个产品的BOM多级展开;如果这一句注释掉了,就可以查询全部产品物料的多级展开;上面还有一个控制的条件要同步改,一共两个.

	                                union 
	                                select 
		                                t1.BOM层次+1 as BOM层级,t1.最顶级BOM内码,t1.BOM版本
		                                ,t1.物料内码,t1.单位,t1.子项物料代码 as 物料代码,t1.子项物料名称 as 物料名称,t1.分录行号 as 分录行号,t1.项次 as 项次,t1.项次组合 as 项次组合,BOM内码和项次组合,t1.子项物料内码 as 子项物料内码,t1.子项物料代码 as 子项物料代码,'' as 子项物料名称,t1.子项类型 as 子项类型,t1.分子 as 分子,t1.分母 as 分母,t1.固定损耗 as 固定损耗,t1.变动损耗 as 变动损耗,t1.FBOMID as BOM内码,t1.FUSEORGID,t1.是否有子项BOM版本
		                                ,dense_rank() over(partition by t1.最顶级BOM内码,t1.父项物料代码 order by t1.BOM层次+1,t1.是否有子项BOM版本 desc,t1.BOM版本 desc) as BOM版本号分区	--通过这个字段标识最新版本的BOM，按照父项物料分区之后，把BOM版本降序排列，BOM版本高的排序序号就是1
	                                from cte t1
	                                where 1=1
		                                --and t1.BOM层次+1 <=2	--可以通过BOM层次字段来控制递归循环的次数,如果这里不加控制，那系统默认最多是循环100次
                                )
                                --select * from cte2_ZuiXinZiXiangBom t2		----调试第二段CTE
                                select t2.BOM层级 as BOM层级
		                                ,t2.物料内码,t2.子项物料内码,t2.单位,g.FPARENTID as 大类,ms.FMATERIALGROUP as 小类
		                                ,t2.物料代码 as 子项物料代码,t2.物料名称 as 物料名称,t2.分录行号 as 分录行号,t2.项次 as 项次,t2.子项类型 as 子项类型,t2.分子 as 分子,t2.分母 as 分母
		                                ,t2.固定损耗 as 固定损耗,t2.变动损耗 as 变动损耗
		                                ,t2.FUSEORGID,t2.项次组合 as 项次组合,t2.BOM内码和项次组合,t2.BOM内码 as 子项BOM版本内码,t2.BOM版本 as 所属BOM,t2.最顶级BOM内码	--这一行的可以注释掉,只是为了排查SQL问题用的.
                                from cte2_ZuiXinZiXiangBom t2
                                left join T_BD_MATERIAL ms on t2.子项物料内码=ms.FMATERIALID
								left join T_BD_MATERIALGROUP g on ms.FMATERIALGROUP = g.FID
                                where 1=1
	                                and t2.BOM版本号分区 = 1		--通过“BOM版本号分区”标识最新版本的BOM，按照父项物料分区之后，把BOM版本降序排列，BOM版本高的值就是1
	                                and ( (t2.BOM层级 = 0 and t2.项次组合 = '10001' ) or (t2.BOM层级 > 0) )	--这个是为了查询出最终的结果.
	                                and t2.FUSEORGID = 1	--蓝海机械账套的‘总装事业部’组织
                                order by t2.BOM内码和项次组合
                            ";
			return DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
		}
		public bool QueryMaterialLevel(Context ctx, long materialid)
		{
			string sSql = $"SELECT * FROM T_ENG_BOM WHERE FMATERIALID={materialid}";
			if (DBServiceHelper.ExecuteDynamicObject(ctx, sSql).Count() > 0)
				return true;
			return false;
		}
		public void SendMQAllocate(Context ctx, List<ENGBomInfo> request)
		{
			MymoooBusinessDataService service = new MymoooBusinessDataService();

			ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
			{
				Url = $"RabbitMQ/SendMessage?rabbitCode=K3Cloud_BomAllocate_",
				Message = JsonConvertUtils.SerializeObject(request)
			};
			taskInfo.Id = service.AddRabbitMqMeaage(ctx, "Apigateway", "", JsonConvertUtils.SerializeObject(taskInfo)).Data;
			Task.Factory.StartNew(() =>
			{
				var result = ApigatewayUtils.InvokePostRabbitService(taskInfo.Url, taskInfo.Message);
				var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
				if (response.IsSuccess)
				{
					service.UpdateRabbitMqMeaage(ctx, taskInfo.Id, result, true);
				}
			});
		}
		public void Save(Context ctx, IBillView editBillView, List<DynamicObject> dynamicObjects)
		{
			SaveService saveService = new SaveService();
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);

			var oper = saveService.Save(ctx, editBillView.BusinessInfo, dynamicObjects.ToArray(), operateOption);
			//清除释放网控
			editBillView.CommitNetworkCtrl();
			editBillView.InvokeFormOperation(FormOperationEnum.Close);
			editBillView.Close();
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
