update AuditFlowConfig set EnvCode =  LEFT(AppId, len(AppId) - 3) where AppId like '%crm'; 
update AuditFlowConfig set EnvCode =  LEFT(AppId, len(AppId) - 3) where AppId like '%scm'; 
update AuditFlowConfig set EnvCode =  LEFT(AppId, len(AppId) - 13) where AppId like '%platformAdmin'; 
update AuditFlowConfig set EnvCode =  LEFT(AppId, len(AppId) - 9) where AppId like '%mymoooerp'; 
update AuditFlowConfig set EnvCode =  LEFT(AppId, len(AppId) - 6) where AppId like '%credit'; 
update AuditFlowConfig set EnvCode =  LEFT(AppId, len(AppId) - 7) where AppId like '%k3cloud'; 
update AuditFlowConfig set EnvCode =  'production' where EnvCode =  '';