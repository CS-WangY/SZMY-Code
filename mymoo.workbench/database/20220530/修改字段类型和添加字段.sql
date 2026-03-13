alter table AuditFlowConfigDetail add ConditionName ntext
alter table AuditFlowConfigDetail add Formal ntext
alter table AuditFlowConfigDetail add ParentId int not null default 0
alter table AuditFlowConfigDetail add SonId int  not null default 0
alter table AuditFlowConfigDetail add Seq int not null default 0
alter table AuditFlowConfigDetail alter column Type nvarchar(50) not null
alter table AuditFlowConfigDetail alter column SPType nvarchar(50)