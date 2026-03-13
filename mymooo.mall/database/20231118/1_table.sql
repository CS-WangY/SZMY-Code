
--CREATE TABLE [RabbitMQMessage](
--	[MessageId] [bigint] IDENTITY(1,1) NOT NULL,
--	[Exchange] [nvarchar](100) NOT NULL DEFAULT (''),
--	[Routingkey] [nvarchar](100) NOT NULL DEFAULT (''),
--	[Headers] [ntext] NULL,
--	[Message] [ntext] NULL,
--	[Keyword] [nvarchar](100) NOT NULL DEFAULT (''),
--	[CreateUserId] [bigint] NOT NULL DEFAULT (0),
--	[CreateUserCode] [nvarchar](100) NOT NULL DEFAULT (''),
--	[CreateUserName] [nvarchar](100) NOT NULL DEFAULT (''),
--	[CreateDate] [datetime] NULL,
--	[IsComplete] [bit] NOT NULL  DEFAULT (0),
--	[CompleteDate] [datetime] NULL,
-- CONSTRAINT [PK_RabbitMQMessage] PRIMARY KEY CLUSTERED 
--(
--	[MessageId] ASC
--)
--) ;

--CREATE NONCLUSTERED INDEX [IX_RabbitMQMessage_Exchange] ON [dbo].[RabbitMQMessage]
--(
--	[Exchange] ASC,
--	[Routingkey] ASC
--);

--CREATE NONCLUSTERED INDEX [IX_RabbitMQMessage_Keyword] ON [dbo].[RabbitMQMessage]
--(
--	[Keyword] ASC
--)
--;


--CREATE TABLE [dbo].[ScheduledTask](
--	[ScheduledId] [bigint] IDENTITY(1,1) NOT NULL,
--	[ScheduledCode] [varchar](100) NOT NULL  DEFAULT (''),
--	[ScheduledName] [nvarchar](100) NOT NULL  DEFAULT (''),
--	[Exchange] [varchar](100) NOT NULL  DEFAULT (''),
--	[Routingkey] [varchar](100) NOT NULL  DEFAULT (''),
--	[ExecuteCalss] [varchar](300) NOT NULL  DEFAULT (''),
--	[ExecuteAction] [varchar](300) NOT NULL  DEFAULT (''),
--	[CreateUserId] [bigint] NOT NULL,
--	[CreateUserCode] [varchar](100) NOT NULL  DEFAULT (''),
--	[CreateUserName] [nvarchar](100) NOT NULL  DEFAULT (''),
--	[CreateUserDate] [datetime] NULL,
-- CONSTRAINT [PK_ScheduledTask] PRIMARY KEY CLUSTERED 
--(
--	[ScheduledId] ASC
--)
--);

--alter table F_USER_MSTR add [RowVersion] RowVersion ;
--alter table F_SYSPROFILE add [RowVersion] RowVersion ;
--alter table F_PRD_MSTR add [RowVersion] RowVersion ;
--alter table ProductSmallClass add [RowVersion] RowVersion ;


