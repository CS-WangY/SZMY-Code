
insert into ScheduledTask(ScheduledCode,ScheduledName,Exchange,Routingkey,ExecuteCalss,ExecuteAction,CreateUserId,CreateUserCode,CreateUserName,CreateUserDate)
values('companyReloadCache',N'加载客户缓存','BackgroundScheduled','company_Reload_cache_','com.mymooo.mall.business.Service.BaseService.CompanyService,com.mymooo.mall.business','ReloadCache',134,'moyifeng',N'莫毅锋',getdate());

insert into ScheduledTask(ScheduledCode,ScheduledName,Exchange,Routingkey,ExecuteCalss,ExecuteAction,CreateUserId,CreateUserCode,CreateUserName,CreateUserDate)
values('companyGroupReloadCache',N'加载集团客户缓存','BackgroundScheduled','companyGroup_Reload_cache_','com.mymooo.mall.business.Service.BaseService.CompanyService,com.mymooo.mall.business','ReloadGroupCache',134,'moyifeng',N'莫毅锋',getdate());


