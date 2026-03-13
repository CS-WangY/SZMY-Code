
  alter table UserRoles add UserId bigint not null default 0

    alter table UserToken add UserId bigint not null default 0

	
    alter table UserPosition add UserId bigint not null default 0

    alter table AssistantUser add AssistantId bigint not null default 0
    alter table AssistantUser add UserId bigint not null default 0

	go
	--给UserRoles分配UserId,多个角色的取深圳蚂蚁用户id,MymoooUser中user不存在的,UserId默认为0
	declare @temp table (
		Id bigint,
		UserId varchar(50)
	);
	insert into @temp(Id, UserId) select Id,UserId from (select t.* from
		(select *,row_number() over (partition by UserId order by AppId desc) rn 
		from MymoooUser) t
	where rn=1) p

	declare @id bigint 
	declare @userId varchar(50) 

	while exists(select Id from @temp)
	begin 
		--set rowcount 1

		select top 1 @id = Id, @userId = UserId from @temp
		declare @count int 
		select @count = count(*) from UserRoles where UserCode = @userId
		if @count > 0
		begin
			update UserRoles set UserId = @id  where UserCode = @userId
		end
		
		set rowcount 0
		delete from @temp where Id = @id
	end
	go
	--给UserPosition分配UserId,多个角色的取深圳蚂蚁用户id,MymoooUser中user不存在的,UserId默认为0
	declare @temp table (
		Id bigint,
		UserId varchar(50)
	);
	insert into @temp(Id, UserId) select Id,UserId from (select t.* from
		(select *,row_number() over (partition by UserId order by AppId desc) rn 
		from MymoooUser) t
	where rn=1) p

	declare @id bigint 
	declare @userId varchar(50) 

	while exists(select Id from @temp)
	begin 
		--set rowcount 1

		select top 1 @id = Id, @userId = UserId from @temp
		declare @count int 
		select @count = count(*) from UserPosition where UserCode = @userId
		if @count > 0
		begin
			update UserPosition set UserId = @id  where UserCode = @userId
		end
		
		set rowcount 0
		delete from @temp where Id = @id
	end
	go
	--给UserToken分配UserId,多个角色的取深圳蚂蚁用户id,MymoooUser中user不存在的,UserId默认为0
	declare @temp table (
		Id bigint,
		UserId varchar(50)
	);
	insert into @temp(Id, UserId) select Id,UserId from (select t.* from
		(select *,row_number() over (partition by UserId order by AppId desc) rn 
		from MymoooUser) t
	where rn=1) p

	declare @id bigint 
	declare @userId varchar(50) 

	while exists(select Id from @temp)
	begin 
		--set rowcount 1

		select top 1 @id = Id, @userId = UserId from @temp
		declare @count int 
		select @count = count(*) from UserToken where UserCode = @userId
		if @count > 0
		begin
			update UserToken set UserId = @id  where UserCode = @userId
		end
		
		set rowcount 0
		delete from @temp where Id = @id
	end
	go
	--给AssistantUser分配UserId,AssistantId,多个角色的取深圳蚂蚁用户id,MymoooUser中user不存在的,UserId, AssistantId默认为0
	declare @temp table (
		Id bigint,
		UserId varchar(50)
	);
	insert into @temp(Id, UserId) select Id,UserId from (select t.* from
		(select *,row_number() over (partition by UserId order by AppId desc) rn 
		from MymoooUser) t
	where rn=1) p

	declare @id bigint 
	declare @userId varchar(50) 

	while exists(select Id from @temp)
	begin 
		--set rowcount 1

		select top 1 @id = Id, @userId = UserId from @temp
		declare @count int 
		select @count = count(*) from AssistantUser where UserCode = @userId
		if @count > 0
		begin
			update AssistantUser set UserId = @id  where UserCode = @userId
		end
		declare @count1 int 
		select @count1 = count(*) from AssistantUser where AssistantCode = @userId
		if @count1 > 0
		begin
			update AssistantUser set AssistantId = @id  where AssistantCode = @userId
		end
		set rowcount 0
		delete from @temp where Id = @id
	end