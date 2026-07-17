using System;
using iOne.HrDepartments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.HrDepartments;

public class HrDepartmentConfiguration : IEntityTypeConfiguration<HrDepartment>
{
    public void Configure(EntityTypeBuilder<HrDepartment> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("hr_department", t =>
        {
            t.HasComment("Phòng ban/Đơn vị");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        // Basic Info
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        // Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<HrDepartmentStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        // DeptLevel: Enum to string conversion (lowercase)
        builder.Property(x => x.DeptLevel)
            .HasColumnName("dept_level")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Unit" → "unit", "Dept" → "dept"
                v => Enum.Parse<HrDepartmentLevel>(v, true) // Parse "unit" → Unit, "dept" → Dept
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Đơn vị hay phòng/ban:\n- unit: đơn vị\n- dept: phòng/ban");

        // Self-Referencing
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id");

        builder.Property(x => x.OrgId)
            .HasColumnName("org_id");

        // Foreign Keys
        builder.Property(x => x.TypeId)
            .HasColumnName("type_id");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id")
            .IsRequired();

        builder.Property(x => x.ProvinceId)
            .HasColumnName("province_id")
            .HasComment("Tỉnh đăng ký kinh doanh");

        builder.Property(x => x.WardId)
            .HasColumnName("ward_id")
            .HasComment("Phường/Xã đăng ký kinh doanh");

        builder.Property(x => x.BankId)
            .HasColumnName("bank_id")
            .HasComment("Ngân hàng");

        // Address Info
        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasMaxLength(500)
            .HasComment("Địa chỉ văn phòng đăng ký kinh doanh");

        builder.Property(x => x.FullAddress)
            .HasColumnName("full_address")
            .HasMaxLength(500);

        // Bank Info
        builder.Property(x => x.BankNo)
            .HasColumnName("bank_no")
            .HasMaxLength(50)
            .HasComment("Số tài khoản ngân hàng");

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

        // ✅ Indexes: Foreign keys
        builder.HasIndex(e => e.ParentId, "ix_hr_department_parent_id");
        builder.HasIndex(e => e.OrgId, "ix_hr_department_org_id");
        builder.HasIndex(e => e.TypeId, "ix_hr_department_type_id");
        builder.HasIndex(e => e.PartnerId, "ix_hr_department_partner_id");
        builder.HasIndex(e => e.ProvinceId, "ix_hr_department_province_id");
        builder.HasIndex(e => e.WardId, "ix_hr_department_ward_id");
        builder.HasIndex(e => e.BankId, "ix_hr_department_bank_id");

        // ✅ Foreign Key Relationships
        // Self-referencing: Parent
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hr_department_parent_id");

        // Self-referencing: Org
        builder.HasOne(x => x.Org)
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hr_department_org_id");

        // Type
        builder.HasOne(x => x.Type)
            .WithMany()
            .HasForeignKey(x => x.TypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hr_department_type_id");

        // Partner
        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("fk_hr_department_partner_id");

        // Province
        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hr_department_province_id");

        // Ward
        builder.HasOne(x => x.Ward)
            .WithMany()
            .HasForeignKey(x => x.WardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hr_department_ward_id");

        // Bank
        builder.HasOne(x => x.Bank)
            .WithMany()
            .HasForeignKey(x => x.BankId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hr_department_bank_id");
    }
}

