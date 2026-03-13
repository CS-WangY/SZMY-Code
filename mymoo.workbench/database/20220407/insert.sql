
USE [mymooo_workbench]
GO
INSERT [dbo].[ApprovalTemplate] ([TemplateId], [AppId], [TemplateName], [MessageExecute], [CallbackUrl], [CreateUser], [CreateDate]) VALUES (N'C4NyG6i5CDHNZW6MszDJB14RFJYrbaw1SWsRjd7zp', N'crm', N'集团客户申请模板', N'com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.BindCompanyCallBack,com.mymooo.workbench.weixin.work', N'Company/SetSubCompany', N'WangZhuo', CAST(N'2022-03-25T09:44:02.437' AS DateTime))
GO


USE [mymooo_workbench]
GO
INSERT [dbo].[ApprovalTemplateField] ( [TemplateId], [FieldNumber], [FieldName], [FieldId], [FieldType], [KeywordSeq], [CreateUser], [CreateDate], [SelectOptionJson]) VALUES ( N'C4NyG6i5CDHNZW6MszDJB14RFJYrbaw1SWsRjd7zp', N'HeadCustCode', N'集团客户编码', N'Text-1647834469861', N'Text', 0, N'', CAST(N'2022-03-25T09:53:55.917' AS DateTime), N'')
GO
INSERT [dbo].[ApprovalTemplateField] ( [TemplateId], [FieldNumber], [FieldName], [FieldId], [FieldType], [KeywordSeq], [CreateUser], [CreateDate], [SelectOptionJson]) VALUES ( N'C4NyG6i5CDHNZW6MszDJB14RFJYrbaw1SWsRjd7zp', N'HeadCustName', N'集团客户名称', N'Text-1647834479986', N'Text', 1, N'', CAST(N'2022-03-25T09:55:20.740' AS DateTime), N'')
GO
INSERT [dbo].[ApprovalTemplateField] ( [TemplateId], [FieldNumber], [FieldName], [FieldId], [FieldType], [KeywordSeq], [CreateUser], [CreateDate], [SelectOptionJson]) VALUES ( N'C4NyG6i5CDHNZW6MszDJB14RFJYrbaw1SWsRjd7zp', N'SubmgCustCode', N'子公司编码', N'Text-1647834489462', N'Text', 2, N'', CAST(N'2022-03-25T09:56:43.497' AS DateTime), N'')
GO
INSERT [dbo].[ApprovalTemplateField] ( [TemplateId], [FieldNumber], [FieldName], [FieldId], [FieldType], [KeywordSeq], [CreateUser], [CreateDate], [SelectOptionJson]) VALUES ( N'C4NyG6i5CDHNZW6MszDJB14RFJYrbaw1SWsRjd7zp', N'SubmgCustName', N'子公司名称', N'Text-1647834499884', N'Text', 3, N'', CAST(N'2022-03-25T09:58:39.157' AS DateTime), N'')
GO
INSERT [dbo].[ApprovalTemplateField] ( [TemplateId], [FieldNumber], [FieldName], [FieldId], [FieldType], [KeywordSeq], [CreateUser], [CreateDate], [SelectOptionJson]) VALUES ( N'C4NyG6i5CDHNZW6MszDJB14RFJYrbaw1SWsRjd7zp', N'SubmgRemarks', N'申请加入集团公司理由', N'Textarea-1647834528314', N'Textarea', 4, N'', CAST(N'2022-03-25T09:59:28.260' AS DateTime), N'')
GO
INSERT [dbo].[ApprovalTemplateField] ( [TemplateId], [FieldNumber], [FieldName], [FieldId], [FieldType], [KeywordSeq], [CreateUser], [CreateDate], [SelectOptionJson]) VALUES ( N'C4NyG6i5CDHNZW6MszDJB14RFJYrbaw1SWsRjd7zp', N'SubmgFiles', N'附件', N'File-1647834548531', N'File', 5, N'', CAST(N'2022-03-25T10:01:52.257' AS DateTime), N'')
GO



