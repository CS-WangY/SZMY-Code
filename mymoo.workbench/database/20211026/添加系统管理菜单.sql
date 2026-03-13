
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
      ,[Name]) values (
0, 'scm','/systemManage','系统管理', 'el-icon-setting',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),	'/index',''	
  )
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
	  select Id,'scm','supplierSetting','供应商配置', '',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),	'/systemManage/supplierSetting','suppliersetting'  from Menu where AppId = 'scm' and path = '/systemManage'
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
	  select Id,'scm','userManage','用户管理', '',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),	'/systemManage/userManage','usermanage'  from Menu where AppId = 'scm' and path = '/systemManage'

	  go
	  insert into RolesMenu(RoleId ,MenuId, IsRight, CreateUser, CreateDate) 
	  select 1,Id,1, 'YinSheng',GETDATE() from Menu where AppId = 'scm' and path in ('/systemManage','supplierSetting','userManage')
	  go
  
INSERT INTO [Menu]
           ([ParentId]
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
     VALUES
           (0,'scm','/supplierManage','供应商管理','el-icon-s-shop' ,10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/index','')
           

GO
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
select m.Id,'scm','priceList','供应商价目表','' ,10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList','pricelist'
from Menu m where m.AppId = 'scm' and m.Path = '/supplierManage'
GO
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
select m.Id,'scm','linearPriceList','线性价目表','' ,10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/linearPriceList','linearpricelist'
from Menu m where m.AppId = 'scm' and m.Path = 'priceList'
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
select m.Id,'scm','linearPriceListQuery','查询','' ,10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/linearPriceList/linearPriceListQuery','linearpricelistquery'
from Menu m where m.AppId = 'scm' and m.Path = 'linearPriceList'
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
select m.Id,'scm','linearPriceListImport','导入','' ,10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/linearPriceList/linearPriceListImport','linearpricelistimport'
from Menu m where m.AppId = 'scm' and m.Path = 'linearPriceList'
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
select m.Id,'scm','linearPriceListSubmit','待提交','' ,10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/linearPriceList/linearPriceListSubmit','linearpricelistsubmit'
from Menu m where m.AppId = 'scm' and m.Path = 'linearPriceList'
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
select m.Id,'scm','linearPriceListAudit','待审核','' ,10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/linearPriceList/linearPriceListAudit','linearpricelistaudit'
from Menu m where m.AppId = 'scm' and m.Path = 'linearPriceList'
go
-- 给管理员授权
insert into [RolesMenu]
           ([RoleId]
           ,[MenuId]
           ,[IsRight]
           ,[CreateUser]
           ,[CreateDate])
		select 1,m.Id, 1,'YinSheng',GETDATE()
 from Menu m where m.AppId = 'scm' and (m.Path = '/supplierManage' or m.Path = 'priceList' or m.Path = 'linearPriceList' or m.Path = 'linearPriceListQuery' or m.Path = 'linearPriceListImport' or m.Path = 'linearPriceListSubmit' or m.Path = 'linearPriceListAudit')



 insert into [RolesMenu]
           ([RoleId]
           ,[MenuId]
           ,[IsRight]
           ,[CreateUser]
           ,[CreateDate])
		select 1,m.Id, 1,'YinSheng',GETDATE()
 from Menu m where m.AppId = 'scm' and (m.Path = 'linearPriceListQuery' or m.Path = 'linearPriceListImport' or m.Path = 'linearPriceListSubmit' or m.Path = 'linearPriceListAudit')






