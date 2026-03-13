using com.mymooo.mall.business.Service.HostedService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.Register;
using Microsoft.Extensions.Options;
using mymooo.core.Account;
using mymooo.core.Config;
using mymooo.core.Filter;
using mymooo.core.HostedService;
using mymooo.core.Middleware;
using mymooo.core.Model.SqlSugarCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
	options.MaxModelValidationErrors = 50;
});
//获取或设置任何请求正文允许的最大大小（以字节为单位）。 如果设置为 null，则最大请求正文大小不受限制。 此限制对始终不受限制的升级连接没有影响
builder.WebHost.UseKestrel(options => options.Limits.MaxRequestBodySize = null);
//builder.Services.AddControllers(config => config.Filters.Add(typeof(GlobalLogsAttribute<MallContext, User, RabbitMQMessage>)));
builder.Services.AddControllers(config => config.Filters.Add(typeof(TokenAttribute<MallContext, User, RabbitMQMessage>)));
builder.Services.RegisterConfigure(builder.Configuration);
builder.Services.RegisterAssembly();
builder.Services.AddHostedService<MainScheduledJob<MainScheduledService, MymoooMainConfig>>();
builder.Services.AddHostedService<BackgroundScheduledJob<ScheduledTask, MymoooMainConfig>>();
var app = builder.Build();
#if DEBUG
using var scope = app.Services.CreateScope();
var mymoooMain = scope.ServiceProvider.GetRequiredService<IOptions<MymoooMainConfig>>();
mymoooMain.Value.IsRelease = false;
#endif

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<AuthorizationLogsMiddleware<MallContext, User, RabbitMQMessage>>();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
