using System;
using iOne.HrEmployees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.HrEmployees;

public class HrEmployeeRoleRelConfiguration : IEntityTypeConfiguration<HrEmployeeRoleRel>
{
    public void Configure(EntityTypeBuilder<HrEmployeeRoleRel> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("hr_employee_role_rel", t =>
        {
            t.HasComment("Bảng lưu các vai trò của nhân viên, một nhân viên có thể có nhiều hơn 1 vai trò");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired();

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date");

        // ✅ Audit columns: snake_case (KHÔNG có soft delete)
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");

        // ✅ Indexes
        builder.HasIndex(e => e.EmployeeId, "ix_hr_employee_role_rel_employee_id");

        builder.HasIndex(e => e.RoleId, "ix_hr_employee_role_rel_role_id");

        builder.HasIndex(e => new { e.EmployeeId, e.RoleId }, "ix_hr_employee_role_rel_employee_role");

        // Foreign Key Relationships
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

