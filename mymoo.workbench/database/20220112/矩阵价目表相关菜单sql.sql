-- 添加矩阵价目表相关菜单
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
select Id,'scm','matrixPriceList','矩阵价目表','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/matrixPriceList', 'matrixpricelist' from Menu where AppId = 'scm' and Name = 'pricelist'
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
select Id,'scm','query','查询','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/matrixPriceList/query', 'matrixpricelistquery' from Menu where AppId = 'scm' and Name = 'matrixpricelist'
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
select Id,'scm', 'import', '导入','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/matrixPriceList/import', 'matrixpricelistimport' from [Menu] where AppId = 'scm' and Name = 'matrixpricelist'
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
select Id,'scm', 'waitSubmit', '待提交','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/matrixPriceList/waitSubmit', 'matrixpricelistwaitsubmit' from [Menu] where AppId = 'scm' and Name = 'matrixpricelist'
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
select Id,'scm', 'waitAudit', '待审核','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/supplierManage/priceList/matrixPriceList/waitAudit', 'matrixpricelistwaitaudit' from [Menu] where AppId = 'scm' and Name = 'matrixpricelist'
go

insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 1,Id,1,'YinSheng',GETDATE() from [Menu] where AppId = 'scm' and Name in ('matrixpricelist','matrixpricelistquery','matrixpricelistimport','matrixpricelistwaitsubmit','matrixpricelistwaitaudit')