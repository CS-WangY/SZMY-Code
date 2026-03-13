

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
	  select Id,'scm','reserveApply','备库申请','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/reserveApply', 'reserveapply' from Menu where AppId = 'scm' and Title = '供应商管理'
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
	  select Id,'scm','import','导入','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/reserveApply/import', 'reserveapplyimport' from Menu where AppId = 'scm' and Title = '备库申请'
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
	  select Id,'scm','query','查询','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/reserveApply/query', 'reserveapplyquery' from Menu where AppId = 'scm' and Title = '备库申请'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'scm' and Title = '备库申请'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'scm' and name = 'reserveapplyimport'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'scm' and name = 'reserveapplyquery'