using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using mymooo.core.Attributes;
using Newtonsoft.Json;
using System.Diagnostics;

namespace com.mymooo.mall.business.Service.BaseService
{
    [AutoInject(InJectType.Scope)]
	public class ProductSmallService(MallContext mymoooContext)
	{
		private readonly MallContext _mymoooContext = mymoooContext;
		public void ReloadAllCache()
		{
			var timeStamp = _mymoooContext.SqlSugar.Ado.SqlQuery<byte[]>("select @@DBTS").First();
			var startTimeStamp = _mymoooContext.RedisCache.GetTimestamp<ProductSmallClass>();
			var filter = " [RowVersion] <= @EndTimeStamp";
			if (startTimeStamp != null)
			{
				filter += " and [RowVersion] > @StartTimeStamp ";
			}

			var smalls = _mymoooContext.SqlSugar.Queryable<ProductSmallClass>().Includes(p => p.ParentProductSmall).Includes(p => p.SupplyOrgs).Includes(p => p.ProductEngineer).Includes(p => p.ProductManager).Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).ToList();

            var tempString = _mymoooContext.SqlSugar.Queryable<BusinessParamConfig>()
              .Where(p => p.BKey == "SpecialSupplyOrgConfig")
              .Select(p => p.BValue)
              .First();
			List<SpecialSupplyOrgResponse>? specialSupplyOrg = [];
            if (!string.IsNullOrEmpty(tempString))
            {
				specialSupplyOrg = JsonConvert.DeserializeObject<List<SpecialSupplyOrgResponse>>(tempString);               
            }

            foreach (var small in smalls)
			{
                if (specialSupplyOrg != null)
				{
					foreach(var it in specialSupplyOrg)
					{
						if (small.Id == it.SmallClassId)
						{
							for (int i = 0; i <  small.SupplyOrgs.Count; i++)
							{
								small.SupplyOrgs[i].IsDefault = false;
								if (small.SupplyOrgs[i].SupplyOrgNumber == it.SupplyOrgNumber)
								{
                                    small.SupplyOrgs[i].IsDefault = true;
                                }
                            }
						}
					}
				}
				_mymoooContext.RedisCache.HashSet(small);
			}
			_mymoooContext.RedisCache.SetTimestamp<ProductSmallClass>(timeStamp);
		}

        public void ReloadCustomerSupplyOrgCache()
        {
            var customerSupplyOrg = _mymoooContext.SqlSugar.Queryable<CompanyMapSupplyOrg>()
                .LeftJoin<BusinessDivisionSupplyOrg>((c, b) => c.SupplyOrgCode == b.SupplyOrgNumber)
				.Where(c => !c.IsDeleted)
				.Select((c,b) => new CompanyMapSupplyOrg { 
					CompanyCode = c.CompanyCode,
					BusinessDivisionNumber = c.BusinessDivisionNumber,
					SupplyOrgCode = c.SupplyOrgCode,
					SupplyOrgId = b.SupplyOrgId,
					SupplyOrgName = b.SupplyOrgName,
					IsDeleted = c.IsDeleted
				})
                .ToList();
            _mymoooContext.RedisCache.HashDelete<CompanyMapSupplyOrg>();

            foreach (var it in customerSupplyOrg)
            {
                _mymoooContext.RedisCache.HashSet(it);
            }


        }


		public List<ClassTreeModel> GetClassTree()
		{
			var classList = _mymoooContext.SqlSugar.Queryable<ProductClass>().Where(p => p.IsRelease).Select(e => new ClassTreeModel
            {
                ClassId = e.ClassId,
                ClassName = e.ClassName,
                ParentClassId = e.ParentClassId,
                Seq = e.Seq,
                Childs = new List<ClassTreeModel>()
            }).ToList();
            //var classListResult = GetChildClass(classList, -1);
            return classList;
        }

		private List<ClassTreeModel> GetChildClass(List<ProductClass> classList,long parentId)
		{
			var childClassList = classList.Where(e=> e.ParentClassId==parentId).Select(e => new ClassTreeModel
            {
                ClassId = e.ClassId,
                ClassName = e.ClassName,
                ParentClassId = e.ParentClassId,
                Seq = e.Seq,
                Childs = new List<ClassTreeModel>()
            }).ToList();
			foreach (var item in childClassList)
			{
				item.Childs = GetChildClass(classList, item.ClassId);
			}
			return childClassList;
		}

    }
}
