


insert into ScheduledTask(ScheduledCode,ScheduledName,Exchange,Routingkey,ExecuteCalss,ExecuteAction,CreateUserId,CreateUserCode,CreateUserName,CreateUserDate)
values('productReloadCache',N'加载产品缓存','BackgroundScheduled','product_Reload_cache_','com.mymooo.mall.business.Service.BaseService.ProductService,com.mymooo.mall.business','ReloadAllCache',134,'moyifeng',N'莫毅锋',getdate());

insert into ScheduledTask(ScheduledCode,ScheduledName,Exchange,Routingkey,ExecuteCalss,ExecuteAction,CreateUserId,CreateUserCode,CreateUserName,CreateUserDate)
values('productSmallReloadCache',N'加载产品小类缓存','BackgroundScheduled','product_small_Reload_cache_','com.mymooo.mall.business.Service.BaseService.ProductSmallService,com.mymooo.mall.business','ReloadAllCache',134,'moyifeng',N'莫毅锋',getdate());

insert into ScheduledTask(ScheduledCode,ScheduledName,Exchange,Routingkey,ExecuteCalss,ExecuteAction,CreateUserId,CreateUserCode,CreateUserName,CreateUserDate)
values('SystemProfileReloadCache',N'加载系统参数缓存','BackgroundScheduled','System_Profile_Reload_cache_','com.mymooo.mall.business.Service.SystemService.SystemProfileService,com.mymooo.mall.business','ReloadCache',134,'moyifeng',N'莫毅锋',getdate());

insert into ScheduledTask(ScheduledCode,ScheduledName,Exchange,Routingkey,ExecuteCalss,ExecuteAction,CreateUserId,CreateUserCode,CreateUserName,CreateUserDate)
values('managemetUserReloadCache',N'加载管理后台用户缓存','BackgroundScheduled','managemet_user_Reload_cache_','com.mymooo.mall.business.Service.BaseService.UserService,com.mymooo.mall.business','ReloadCache',134,'moyifeng',N'莫毅锋',getdate());

insert into ScheduledTask(ScheduledCode,ScheduledName,Exchange,Routingkey,ExecuteCalss,ExecuteAction,CreateUserId,CreateUserCode,CreateUserName,CreateUserDate)
values('SalesHistoryReloadCache',N'加载销售历史价缓存','BackgroundScheduled','sales_history_Reload_cache_','com.mymooo.mall.business.Service.SalesService.SalesOrderService,com.mymooo.mall.business','ReloadCache',134,'moyifeng',N'莫毅锋',getdate());
