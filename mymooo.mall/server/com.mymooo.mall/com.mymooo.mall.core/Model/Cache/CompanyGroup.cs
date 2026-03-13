using mymooo.core.Attributes.Redis;
using System.ComponentModel.DataAnnotations;

namespace com.mymooo.mall.core.Model.Cache
{
	/// <summary>
	/// 集团公司组
	/// </summary>
	[RedisKey("mymooo-company", 2, false)]
	public class CompanyGroup
	{
		/// <summary>
		/// 公司编码
		/// </summary>
		[RedisPrimaryKey]
		[Required(ErrorMessage = "客户编码不能为空!")]
		[StringLength(30, ErrorMessage = "客户编码不能超过30个字符!")]
		public string? CompanyCode { get; set; }

		/// <summary>
		/// 子公司集合
		/// </summary>
		[RedisValue(true)]
		public List<string>? SubCompanyCode { get; set; }
		
		/// <summary>
		/// 父级公司
		/// </summary>
		[RedisValue]
		public string? ParentCompanyCode { get; set; }

		/// <summary>
		/// 源父级公司
		/// </summary>
		public string? SourceParentCompanyCode { get; set; }

		/// <summary>
		/// 是否统一信用管理
		/// </summary>
		[RedisValue]
		public bool UnifiedCreditLine { get; set; }

		/// <summary>
		/// 统一信用管理子公司编码
		/// </summary>
		[RedisValue(true)]
		public List<string>? SubCreditCompanyCode { get; set; }

		/// <summary>
		/// 源子公司
		/// </summary>
		public List<string>? SourceSubCreditCompanyCode { get; set; }

		/// <summary>
		/// 比较
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object? obj)
		{
			return base.Equals(obj);
		}

		/// <summary>
		/// 比较是否相等
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
#pragma warning disable CS8602 // 解引用可能出现空引用。
		public static bool operator ==(CompanyGroup? left, CompanyGroup? right)
		{
			if (left is null && right is null)
			{
				return true;
			}

			if (left.UnifiedCreditLine != right.UnifiedCreditLine)
			{
				return false;
			}

			if ((left is null && right is not null) || (left is not null && right is null))
			{
				return false;
			}

			if (left.CompanyCode != right.CompanyCode)
			{
				return false;
			}

			if (left.ParentCompanyCode != right.ParentCompanyCode)
			{
				return false;
			}

			if (left.SubCreditCompanyCode == null && right.SubCreditCompanyCode == null)
			{
				return true;
			}

			if ((left.SubCreditCompanyCode == null && right.SubCreditCompanyCode != null) || (left.SubCreditCompanyCode != null && right.SubCreditCompanyCode == null))
			{
				return false;
			}

			if (left.SubCreditCompanyCode.Count != right.SubCreditCompanyCode?.Count)
			{
				return false;
			}
			foreach (var item in left.SubCreditCompanyCode)
			{
				if (!right.SubCreditCompanyCode.Exists(p => p == item))
				{
					return false;
				}
			}
			return true;
		}
#pragma warning restore CS8602 // 解引用可能出现空引用。

		/// <summary>
		/// 比较是否不相等
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(CompanyGroup? left, CompanyGroup? right)
		{
			return !(left == right);
		}

		/// <summary>
		/// 必须的,不知道做什么的
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return HashCode.Combine(CompanyCode, ParentCompanyCode, SubCompanyCode);
		}
	}
}
