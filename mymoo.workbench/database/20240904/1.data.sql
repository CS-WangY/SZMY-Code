

ALTER TABLE Menu DROP CONSTRAINT FK_Menu_AppId;

update Menu set AppId = 'srm'  where AppId = 'scm';
update ThirdpartyApplicationConfig set AppId = 'srm' where AppId = 'scm';
update Menu  set Name = 'srm'  where Name = 'scm';