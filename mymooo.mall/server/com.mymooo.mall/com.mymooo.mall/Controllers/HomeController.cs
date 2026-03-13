using com.mymooo.mall.business.Service.BaseService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using com.mymooo.mall.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mymooo.core.Attributes;
using System.Diagnostics;

namespace com.mymooo.mall.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="mymoooContext"></param>
	/// <param name="productSmallService"></param>
	public class HomeController(MallContext mymoooContext,ProductSmallService productSmallService) : Controller
	{
		private readonly MallContext _mymoooContext = mymoooContext;
        private readonly ProductSmallService _productSmallService = productSmallService;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [AllowAnonymous]
		[IgnoreLog]
		public IActionResult Index()
		{
			var defaultSalemanId = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "SALESMAN" }, p => p.Value);
			return View();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]
		[IgnoreLog]
		public IActionResult Privacy()
		{
			return View();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [AllowAnonymous]
        [IgnoreLog]
        public IActionResult ClearSmallClassCache()
        {        
			_productSmallService.ReloadAllCache();
            return Ok();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [AllowAnonymous]
        [IgnoreLog]
        public IActionResult ClearCustomerSupplyOrgCache()
        {
            _productSmallService.ReloadCustomerSupplyOrgCache();
            return Ok();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		[AllowAnonymous]
		[IgnoreLog]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]
		[IgnoreLog]
		public IActionResult CreateEntity()
		{
			List<string> tables = [];
            tables.Add("Express");
            //tables.Add("ScheduledTask");
			//tables.Add("F_CUST_BOOK_MSTR");
			//tables.Add("F_CUST_BOOK_DETAIL");
			//tables.Add("F_CUST_QUOTATION_MSTR");
			//tables.Add("F_CUST_QUOTATION_DETAIL");
			//tables.Add("ResolvedOrder");
			//tables.Add("ResolvedHistory");
			//tables.Add("ResolvedOrderChangeHistory");
			//tables.Add("F_CUST_ADDR");
			//tables.Add("Company");


            _mymoooContext.SqlSugar.DbFirst.SettingNamespaceTemplate(old => { return old + "\r\nusing SqlSugar;"; })
	.SettingClassTemplate(old =>
	{
		return @"{using}
namespace {Namespace}
{
{ClassDescription}{SugarTable}
    public partial class {ClassName}
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public {ClassName}()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{
{Constructor}
        }

{PropertyName}
    }
}";
	}).SettingPropertyDescriptionTemplate(old =>
	{
		return @"        /// <summary>
        /// Desc:{PropertyDescription}
        /// Default:{DefaultValue}
        /// Nullable:{IsNullable}
        /// </summary>";
	})
	.SettingPropertyTemplate((columns, temp, type) =>
	{
		var columnattribute = "\r\n        [SugarColumn({0})]";
		List<string> attributes = [];
		if (columns.IsPrimarykey)
			attributes.Add("IsPrimaryKey = true");
		if (columns.IsIdentity)
			attributes.Add("IsIdentity = true");
		if (attributes.Count == 0)
		{
			columnattribute = "";
		}

		temp = temp.Replace("{PropertyType}", type)
					.Replace("{PropertyName}", columns.DbColumnName)
					.Replace("{SugarColumn}", string.Format(columnattribute, string.Join(", ", attributes)))
					.Replace("           public", "        public");
		if (type == "string")
		{
			temp = temp.Replace("{get;set;}", "{ get; set; } = string.Empty;");
		}
		else
		{
			temp = temp.Replace("{get;set;}", "{ get; set; }");
		}
		return temp;
	}).Where(it => tables.Exists(p => p.Equals(it, StringComparison.OrdinalIgnoreCase))).CreateClassFile("E:\\code\\gitlab\\mymooo\\mall\\server\\com.mymooo.mall\\com.mymooo.mall.core\\SqlSugarCore\\BaseInformation\\CustomerInformation", "com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation");

			return Ok();
		}
	}
}