--功能管理添加启用人启用时间字段
alter table MenuItem add  EnableUser varchar(30)  null
alter table MenuItem add  EnableDate datetime  null