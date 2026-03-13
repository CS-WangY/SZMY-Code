-- 添加菜单-供应商成本价维护
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
 select Id,'scm','costPriceMaintain','供应商成本价维护','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/costPriceMaintain','costpricemaintain' from [Menu] where AppId = 'scm' and path = '/supplierManage'
go
 insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
 select 1,Id,1,'YinSheng',GETDATE() from [Menu] where AppId = 'scm' and path = 'costPriceMaintain'