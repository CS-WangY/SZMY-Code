use mymooo_workbench

  insert into [Menu] values
  (0,'workbench', '/externallink', '其他系统管理', 'link', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/index', '')
  go
  declare @id1 bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = '/externallink')
  insert into [Menu] values
  (@id1,'workbench', '', '蚂蚁平台', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'', 'platformAdmin'),
  (@id1,'workbench', '', 'CRM', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'', 'crm'),
  (@id1,'workbench', '', '报表', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'', 'report')


   go

DECLARE @temp TABLE
  (
      Id bigint
  );
INSERT INTO @temp(Id) SELECT Id FROM Roles

declare @roleId bigint
declare @id1 bigint
declare @id2 bigint
declare @id3 bigint
declare @id4 bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = '/externallink')
  set @id2 = (select Id from [Menu] where AppId = 'workbench' and Name = 'platformAdmin')
  set @id3 = (select Id from [Menu] where AppId = 'workbench' and Name = 'crm')
  set @id4 = (select Id from [Menu] where AppId = 'workbench' and Name = 'report')

while exists(select Id from @temp)
begin
    SET ROWCOUNT 1
	SELECT @roleId= Id FROM @temp;
	insert into RolesMenu values (@roleId,@id1,1,'YinSheng', GETDATE());
	insert into RolesMenu values (@roleId,@id2,1,'YinSheng', GETDATE());
	insert into RolesMenu values (@roleId,@id3,1,'YinSheng', GETDATE());
	insert into RolesMenu values (@roleId,@id4,1,'YinSheng', GETDATE());
	SET ROWCOUNT 0

	delete from @temp where Id = @roleId
end