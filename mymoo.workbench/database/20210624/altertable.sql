--修改MenuItem字段长度
DROP INDEX Idx_MenuItem_Path ON MenuItem
ALTER TABLE MenuItem DROP CONSTRAINT DF__MenuItem__Descri__693CA210

ALTER TABLE MenuItem ALTER COLUMN Path VARCHAR(255)
ALTER TABLE MenuItem ALTER COLUMN Title NVARCHAR(255)
ALTER TABLE MenuItem ALTER COLUMN  Description VARCHAR(255)

ALTER TABLE [dbo].[MenuItem] ADD  DEFAULT ('') FOR [Description]
CREATE NONCLUSTERED INDEX [Idx_MenuItem_Path] ON [dbo].[MenuItem]
(
	[Path] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

--修改Roles表Description、ForbiddenUser字段允许为空
alter table Roles alter column Description nvarchar(500) null;
alter table Roles alter column ForbiddenUser varchar(30) null;