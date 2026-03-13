using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using mymooo.core.Attributes;
using mymooo.core.Cache;

namespace com.mymooo.mall.core.Cache
{
	/// <summary>
	/// 企业客户缓存
	/// </summary>
	[AutoInject(InJectType.Scope)]
	public class CompanyCacheService(RedisCache redisCache)
	{
		private readonly RedisCache _redisCache = redisCache;

		/// <summary>
		/// 获取公司客户信息
		/// </summary>
		/// <param name="companyCode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public Company? GetCompany(string companyCode)
		{
			if (string.IsNullOrWhiteSpace(companyCode))
			{
				throw new ArgumentException($"“{nameof(companyCode)}”不能为 null 或空白。", nameof(companyCode));
			}
			Company company = new() { Code = companyCode };
			return _redisCache.HashGet(company);
		}

		/// <summary>
		/// 获取公司客户信息
		/// </summary>
		/// <param name="companyId"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public Company? GetCompany(Guid? companyId)
		{
			if (companyId == null)
			{
				throw new ArgumentException($"“{nameof(companyId)}”不能为 null 或空白。", nameof(companyId));
			}
			Company company = new() { Id = companyId.Value };
			return _redisCache.HashGet(company);
		}

		/// <summary>
		/// 获取父级公司编码
		/// </summary>
		/// <param name="companyCode">公司编码</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public string GetParentCompanyCode(string companyCode)
		{
			if (string.IsNullOrWhiteSpace(companyCode))
			{
				throw new ArgumentException($"“{nameof(companyCode)}”不能为 null 或空白。", nameof(companyCode));
			}
			CompanyGroup companyGroup = new() { CompanyCode = companyCode };
			companyGroup.ParentCompanyCode = _redisCache.HashGet(companyGroup, p => p.ParentCompanyCode);
			if (string.IsNullOrWhiteSpace(companyGroup.ParentCompanyCode))
			{
				return companyCode;
			}
			else
			{
				return companyGroup.ParentCompanyCode;
			}
		}

		/// <summary>
		/// 获取统一信用管理父级公司编码
		/// </summary>
		/// <param name="companyCode">公司编码</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public string GetParentCreditCompanyCode(string companyCode)
		{
			if (string.IsNullOrWhiteSpace(companyCode))
			{
				throw new ArgumentException($"“{nameof(companyCode)}”不能为 null 或空白。", nameof(companyCode));
			}
			CompanyGroup companyGroup = new() { CompanyCode = companyCode };
			companyGroup.ParentCompanyCode = _redisCache.HashGet(companyGroup, p => p.ParentCompanyCode);
			if (!string.IsNullOrWhiteSpace(companyGroup.ParentCompanyCode) && _redisCache.HashGet(companyGroup, p => p.UnifiedCreditLine))
			{
				return companyGroup.ParentCompanyCode;
			}
			else
			{
				return companyCode;
			}
		}

		/// <summary>
		/// 判断客户是否启用了统一信用管理
		/// </summary>
		/// <param name="companyCode">公司编码</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public bool GetCreditUnifiedCreditLine(string companyCode)
		{
			if (string.IsNullOrWhiteSpace(companyCode))
			{
				throw new ArgumentException($"“{nameof(companyCode)}”不能为 null 或空白。", nameof(companyCode));
			}
			CompanyGroup companyGroup = new() { CompanyCode = companyCode };

			return _redisCache.HashGet(companyGroup, p => p.UnifiedCreditLine);
		}

		/// <summary>
		/// 获取全部集团子公司编码
		/// </summary>
		/// <param name="companyCode">公司编码</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public List<string>? GetSubCompanyCodes(string companyCode)
		{
			if (string.IsNullOrWhiteSpace(companyCode))
			{
				throw new ArgumentException($"“{nameof(companyCode)}”不能为 null 或空白。", nameof(companyCode));
			}
			CompanyGroup companyGroup = new() { CompanyCode = companyCode };
			return _redisCache.HashGet(companyGroup, p => p.SubCompanyCode);
		}

		/// <summary>
		/// 获取统一信用管理集团子公司编码
		/// </summary>
		/// <param name="companyCode">公司编码</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public List<string>? GetSubCreditCompanyCodes(string companyCode)
		{
			if (string.IsNullOrWhiteSpace(companyCode))
			{
				throw new ArgumentException($"“{nameof(companyCode)}”不能为 null 或空白。", nameof(companyCode));
			}
			CompanyGroup companyGroup = new() { CompanyCode = companyCode };
			return _redisCache.HashGet(companyGroup, p => p.SubCreditCompanyCode);
		}
	}
}
