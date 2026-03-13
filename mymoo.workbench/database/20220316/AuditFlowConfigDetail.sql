USE [mymooo_workbench]
GO

/****** Object:  Table [dbo].[AuditFlowConfigDetail]    Script Date: 2022/3/16 21:53:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AuditFlowConfigDetail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AuditFlowConfigId] [bigint] NOT NULL,
	[Type] [int] NOT NULL,
	[SPType] [int] NULL,
	[UserCode] [nvarchar](50) NULL,
	[CreateUserCode] [nvarchar](50) NULL,
	[CreateDateTime] [datetime] NULL,
 CONSTRAINT [PK_CallbackUrlConfigDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AuditFlowConfigDetail]  WITH CHECK ADD  CONSTRAINT [FK_AuditFlowConfigDetail_AuditFlowConfigDetail] FOREIGN KEY([AuditFlowConfigId])
REFERENCES [dbo].[AuditFlowConfig] ([Id])
GO

ALTER TABLE [dbo].[AuditFlowConfigDetail] CHECK CONSTRAINT [FK_AuditFlowConfigDetail_AuditFlowConfigDetail]
GO


