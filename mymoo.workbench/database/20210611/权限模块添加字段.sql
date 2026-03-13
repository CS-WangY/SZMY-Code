--Roles表添加字段IsAdmin
alter table Roles add IsAdmin bit not null default(0)

--Menu表添加字段Component、Name，设置PublishUser允许为null
alter table Menu add  Component varchar(500) not null default('')
alter table Menu add  Name varchar(30) not null default('')
alter table Menu alter column PublishUser varchar(30) null;

--ThirdpartyApplicationConfig表添加字段IsWeixinWork
alter table ThirdpartyApplicationConfig add IsWeixinWork bit not null default(0)