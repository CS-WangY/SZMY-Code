
alter table AuditFlowConfig add EnvCode varchar(100) not null default '';


ALTER TABLE AuditFlowConfig ADD CONSTRAINT DF_AuditFlowConfig_AppId DEFAULT '' FOR AppId;