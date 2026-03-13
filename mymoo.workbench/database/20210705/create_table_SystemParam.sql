USE mymooo_workbench
GO

/****** Object:  Table [dbo].[SystemParam]    Script Date: 2021/7/5 9:12:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SystemParam](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[GroupId] [bigint] NOT NULL,
	[SystemParamKey] [varchar](50) NOT NULL,
	[SystemParamValue] [nvarchar](200) NOT NULL,
	[SystemParamDesc] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_SystemParam] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统参数分类Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SystemParam', @level2type=N'COLUMN',@level2name=N'GroupId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统参数key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SystemParam', @level2type=N'COLUMN',@level2name=N'SystemParamKey'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统参数值' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SystemParam', @level2type=N'COLUMN',@level2name=N'SystemParamValue'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统参数描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SystemParam', @level2type=N'COLUMN',@level2name=N'SystemParamDesc'
GO

insert into [SystemParam] values (
  0,'System_FunctionAttr_Business','业务','业务'
),(
  0,'System_FunctionAttr_Manage','管理','管理'
),(
  0,'System_FunctionAttr_Purchase','采购','采购'
),(
  0,'System_FunctionAttr_R&D','研发','研发'
),(
  0,'System_FunctionAttr_Finance','财务','财务'
),(
  0,'System_FunctionAttr_Other','其他','其他'
)
