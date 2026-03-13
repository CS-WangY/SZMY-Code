-- 菜单---价目表下达组织 下达 下达历史 
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
select Id,'scm', 'sendOrg', '价目表下达组织','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/sendOrg', 'sendorg' from [Menu] where AppId = 'scm' and Path = 'priceList'
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
select Id,'scm', 'send', '下达','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/sendOrg/send', 'send' from [Menu] where AppId = 'scm' and Path = 'sendOrg'
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
select Id,'scm', 'sendHistory', '下达历史','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/sendOrg/sendHistory', 'sendhistory' from [Menu] where AppId = 'scm' and Path = 'sendOrg'
go
insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 1,Id,1,'YinSheng',GETDATE() from [Menu] where AppId = 'scm' and Path = 'sendOrg' or  Path = 'send' or Path = 'sendHistory'


