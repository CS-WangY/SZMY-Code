if(COL_LENGTH('MymoooUser','UpdateTime') is null)
alter table MymoooUser
add UpdateTime DateTime default(null)