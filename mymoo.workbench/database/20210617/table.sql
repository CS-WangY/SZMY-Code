
use mymooo_workbench;


create table Department
(
	Id				bigint	identity(1,1) primary key,
	AppId			varchar(30)		not null	default '',
	DepartmentId	bigint	not null default 0,
	ParentId		bigint	not null default 0,
	[Name]			nvarchar(255) not null default '',
	name_en			varchar(255) not null default '',
	[order]			int	not null default 0,
	Versions		bigint	not null default 0,
	IsDelete		bit not null default 0
)

go

alter table Department add constraint FK_Department_AppId foreign key (AppId) references ThirdpartyApplicationConfig(AppId)
go

create Index Idx_Department_AppId on Department(AppId,DepartmentId)
go


create table MymoooUser
(
	Id				bigint	identity(1,1) primary key,
	AppId			varchar(30)		not null	default '',
	UserId			varchar(255) not null default '',
	[Name]			nvarchar(255) not null default '',
	Position			nvarchar(255) not null default '',
	Mobile				nvarchar(255) not null default '',
	Gender				nvarchar(255) not null default '',
	Email			nvarchar(255) not null default '',
	Telephone			nvarchar(255) not null default '',
	Alias			nvarchar(255) not null default '',
	[Address]			nvarchar(1000) not null default '',
	OpenUserid			varchar(255) not null default '',
	MainDepartmentId			bigint	not null default 0,
	[Status]			int not null default 0,
	QrCode			nvarchar(255) not null default '',
	ExternalPosition			nvarchar(255) not null default '',
	Versions		bigint	not null default 0,
	IsDelete		bit not null default 0
)

go

alter table MymoooUser add constraint FK_MymoooUser_AppId foreign key (AppId) references ThirdpartyApplicationConfig(AppId)
go

create Index Idx_MymoooUser_UserId on MymoooUser(UserId)
go


create table DepartmentUser
(
	Id				bigint	identity(1,1) primary key,
	DepartmentId	bigint	not null default 0,
	UserId	bigint	not null default 0,
	[order]			int	not null default 0,
	IsLeaderInDepartment			int	not null default 0,
)

go

alter table DepartmentUser add constraint FK_DepartmentUser_DepartmentId foreign key (DepartmentId) references Department(Id)
go


alter table DepartmentUser add constraint FK_DepartmentUser_UserId foreign key (UserId) references MymoooUser(Id)
go
