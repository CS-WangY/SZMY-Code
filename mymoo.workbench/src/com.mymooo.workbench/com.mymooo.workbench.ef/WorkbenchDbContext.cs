using com.mymooo.workbench.ef.AccountContext;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using Microsoft.EntityFrameworkCore;
using mymooo.weixinWork.SDK.Approval.Model.Enum;
using System;

namespace com.mymooo.workbench.ef
{
	public class WorkbenchDbContext : DbContext
    {
        public DbSet<UserToken> UserToken { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<RolesMenu> RolesMenu { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<DepartmentUser> DepartmentUser { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<MenuItem> MenuItem { get; set; }
        public DbSet<RolesMenuItem> RolesMenuItem { get; set; }
        public DbSet<ThirdpartyApplicationConfig> ThirdpartyApplicationConfig { get; set; }
        public DbSet<ThirdpartyApplicationDetail> ThirdpartyApplicationDetail { get; set; }
        public virtual DbSet<WeiXinMessage> WeiXinMessage { get; set; }
        public virtual DbSet<ApprovalMessage> ApprovalMessage { get; set; }
        public virtual DbSet<ApprovalTemplate> ApprovalTemplate { get; set; }
        public virtual DbSet<ApprovalTemplateField> ApprovalTemplateField { get; set; }
        public virtual DbSet<Position> Position { get; set; }
        public virtual DbSet<UserPosition> UserPosition { get; set; }
        public virtual DbSet<AssistantUser> AssistantUsers { get; set; }
        public virtual DbSet<SystemParam> SystemParam { get; set; }

        public virtual DbSet<AuditFlowConfig> AuditFlowConfigs { get; set; }
        public virtual DbSet<AuditFlowConfigDetail> AuditFlowConfigDetails { get; set; }
        public virtual DbSet<CallbackUrlConfig> CallbackUrlConfigs { get; set; }

        public virtual DbSet<QueryMobileHistory> QueryMobileHistories { get; set; }

        public WorkbenchDbContext(DbContextOptions<WorkbenchDbContext> options)
        : base(options)
        {
            //Scaffold-DbContext "Data Source=192.168.11.100,11433;Initial Catalog=mymooo_workbench;User ID=sa;Password=Abc.123456;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Tables MenuFunction,RolesMenuFunction  -Force
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WeiXinMessage>(entity =>
            {
                entity.Property(u => u.Result).HasDefaultValue("");
                entity.Property(u => u.Spno).HasDefaultValue("");
            });
            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.Property(u => u.MymoooCompany).HasDefaultValue("");
            });
            modelBuilder.Entity<ApprovalMessage>(entity =>
            {
                entity.Property(u => u.ApprovalUser).HasDefaultValue("");
            });
            modelBuilder.Entity<ApprovalTemplateField>().Property(e => e.FieldType).HasConversion(v => v.ToString(), v => (ApproverFieldType)Enum.Parse(typeof(ApproverFieldType), v));

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department");

                entity.HasIndex(e => new { e.AppId, e.DepartmentId }, "Idx_Department_AppId");

                entity.Property(e => e.AppId)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FunctionAttr)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.NameEn)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name_en")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.HasOne(d => d.App)
                    .WithMany(p => p.Departments)
                    .HasForeignKey(d => d.AppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Department_AppId");
            });

            modelBuilder.Entity<DepartmentUser>(entity =>
            {
                entity.ToTable("DepartmentUser");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.DepartmentUsers)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DepartmentUser_DepartmentId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DepartmentUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DepartmentUser_UserId");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("MymoooUser");

                entity.HasIndex(e => e.UserId, "Idx_MymoooUser_UserId");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Alias)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.AppId)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ExternalPosition)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Gender)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.OpenUserid)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Position)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.QrCode)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Telephone)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.App)
                    .WithMany(p => p.MymoooUsers)
                    .HasForeignKey(d => d.AppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MymoooUser_AppId");
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.ToTable("Position");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ForbiddenDate).HasColumnType("datetime");

                entity.Property(e => e.ForbiddenUser)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.IsAssistant).HasDefaultValueSql("((0))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<UserPosition>(entity =>
            {
                entity.ToTable("UserPosition");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.UserPositions)
                    .HasForeignKey(d => d.PositionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserPosition_PositionId");
            });

            modelBuilder.Entity<AssistantUser>(entity =>
            {
                entity.ToTable("AssistantUser");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<SystemParam>(entity =>
            {
                entity.ToTable("SystemParam");

                entity.Property(e => e.GroupId).HasComment("系统参数分类Id");

                entity.Property(e => e.SystemParamDesc)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasComment("系统参数描述");

                entity.Property(e => e.SystemParamKey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("系统参数key");

                entity.Property(e => e.SystemParamValue)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasComment("系统参数值");
            });

            modelBuilder.Entity<AuditFlowConfig>(entity =>
            {
                entity.ToTable("AuditFlowConfig");

                entity.Property(e => e.AppId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.CreateUserCode).HasMaxLength(50);

                entity.Property(e => e.TemplateId)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<AuditFlowConfigDetail>(entity =>
            {
                entity.ToTable("AuditFlowConfigDetail");

                entity.Property(e => e.CreateDateTime).HasColumnType("datetime");

                entity.Property(e => e.CreateUserCode).HasMaxLength(50);

                entity.Property(e => e.Sptype).HasColumnName("SPType").HasConversion(v => v.ToString(), v => (ApproverNodeModel)Enum.Parse(typeof(ApproverNodeModel), v));

                entity.Property(e => e.Type).HasConversion(v => v.ToString(), v => (AuditFlowDetailType)Enum.Parse(typeof(AuditFlowDetailType), v));

                entity.Property(e => e.UserCode).HasMaxLength(50);      

                entity.HasOne(d => d.AuditFlowConfig)
                    .WithMany(p => p.AuditFlowConfigDetails)
                    .HasForeignKey(d => d.AuditFlowConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AuditFlowConfigDetail_AuditFlowConfigDetail");
            });

            modelBuilder.Entity<CallbackUrlConfig>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.ToTable("CallbackUrlConfig");

                entity.Property(e => e.AppId)
                    .HasMaxLength(50);

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.CreateUserCode).HasMaxLength(50);

                entity.Property(e => e.Spno)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("SPNO");

                entity.Property(e => e.TemplateId)
                    .IsRequired()
                    .HasMaxLength(100);
            });
            
            modelBuilder.Entity<QueryMobileHistory>(entity => {
                entity.ToTable("QueryMobileHistory");

                entity.Property(e => e.QueryByName).IsRequired();
                entity.Property(e => e.CreateTime).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Mobile).IsRequired();
            });
        }
    }
}
