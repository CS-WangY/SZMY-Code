
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
 select Id,'scm','maintainProgress','价目表维护进度','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/maintainProgress','maintainprogress' from Menu where AppId = 'scm' and Title = '供应商价目表'
 go
 insert into RolesMenu(RoleId
      ,[MenuId]
      ,[IsRight]
      ,[CreateUser]
      ,[CreateDate])
select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'scm' and Name = 'maintainprogress'
