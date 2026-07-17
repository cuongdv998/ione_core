using System;
using iOne.HrEmployeePositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.HrEmployeePositions;

public class HrEmployeePositionConfiguration : IEntityTypeConfiguration<HrEmployeePosition>
{
    public void Configure(EntityTypeBuilder<HrEmployeePosition> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("hr_employee_position", t =>
        {
            t.HasComment("Bảng lưu chức danh công tác");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Type).HasColumnName("type");
        builder.Property(x => x.Status).HasColumnName("status");
        
        // ✅ Audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // Configure Type enum -> string conversion (lowercase)
        builder.Property(e => e.Type)
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<HrEmployeePositionType>(v, true)
            )
            .HasMaxLength(10)
            .HasComment("Phân loại vị trí/vai trò: - profess: chuyên môn - manage: quản lý");

        // Configure Status enum -> string conversion (lowercase)
        builder.Property(e => e.Status)
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<HrEmployeePositionStatus>(v, true)
            )
            .HasMaxLength(10)
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");
    }
}

