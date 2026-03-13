using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace com.mymooo.mall.core.Attributes.Validation
{
	/// <summary>
	/// 产品小类校验
	/// </summary>
	/// <param name="publish"></param>
	/// <param name="leaf"></param>
	public class ProductSmallAttribute(bool publish = false, bool leaf = false) : ValidationAttribute
	{
		private readonly bool _publish = publish;
		private readonly bool _leaf = leaf;

		/// <summary>
		/// 校验
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return ValidationResult.Success;
			}
			string key = value.ToString() ?? "0";
			_ = long.TryParse(key, out long id);
			if (id == 0)
			{
				return ValidationResult.Success;
			}
			MallContext context = validationContext.GetRequiredService<MallContext>();
			ProductSmallClass smallClass = new() { Id = id };
			if (context.RedisCache.HashExists(smallClass))
			{
				if (!_publish && !_leaf)
				{
					return ValidationResult.Success;
				}
				if (_leaf && !context.RedisCache.HashGet(smallClass, p => p.IsLeaf))
				{
					return new ValidationResult("产品小类不是叶子节点!");
				}

				if (_publish && !context.RedisCache.HashGet(smallClass, p => p.IsPublish))
				{
					return new ValidationResult("产品小类没有发布!");
				}
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult("产品小类在系统中不存在!");
			}
		}
	}
}
