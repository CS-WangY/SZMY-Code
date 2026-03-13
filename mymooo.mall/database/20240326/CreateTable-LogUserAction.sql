if not exists (select * from sys.all_columns where  [object_id]=object_id(N'[LogUserAction]'))
BEGIN
CREATE TABLE [dbo].[LogUserAction](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[UserName] [nvarchar](50) NULL,
	[Ip] [nvarchar](50) NULL,
	[HostName] [nvarchar](50) NULL,
	[ActionPath] [nvarchar](50) NULL,
	[MainParam] [nvarchar](500) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[NTime] [float] NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LogUserAction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[LogUserAction] ADD  CONSTRAINT [DF_LogUserAction_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]

END