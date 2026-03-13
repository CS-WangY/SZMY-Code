alter table Company add [RowVersion] RowVersion ;

create index Idx_SubAndParentCompany_CompanyId on SubAndParentCompany(CompanyId);
create index Idx_SubAndParentCompany_ParentCompanyId on SubAndParentCompany(ParentCompanyId);