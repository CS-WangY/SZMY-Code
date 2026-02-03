using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mymooo.core.Attributes;
using mymooo.k3cloud.core.Account;

namespace mymooo.k3cloud.Controllers
{
	/// <summary>
	/// home
	/// </summary>
	public class HomeController(KingdeeContent kingdeeContent) : Controller
	{
		private readonly KingdeeContent _kingdeeContent = kingdeeContent;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[IgnoreLog]
		public IActionResult Index()
        {
			return View();
		}

		/// <summary>
		/// 测试
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]
		[IgnoreLog]
		public IActionResult Test()
        {
			return Ok();
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
		/// 创建orm实体
		/// </summary>
		/// <returns></returns>
		[IgnoreLog]
		public IActionResult CreateEntity()
		{
			List<string> tables =
			[
                "T_SAL_RETURNNOTICE"
            ];

			_kingdeeContent.SqlSugar.DbFirst.SettingNamespaceTemplate(old => { return old + "\r\nusing SqlSugar;"; })
	.SettingClassTemplate(old =>
	{
		return @"{using}
namespace {Namespace}
{
{ClassDescription}{SugarTable}
    public partial class {ClassName}
    {
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
	}).Where(it => tables.Exists(p => p.Equals(it, StringComparison.OrdinalIgnoreCase))).CreateClassFile("E:\\code\\gitlab\\mymooo\\k3cloudApi\\src\\mymooo.k3cloud\\mymooo.k3cloud.core\\SqlSugarCore\\Sales", "mymooo.k3cloud.core.SqlSugarCore.Sales");

			return Ok();
		}
	}
}
