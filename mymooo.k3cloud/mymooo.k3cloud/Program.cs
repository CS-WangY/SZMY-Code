using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Hosting.WindowsServices;
using mymooo.core.Account;
using mymooo.core.Filter;
using mymooo.core.HostedService;
using mymooo.core.Middleware;
using mymooo.core.Register;
using mymooo.k3cloud.business.Register;
using mymooo.k3cloud.business.Services;
using mymooo.k3cloud.core.Account;
using mymooo.product.selection.Common;
using mymooo.weixinWork.SDK.Config;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};
var builder = WebApplication.CreateBuilder(options);

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
    options.MaxModelValidationErrors = 50;
}); 
//获取或设置任何请求正文允许的最大大小（以字节为单位）。 如果设置为 null，则最大请求正文大小不受限制。 此限制对始终不受限制的升级连接没有影响
builder.WebHost.UseKestrel(options => options.Limits.MaxRequestBodySize = null);
builder.Services.AddControllers(config => config.Filters.Add(typeof(GlobalLogsAttribute<KingdeeContent, User>)));
//builder.Services.AddControllers(config => config.Filters.Add(typeof(TokenAttribute<KingdeeContent, User>)));

builder.Services.RegisterConfigure(builder.Configuration);
builder.Services.ReadConfig<WeiXinWorkConfig>(builder.Configuration);
builder.Services.ReadConfig<UrlConfig>(builder.Configuration);
builder.Services.RegisterAssembly("mymooo.core.dll", "mymooo.weixinWork.SDK.dll", "mymooo.pdm.SDK.dll", "mymooo.k3cloud.core.dll", "mymooo.k3cloud.business.dll"
    , "com.mymooo.credit.core.dll", "com.mymooo.credit.SDK.dll", "Mymooo.Threed.WebService.Core.dll", "Mymooo.Threed.WebService.SDK.dll", "mymooo.product.selection.dll");
builder.Services.AddHostedService<BackgroundScheduledJob<KingdeeContent, User>>();
builder.Services.AddHostedService<MainScheduledJob<KingdeeContent, User, MainScheduledService>>();
builder.Host.UseWindowsService();
builder.Services.RegisterSwaggerGen("蚂蚁工场金蝶服务API", "蚂蚁工场金蝶服务API", "mymooo.core.xml", "mymooo.weixinWork.SDK.xml", "mymooo.pdm.SDK.xml"
    , "mymooo.k3cloud.core.xml", "com.mymooo.credit.core.xml", "Mymooo.Threed.WebService.Core.xml", "mymooo.product.selection.xml");

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
await app.DatabaseInitialize<DatabaseInitialize, KingdeeContent, User>();
await app.ApplicationInitialize<KingdeeInitialize, KingdeeContent, User>();
app.UseMiddleware<AuthorizationLogsMiddleware<KingdeeContent, User>>();
app.UseSignLogin<KingdeeContent, User>((context, redirectUri) =>
{
    if (string.IsNullOrWhiteSpace(redirectUri))
    {
        context.Response.Redirect($"/");
    }
    else
    {
        context.Response.Redirect($"{redirectUri}");
    }
});
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
