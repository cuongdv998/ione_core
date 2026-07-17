using System;
using iOne.HrEmployees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.HrEmployees;

public class HrEmployeeConfiguration : IEntityTypeConfiguration<HrEmployee>
{
    public void Configure(EntityTypeBuilder<HrEmployee> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("hr_employee", t =>
        {
            t.HasComment("Bảng định nghĩa nhân viên");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        // Basic Info
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(50)
            .IsRequired();

        // Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<HrEmployeeStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        // Foreign Keys
        builder.Property(x => x.PositionId)
            .HasColumnName("position_id")
            .HasComment("Liên kết chức danh");

        builder.Property(x => x.LevelId)
            .HasColumnName("level_id")
            .HasComment("Liên kết cấp bậc");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id");

        builder.Property(x => x.OrgId)
            .HasColumnName("org_id")
            .HasComment("ID đơn vị");

        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();

        builder.Property(x => x.IsManager)
            .HasColumnName("is_manager")
            .HasComment("Đánh dấu có phải lãnh đạo đơn vị không:\n- true: có\n- false: không");

        builder.Property(x => x.ManagerId)
            .HasColumnName("manager_id")
            .HasComment("Người quản lý trực tiếp");

        builder.Property(x => x.ProvinceId)
            .HasColumnName("province_id");

        builder.Property(x => x.WardId)
            .HasColumnName("ward_id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasComment("ID liên kết với 1 user login");

        // Address Info
        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasMaxLength(250);

        builder.Property(x => x.FullAddress)
            .HasColumnName("full_address")
            .HasMaxLength(500);

        // Contact Info
        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(15);

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(50);

        // ✅ ExtraProperties: snake_case
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        // ✅ Audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(e => e.DepartmentId, "ix_hr_employee_department_id");

        builder.HasIndex(e => e.OrgId, "ix_hr_employee_org_id");

        builder.HasIndex(e => e.ManagerId, "ix_hr_employee_manager_id");

        // Foreign Key Relationships
        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Level)
            .WithMany()
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Org)
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Manager)
            .WithMany(x => x.ManagedEmployees)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Ward)
            .WithMany()
            .HasForeignKey(x => x.WardId)
            .OnDelete(DeleteBehavior.Restrict);

        // User relationship (IdentityUser)
        builder.HasOne<Volo.Abp.Identity.IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

