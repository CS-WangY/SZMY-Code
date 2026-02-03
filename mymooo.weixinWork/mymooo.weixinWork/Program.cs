using mymooo.core.Account;
using mymooo.core.Register;
using mymooo.weixinWork.Models;
using mymooo.weixinWork.SDK.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
	options.MaxModelValidationErrors = 50;
});
//获取或设置任何请求正文允许的最大大小（以字节为单位）。 如果设置为 null，则最大请求正文大小不受限制。 此限制对始终不受限制的升级连接没有影响
builder.WebHost.UseKestrel(options => options.Limits.MaxRequestBodySize = null);
builder.Services.AddHttpClient();
builder.Services.RegisterConfigure(builder.Configuration);
builder.Services.Configure<WeiXinWorkConfig>(builder.Configuration.GetSection(nameof(WeiXinWorkConfig)));
builder.Services.RegisterAssembly("mymooo.core.dll", "mymooo.weixinWork.dll", "mymooo.weixinWork.SDK.dll");
builder.Services.RegisterSwaggerGen("蚂蚁工场产品服务API", "蚂蚁工场产品服务API", "mymooo.core.xml");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
await app.ApplicationInitialize<WeiXinWorkInitialize, WeixinWorkContext, User>();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
