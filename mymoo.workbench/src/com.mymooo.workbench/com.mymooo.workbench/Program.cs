using com.mymooo.workbench.business.QuartzTask.WeiXinWork;
using com.mymooo.workbench.business.Register;
using com.mymooo.workbench.business.WebSocket;
using com.mymooo.workbench.cache.ElasticSearch.Service;
using com.mymooo.workbench.core;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Expressage.DeBang;
using com.mymooo.workbench.core.Expressage.JiaYunMei;
using com.mymooo.workbench.core.Expressage.Kuayue;
using com.mymooo.workbench.core.Expressage.ShunFeng;
using com.mymooo.workbench.core.Expressage.SuTeng;
using com.mymooo.workbench.core.Mail;
using com.mymooo.workbench.core.Minio;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mymooo.core.Account;
using mymooo.core.Filter;
using mymooo.core.HostedService;
using mymooo.core.Middleware;
using mymooo.core.Register;
using mymooo.weixinWork.SDK.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.MaxModelValidationErrors = 50;
});
//获取或设置任何请求正文允许的最大大小（以字节为单位）。 如果设置为 null，则最大请求正文大小不受限制。 此限制对始终不受限制的升级连接没有影响
builder.WebHost.UseKestrel(options => options.Limits.MaxRequestBodySize = null);
builder.Services.AddControllers(config => config.Filters.Add(typeof(GlobalLogsAttribute<WorkbenchContext, User>)));
builder.Services.AddControllers(config => config.Filters.Add(typeof(TokenAttribute<WorkbenchContext, User>)));
builder.Services.RegisterConfigure(builder.Configuration);
builder.Services.Configure<WeiXinWorkConfig>(builder.Configuration.GetSection(nameof(WeiXinWorkConfig)));
builder.Services.Configure<K3CloudConfig>(builder.Configuration.GetSection("K3CloudConfig"));
builder.Services.Configure<CloudStockConfig>(builder.Configuration.GetSection("CloudStockConfig"));
builder.Services.Configure<KuayueExpressageInfo>(builder.Configuration.GetSection("kyExpressage"));
builder.Services.Configure<ShunFengExpressageInfo>(builder.Configuration.GetSection("sfExpressage"));
builder.Services.Configure<DeBangExpressageInfo>(builder.Configuration.GetSection("dbExpressage"));
builder.Services.Configure<SuTengExpressageInfo>(builder.Configuration.GetSection("stExpressage"));
builder.Services.Configure<MinioConfig>(builder.Configuration.GetSection("MinioSettings"));
builder.Services.Configure<MailServiceConfig>(builder.Configuration.GetSection("MailServiceConfig"));
builder.Services.Configure<JiaYunMeiExpressageInfo>(builder.Configuration.GetSection("jiayunmeiExpressage"));
builder.Services.Configure<CloudStockMinioConfig>(builder.Configuration.GetSection("CloudStockMinioSettings"));

var sqlConnection = builder.Configuration.GetConnectionString("SqlServerConnection");
builder.Services.AddDbContext<WorkbenchDbContext>(option => option.UseSqlServer(sqlConnection));
builder.Services.AddDbContext<WeixinDbContext>(option => option.UseSqlServer(sqlConnection));
builder.Services.AddDbContext<MessageScheduledDbContext>(option => option.UseSqlServer(sqlConnection));

builder.Services.RegisterAssembly("mymooo.core.dll", "mymooo.weixinWork.SDK.dll", "com.mymooo.api.gateway.SDK.dll", "com.mymooo.workbench.dll", "com.mymooo.workbench.core.dll", "com.mymooo.workbench.business.dll", "com.mymooo.workbench.cache.dll", "com.mymooo.workbench.weixin.work.dll", "com.mymooo.workbench.qichacha.dll");

builder.Services.AddSingleton<IElasticSearchService, ElasticSearchService>();
builder.Services.AddSingleton<IElasticSearchJsonService, ElasticSearchJsonService>();
builder.Services.AddHostedService<MainScheduledJob<WorkbenchContext, User, MessageScheduled>>();
builder.Services.AddHostedService<BackgroundScheduledJob<WorkbenchContext, User>>();
builder.Services.AddSignalR();

builder.Services.RegisterSwaggerGen("蚂蚁工场统一平台服务API", "蚂蚁工场统一平台服务API", "mymooo.core.xml", "mymooo.weixinWork.SDK.xml", "com.mymooo.workbench.xml", "com.mymooo.workbench.core.xml");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
await app.ApplicationInitialize<WorkbenchInitialize, WorkbenchContext, User>();
app.UseMiddleware<AuthorizationLogsMiddleware<WorkbenchContext, User>>();
app.UseMiddleware<AuthorizationUserMiddleware>();
app.UseSignLogin<WorkbenchContext, User>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/ChatHub", options =>
{
    options.AllowStatefulReconnects = true;
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
