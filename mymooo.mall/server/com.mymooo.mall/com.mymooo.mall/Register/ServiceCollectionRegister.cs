using com.mymooo.mall.core.Config;
using com.mymooo.mall.wcf;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Config;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.Config;
using SqlSugar;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace com.mymooo.mall.Register
{
	/// <summary>
	/// 依赖注入
	/// </summary>
	public static class ServiceCollectionRegister
	{
		/// <summary>
		/// 注入配置
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public static IServiceCollection RegisterConfigure(this IServiceCollection services, ConfigurationManager configuration)
		{
			services.AddHttpClient();
			services.AddMvc().AddJsonOptions(options =>
			{
				//不返回值为NULL的属性
				options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
				//设置不使用驼峰格式处理，由后台字段确定大小写
				//options.JsonSerializerOptions.PropertyNamingPolicy = null;
				//配置序列化时时间格式为yyyy-MM-dd HH:mm:ss
				options.JsonSerializerOptions.Converters.Add(new LongConverter());
				options.JsonSerializerOptions.Converters.Add(new IntConverter());
				options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
				options.JsonSerializerOptions.Converters.Add(new DateTimeNullConverter());
				options.JsonSerializerOptions.Converters.Add(new DecimalConverter());
				options.JsonSerializerOptions.Converters.Add(new BoolConverter());
				options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
				//options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
				//忽略循环引用
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
				//允许多余的,号
				options.JsonSerializerOptions.AllowTrailingCommas = true;
			});

			services.Configure<ApigatewayConfig>(configuration.GetSection("apigatewayConfig"));
			services.Configure<RsaConfig>(configuration.GetSection("rsaConfig"));
			services.Configure<MymoooMainConfig>(configuration.GetSection("mallMain"));
			services.Configure<RedisConfig>(configuration.GetSection("redisConfig"));
			services.Configure<GatewayRedisConfig>(configuration.GetSection(nameof(GatewayRedisConfig)));
			services.Configure<MymoooLogsConfig>(configuration.GetSection("elasticsearchConfig"));
			services.Configure<ElasticsearchConfig>(configuration.GetSection("elasticsearchConfig"));
			services.Configure<WcfConfig>(configuration.GetSection("wcfConfig"));
			services.Configure<WeiXinWorkConfig>(configuration.GetSection(nameof(WeiXinWorkConfig)));

			services.Configure<FormOptions>(options =>
			{
				options.ValueLengthLimit = int.MaxValue;
				options.MultipartBodyLengthLimit = long.MaxValue; // In case of multipart
				options.MultipartHeadersLengthLimit = int.MaxValue;
			});

			var elasticsearchConfig = services.BuildServiceProvider().GetService<IOptions<MymoooLogsConfig>>()?.Value;
			if (elasticsearchConfig != null)
			{
				services.TryAddSingleton(s =>
				{
					ElasticsearchClient elasticsearchClient = new(new ElasticsearchClientSettings(new Uri(elasticsearchConfig.Url))
						.DefaultMappingFor<RequestLogs>(i => i
							.IndexName(elasticsearchConfig.LogIndex)
							.IdProperty(p => p.Id)
						).EnableDebugMode().PrettyJson().RequestTimeout(TimeSpan.FromMinutes(10)));
					return elasticsearchClient;
				});
			}
			return services;
		}

		/// <summary>
		/// 注入type
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection RegisterAssembly(this IServiceCollection services)
		{
			var path = AppDomain.CurrentDomain.BaseDirectory;
			List<string> files =
			[
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mymooo.core.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mymooo.k3cloud.core.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mymooo.k3cloud.SDK.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "com.mymooo.mall.business.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "com.mymooo.mall.core.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "com.mymooo.mall.wcf.dll"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mymooo.weixinWork.SDK.dll"),
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
