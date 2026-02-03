using Microsoft.AspNetCore.Mvc;
using mymooo.weixinWork.Models;
using SqlSugar;
using System.Diagnostics;

namespace mymooo.weixinWork.Controllers
{
	public class HomeController(WeixinWorkContext weixinWorkContext) : Controller
	{
		private readonly WeixinWorkContext _weixinWorkContext = weixinWorkContext;

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult CreateEntity()
		{
			List<string> tables = [];
			tables.Add("ApprovalTemplate");
			tables.Add("ApprovalTemplateField");
			tables.Add("AuditFlowConfig");
			tables.Add("AuditFlowConfigDetail");

			_weixinWorkContext.SqlSugar.DbFirst.SettingNamespaceTemplate(old => { return old + "\r\nusing SqlSugar;"; })
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
	}).Where(it => tables.Exists(p => p.Equals(it, StringComparison.OrdinalIgnoreCase))).CreateClassFile("E:\\code\\gitlab\\mymooo\\mymooo.weixinWork\\src\\mymooo.weixinWork\\mymooo.weixinWork.SDK\\SqlSugarCore", "mymooo.weixinWork.SDK.SqlSugarCore");

			return Ok();
		}
	}
}
