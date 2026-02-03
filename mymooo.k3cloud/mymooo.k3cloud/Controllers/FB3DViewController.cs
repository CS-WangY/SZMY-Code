using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using mymooo.k3cloud.core.FB3DView;
using mymooo.k3cloud.Models;

namespace mymooo.k3cloud.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	public class FB3DViewController : Controller
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IActionResult Index()
		{
			return View();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet("3DViewUrl")]
		public string Get3DViewUrl()
		{
			string currurl = HttpContext.Request.GetDisplayUrl();
			string url = Regex.Replace(currurl, "3DViewUrl", "Get3DView", RegexOptions.IgnoreCase);
			return url;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet("Get3DView/request3dView")]
		public IActionResult Get3DView([FromQuery] Request3DView request3dView)
		{
			//request3dView.FPreviewUrl2D = HttpUtility.HtmlDecode(request3dView.FPreviewUrl2D);
			//request3dView.FPreviewUrl3D = HttpUtility.HtmlDecode(request3dView.FPreviewUrl3D);
			return View(request3dView);
		}
	}
}
