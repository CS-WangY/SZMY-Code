-- 添加菜单--数据分析--报价转订购数据分析
insert into Menu(
[ParentId]
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
      ,[Name]
) values (0,'scm','/dataAnalysis','数据分析','excel',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/index','')
go
insert into Menu(
[ParentId]
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
      ,[Name]
)
select Id,'scm','quotetoorder','报价转订购','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/dataAnalysis/quotetoorder/index','quotetoorder' from Menu where AppId = 'scm' and Path = '/dataAnalysis'
go
insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'scm' and Path in ('/dataAnalysis', 'quotetoorder')
