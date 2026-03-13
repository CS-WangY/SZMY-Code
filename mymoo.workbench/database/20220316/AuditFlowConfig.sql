USE [mymooo_workbench]
GO

/****** Object:  Table [dbo].[AuditFlowConfig]    Script Date: 2022/3/16 21:53:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AuditFlowConfig](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AppId] [nvarchar](50) NOT NULL,
	[TemplateId] [nvarchar](100) NOT NULL,
	[ApprovalMode] [int] NOT NULL,
	[NotifyType] [int] NOT NULL,
	[CreateTime] [datetime] NULL,
	[CreateUserCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_AuditFlowConfig] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


