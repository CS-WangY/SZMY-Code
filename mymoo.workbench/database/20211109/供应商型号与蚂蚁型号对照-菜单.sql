-- 菜单---供应商型号和蚂蚁型号对照表 导入 删除 待提交 待审核 
insert into [Menu]([ParentId]
      ,[AppId]
      ,[Path]
      ,[Title]
      ,[Icon]
      ,[Sort]
      ,[IsPublish]
      ,[Description]
      ,[CreateUser]
      ,[CreateDate]
      ,[PublishUser]
      ,[PublishDate]
      ,[Component]
      ,[Name])
select Id,'scm', 'modelComparison', '供应商和蚂蚁型号对照表','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/modelComparison', 'modelcomparison' from [Menu] where AppId = 'scm' and Path = '/systemManage'
go
insert into [Menu]([ParentId]
      ,[AppId]
      ,[Path]
      ,[Title]
      ,[Icon]
      ,[Sort]
      ,[IsPublish]
      ,[Description]
      ,[CreateUser]
      ,[CreateDate]
      ,[PublishUser]
      ,[PublishDate]
      ,[Component]
      ,[Name])
select Id,'scm', 'query', '查询','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/modelComparison/query', 'modelcomparequery' from [Menu] where AppId = 'scm' and Path = 'modelComparison'
go
insert into [Menu]([ParentId]
      ,[AppId]
      ,[Path]
      ,[Title]
      ,[Icon]
      ,[Sort]
      ,[IsPublish]
      ,[Description]
      ,[CreateUser]
      ,[CreateDate]
      ,[PublishUser]
      ,[PublishDate]
      ,[Component]
      ,[Name])
select Id,'scm', 'import', '导入','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/modelComparison/import', 'modelcompareimport' from [Menu] where AppId = 'scm' and Path = 'modelComparison'
go
insert into [Menu]([ParentId]
      ,[AppId]
      ,[Path]
      ,[Title]
      ,[Icon]
      ,[Sort]
      ,[IsPublish]
      ,[Description]
      ,[CreateUser]
      ,[CreateDate]
      ,[PublishUser]
      ,[PublishDate]
      ,[Component]
      ,[Name])
select Id,'scm', 'delete', '删除','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/modelComparison/delete', 'modelcomparedelete' from [Menu] where AppId = 'scm' and Path = 'modelComparison'
go
insert into [Menu]([ParentId]
      ,[AppId]
      ,[Path]
      ,[Title]
      ,[Icon]
      ,[Sort]
      ,[IsPublish]
      ,[Description]
      ,[CreateUser]
      ,[CreateDate]
      ,[PublishUser]
      ,[PublishDate]
      ,[Component]
      ,[Name])
select Id,'scm', 'waitSubmit', '待提交','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/modelComparison/waitSubmit', 'modelcomparewaitsubmit' from [Menu] where AppId = 'scm' and Path = 'modelComparison'
go
insert into [Menu]([ParentId]
      ,[AppId]
      ,[Path]
      ,[Title]
      ,[Icon]
      ,[Sort]
      ,[IsPublish]
      ,[Description]
      ,[CreateUser]
      ,[CreateDate]
      ,[PublishUser]
      ,[PublishDate]
      ,[Component]
      ,[Name])
select Id,'scm', 'waitAudit', '待审核','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/modelComparison/waitAudit', 'modelcomparewaitaudit' from [Menu] where AppId = 'scm' and Path = 'modelComparison'
go

insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 1,Id,1,'YinSheng',GETDATE() from [Menu] where AppId = 'scm' and Path = 'modelComparison' or Component = '/systemManage/modelComparison/import' 
or Component = '/systemManage/modelComparison/delete' or Component = '/systemManage/modelComparison/waitSubmit' or Component = '/systemManage/modelComparison/waitAudit'
or Component = '/systemManage/modelComparison/query'
