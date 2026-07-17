using System;
using iOne.ResBusinessAssignees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResBusinessAssignees;

public class ResBusinessAssigneeConfiguration : IEntityTypeConfiguration<ResBusinessAssignee>
{
    public void Configure(EntityTypeBuilder<ResBusinessAssignee> builder)
    {
        builder.ToTable("res_business_assignee", t =>
        {
            t.HasComment("Bảng định nghĩa đối tượng sẽ thực hiện theo thẩm quyền");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id");
        builder.Property(x => x.BusinessCode)
            .HasColumnName("business_code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Mã nghiệp vụ liên quan, từ admin_config code = BUSINESS_CODE");
        builder.Property(x => x.AuthorityCode)
            .HasColumnName("authority_code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Mã thẩm quyền thực hiện");
        builder.Property(x => x.AssigneeType)
            .HasColumnName("assignee_type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResBusinessAssigneeType>(v, true))
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("emp - đích danh; role - theo vai trò; system - hệ thống");
        builder.Property(x => x.EffectDate).HasColumnName("effect_date").IsRequired();
        builder.Property(x => x.ExpireDate).HasColumnName("expire_date");
        builder.Property(x => x.AssigneeRole)
            .HasColumnName("assignee_role")
            .HasMaxLength(50)
            .HasComment("Mã vai trò, bắt buộc nếu assigneeType = role");
        builder.Property(x => x.AssigneeId)
            .HasColumnName("assignee_id")
            .HasComment("ID nhân viên, bắt buộc nếu assigneeType = emp");
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .HasComment("Đơn vị thực hiện");
        builder.Property(x => x.DepartmentLevel)
            .HasColumnName("department_level")
            .HasConversion<string>(
                v => v == null ? null : v.Value.ToString().ToLowerInvariant(),
                v => string.IsNullOrEmpty(v) ? (ResBusinessAssigneeDepartmentLevel?)null : Enum.Parse<ResBusinessAssigneeDepartmentLevel>(v, true))
            .HasMaxLength(15)
            .HasComment("in - trong phân cấp; out - trên phân cấp");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResBusinessAssigneeStatus>(v, true))
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("active / deactive");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(x => x.BusinessCode, "ix_res_business_assignee_business_code");
        builder.HasIndex(x => x.AuthorityCode, "ix_res_business_assignee_authority_code");
        builder.HasIndex(x => x.DepartmentId, "ix_res_business_assignee_department_id");
        builder.HasIndex(x => x.AssigneeId, "ix_res_business_assignee_assignee_id");
        builder.HasIndex(x => x.EffectDate, "ix_res_business_assignee_effect_date");
    }
}
