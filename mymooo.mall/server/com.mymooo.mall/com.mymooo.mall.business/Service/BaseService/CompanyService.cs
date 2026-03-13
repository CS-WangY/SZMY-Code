using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.CompanyInfo;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using Elastic.Clients.Elasticsearch.QueryDsl;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Attributes.RabbitMQ;
using mymooo.core.Utils.JsonConverter;

namespace com.mymooo.mall.business.Service.BaseService
{
	[AutoInject(InJectType.Scope)]
	public class CompanyService(MallContext mymoooContext, GatewayService gatewayService)
	{
		private readonly MallContext _mymoooContext = mymoooContext;
		private readonly GatewayService _mgatewayService = gatewayService;

		public async Task<ResponseMessage<dynamic>> BindRelation(SubAndParentCompany request)
		{
			ResponseMessage<dynamic> result = new();
			request.CreateDateTime = DateTime.Now;
			request.IsValid = true;
			var company = _mymoooContext.RedisCache.HashGet(new Company() { Code = request.CompanyCode });
			var parentCompany = _mymoooContext.RedisCache.HashGet(new Company() { Code = request.ParentCompanyCode });
			if (company == null || parentCompany == null)
			{
				result.Code = ResponseCode.NoExistsData;
				result.ErrorMessage = $"客户编码{request.CompanyCode}或者母公司编码{request.ParentCompanyCode}在数据库中不存在!";
				return result;
			}
			request.CompanyId = company.Id;
			request.ParentCompanyId = parentCompany.Id;
			var data = _mymoooContext.SqlSugar.Queryable<SubAndParentCompany>().First(c => c.CompanyId == request.CompanyId && c.ParentCompanyId == request.ParentCompanyId);
			if (data == null)
			{
				if (!(await _mymoooContext.SqlSugar.Insertable(request).IgnoreColumnsNull().ExecuteCommandAsync() > 0))
				{
					result.Code = ResponseCode.DbUpdateException;
					result.ErrorMessage = "数据库插入出错";
				}
			}
			result.Data = request;

			return result;
		}

		public async Task<ResponseMessage<SubAndParentCompany>> UnBindRelation(SubAndParentCompany request)
		{
			ResponseMessage<SubAndParentCompany> result = new()
			{
				Data = _mymoooContext.SqlSugar.Queryable<SubAndParentCompany>().First(c => c.Id == request.Id)
			};
			if (result.Data == null)
			{
				result.Code = ResponseCode.abort;
				result.ErrorMessage = $"关系在数据库中不存在!";
				return result;
			}
			var company = _mymoooContext.RedisCache.HashIndexGet(new Company() { Id = result.Data.CompanyId ?? Guid.NewGuid() });
			var parentCompany = _mymoooContext.RedisCache.HashIndexGet(new Company() { Id = result.Data.ParentCompanyId ?? Guid.NewGuid() });

			if (company == null || parentCompany == null)
			{
				result.Code = ResponseCode.NoExistsData;
				result.ErrorMessage = $"客户Id:{result.Data.CompanyId}或者母公司Id:{result.Data.ParentCompanyId}在数据库中不存在!";
				return result;
			}
			if (!(await _mymoooContext.SqlSugar.Deleteable(result.Data).ExecuteCommandAsync() > 0))
			{
				result.Code = ResponseCode.DbUpdateException;
				result.ErrorMessage = "数据库删除出错";
			}
			result.Data.CompanyCode = company.Code;
			result.Data.ParentCompanyCode = parentCompany.Code;

			return result;
		}

		public async Task<ResponseMessage<SubAndParentCompany>> UpdateCompanyGroupCache(SubAndParentCompany request)
		{
			ResponseMessage<SubAndParentCompany> result = new() { Data = request };
			var query = _mymoooContext.SqlSugar.Queryable<Company>()
				.Includes(p => p.SubCompanys.Where(c => c.IsValid == true).ToList())
				.Includes(p => p.SubCompanys, r => r.ParentCompany)
				.Includes(p => p.ParentCompanys.Where(c => c.IsValid == true).ToList())
				.Includes(p => p.ParentCompanys, r => r.SubCompany);
			await SetCampnayGroupCache(null, query.First(c => c.Code == request.CompanyCode));
			await SetCampnayGroupCache(null, query.First(c => c.Code == request.ParentCompanyCode));

			return result;
		}

		public void ReloadCache()
		{
			var timeStamp = _mymoooContext.SqlSugar.Ado.SqlQuery<byte[]>("select @@DBTS").First();
			var startTimeStamp = _mymoooContext.RedisCache.GetTimestamp<Company>();
			var filter = " [RowVersion] <= @EndTimeStamp";
			if (startTimeStamp != null)
			{
				filter += " and [RowVersion] > @StartTimeStamp ";
			}

			var query = _mymoooContext.SqlSugar.Queryable<Company>().Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).OrderBy(p => p.Code);
			int pageIndex = 1;
			var companys = query.ToOffsetPage(pageIndex, 1000);
			while (companys.Count > 0)
			{
				foreach (var company in companys)
				{
					_mymoooContext.RedisCache.HashSet(company);
				}
				companys = query.ToOffsetPage(++pageIndex, 1000);
			}
			_mymoooContext.RedisCache.SetTimestamp<Company>(timeStamp);
		}

		public async Task ReloadGroupCache([RabbitBody] CompanyReloadCacheRequest? request)
		{
			int pageIndex = 1;
			var query = _mymoooContext.SqlSugar.Queryable<Company>()
				.Includes(p => p.SubCompanys.Where(c => c.IsValid == true).ToList())
				.Includes(p => p.SubCompanys, r => r.ParentCompany)
				.Includes(p => p.ParentCompanys.Where(c => c.IsValid == true).ToList())
				.Includes(p => p.ParentCompanys, r => r.SubCompany)
				.OrderBy(p => p.Code);
			var companys = query.ToOffsetPage(pageIndex, 100);
			while (companys.Count > 0)
			{
				foreach (var company in companys)
				{
					await SetCampnayGroupCache(request, company);
				}
				companys = query.ToOffsetPage(++pageIndex, 100);
			}
		}

		private async Task SetCampnayGroupCache(CompanyReloadCacheRequest? request, Company company)
		{
			CompanyGroup oldCompanyGroup = new() { CompanyCode = company.Code };
			oldCompanyGroup.SubCompanyCode = _mymoooContext.RedisCache.HashGet(oldCompanyGroup, p => p.SubCompanyCode);
			oldCompanyGroup.SubCreditCompanyCode = _mymoooContext.RedisCache.HashGet(oldCompanyGroup, p => p.SubCreditCompanyCode);
			oldCompanyGroup.ParentCompanyCode = _mymoooContext.RedisCache.HashGet(oldCompanyGroup, p => p.ParentCompanyCode);
			oldCompanyGroup.UnifiedCreditLine = _mymoooContext.RedisCache.HashGet(oldCompanyGroup, p => p.UnifiedCreditLine);
			CompanyGroup companyGroup = new() { CompanyCode = company.Code };
			if (company.SubCompanys != null && company.SubCompanys.Exists(p => p.IsValid))
			{
				var subCompany = company.SubCompanys.First(p => p.IsValid);
				companyGroup.UnifiedCreditLine = subCompany.UnifiedCreditLine;
				companyGroup.ParentCompanyCode = subCompany.ParentCompany?.Code;
			}
			if (company.ParentCompanys != null && company.ParentCompanys.Count > 0)
			{
				companyGroup.SubCompanyCode = company.ParentCompanys.Select(p => p.SubCompany?.Code ?? "").ToList();
				companyGroup.SubCreditCompanyCode = company.ParentCompanys.Where(p => p.UnifiedCreditLine).Select(p => p.SubCompany?.Code ?? "").ToList();
			}

			if (request != null && request.IsReloadCache && companyGroup != oldCompanyGroup)
			{
				companyGroup.SourceSubCreditCompanyCode = oldCompanyGroup.SubCreditCompanyCode;
				companyGroup.SourceParentCompanyCode = oldCompanyGroup.ParentCompanyCode;
				//和缓存里面的不一致,发送mq消息重新计算客户信用额度
				await _mgatewayService.SendMessage("platformAdmin_company_change_recalculate_credit_", JsonSerializerOptionsUtils.Serialize(companyGroup));
			}
			_mymoooContext.RedisCache.HashSet(companyGroup);
		}
	}
}
