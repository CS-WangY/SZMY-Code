using Microsoft.Extensions.DependencyInjection;
using mymooo.core.Attributes;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace com.mymooo.workbench.Register
{
    /// <summary>
    /// ioc 注入
    /// </summary>
    public static class ServiceCollectionRegister
	{
		/// <summary>
		/// 动态注入
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection RegisterAssembly(this IServiceCollection services)
		{
			var path = AppDomain.CurrentDomain.BaseDirectory;
			List<string> files =
			[
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mymooo.core.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mymooo.weixinWork.SDK.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "com.mymooo.api.gateway.SDK.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "com.mymooo.workbench.core.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "com.mymooo.workbench.business.dll"),
			]; 
			foreach (var file in files)
			{
				var types = Assembly.LoadFrom(file).GetTypes().Where(a => a.GetCustomAttribute<AutoInjectAttribute>() != null).ToList();
				if (types.Count <= 0)
				{
					continue;
				}
				foreach (var type in types)
				{
					var attr = type.GetCustomAttribute<AutoInjectAttribute>();
					switch (attr?.InJectType)
					{
						case InJectType.Scope:
							services.AddScoped(type);
							break;
						case InJectType.Single:
							services.AddSingleton(type);
							break;
						case InJectType.Transient:
							services.AddTransient(type);
							break;
					}
				}
			}
			return services;
		}
	}
}
