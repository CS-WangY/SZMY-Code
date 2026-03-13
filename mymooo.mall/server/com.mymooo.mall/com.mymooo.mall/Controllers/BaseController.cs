using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace com.mymooo.mall.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class BaseController : Controller
	{

		private readonly List<Task> _tasks;

		/// <summary>
		/// 
		/// </summary>
		protected BaseController()
		{
			_tasks = [];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="models"></param>
		/// <returns></returns>
		public async Task<ResponseMessage<dynamic>> ModelVerify<T>(List<T> models) where T : class
		{
			ResponseMessage<dynamic> response = new()
			{
				ErrorMessages = []
			};
			int row = 1;
			foreach (var model in models)
			{
				var result = await this.TryUpdateModelAsync(model);

				if (!result)
				{
					response.Code = ResponseCode.ModelError;
					foreach (var error in this.ModelState)
					{
						foreach (var item in error.Value.Errors)
						{
							response.ErrorMessages.Add($"第{row}行:{error.Key}:{item.ErrorMessage}");
						}
					}
				}
				row++;
			}
			if (response.ErrorMessages.Count == 0)
			{
				response.Code = ResponseCode.Success;
			}
			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<ResponseMessage<dynamic>> ModelVerify<T>(T model) where T : class
		{
			ResponseMessage<dynamic> response = new();
			var result = await this.TryUpdateModelAsync(model);

			if (!result)
			{
				response.Code = ResponseCode.ModelError;
				response.ErrorMessages = [];
				foreach (var error in this.ModelState)
				{
					foreach (var item in error.Value.Errors)
					{
						response.ErrorMessages.Add($"{error.Key}:{item.ErrorMessage}");
					}
				}
			}
			else
			{
				response.Code = ResponseCode.Success;
			}
			return response;
		}
		//[NonAction]
		//      protected virtual string RenderPartialViewToString(string viewName, object model)
		//      {
		//          if (string.IsNullOrEmpty(viewName))
		//              viewName = ControllerContext.RouteData.GetRequiredString("action");

		//          ViewData.Model = model;

		//          using (var sw = new StringWriter())
		//          {
		//              var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
		//              var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
		//              viewResult.View.Render(viewContext, sw);

		//              return sw.GetStringBuilder().ToString();
		//          }
		//      }



		//   protected virtual ContentResult ReturnJson(object data)
		//   {
		//    return Content(JsonHelper.CamelCaseJson(data));
		//}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="task"></param>
		protected virtual void AddTask(Task task)
		{
			_tasks.Add(task);
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void RunTasks()
		{
			foreach (var task in _tasks)
			{
				task.Start();
			}
			Task.WaitAll([.. _tasks]);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		protected virtual ContentResult ReturnJson(object data)
		{
			return Content(JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			}));
		}

	}
}