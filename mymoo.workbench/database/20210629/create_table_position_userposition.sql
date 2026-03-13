USE [mymooo_workbench]

create table Position
(
	[Id] [bigint] IDENTITY(1,1)  primary key,
	[Code] [varchar](30) NOT NULL default '',
	[Name] [nvarchar](100) NOT NULL default '',
	[Description] [nvarchar](500) NULL default '',
	[IsForbidden] [bit] NOT NULL default 0,
	[CreateUser] [varchar](30) NOT NULL default '',
	[CreateDate] [datetime] NULL,
	[ForbiddenUser] [varchar](30) NULL default '',
	[ForbiddenDate] [datetime] NULL,
	[IsAssistant] [bit] NULL default 0
)
go

create table UserPosition
(
	Id					bigint	identity(1,1) primary key,
	UserCode			varchar(30)		not null	default '',
	PositionId    	    bigint			not null	default 0,
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)
go

create index Idx_UserPosition_UserCode on UserPosition(UserCode)
go
alter table UserPosition add constraint FK_UserPosition_PositionId foreign key (PositionId) references Position(Id)
go
