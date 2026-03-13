
  insert into Menu([ParentId]
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
	  select Id,'crm','stockApply','备货申请','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/salesOrderManage/stockApply', 'stockapply' from Menu where AppId = 'crm' and Title = '销售管理'
	  go
	  insert into Menu([ParentId]
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
	  select Id,'crm','import','导入','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/salesOrderManage/stockApply/import', 'stockapplyimport' from Menu where AppId = 'crm' and Title = '备货申请'
	  go
	  insert into Menu([ParentId]
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
	  select Id,'crm','query','查询','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/salesOrderManage/stockApply/query', 'stockapplyquery' from Menu where AppId = 'crm' and Title = '备货申请'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'crm' and Title = '备货申请'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'crm' and name = 'stockapplyimport'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'crm' and name = 'stockapplyquery'