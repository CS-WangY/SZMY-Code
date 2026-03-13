
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
	  select Id,'crm','quotation','报价单','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/salesOrderManage/quotation', 'quotation' from Menu where AppId = 'crm' and Title = '销售管理'
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
	  select Id,'crm','import','模拟报价','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/salesOrderManage/quotation/import', 'quotationimport' from Menu where AppId = 'crm' and Title = '报价单'
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
	  select Id,'crm','query','查询','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/salesOrderManage/quotation/query', 'quotationquery' from Menu where AppId = 'crm' and Title = '报价单'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'crm' and Title = '报价单'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'crm' and name = 'quotationimport'
	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'crm' and name = 'quotationquery'