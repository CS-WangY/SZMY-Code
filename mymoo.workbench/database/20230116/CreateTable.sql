if exists (select * from sysobjects where id = object_id(N'QueryMobileHistory') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table QueryMobileHistory
Create Table QueryMobileHistory
(
 Id bigint Identity(1,1) primary key,
 Name nvarchar(20),
 Mobile nvarchar(20),
 CreateTime DateTime,
 QueryByName nvarchar(20)
)