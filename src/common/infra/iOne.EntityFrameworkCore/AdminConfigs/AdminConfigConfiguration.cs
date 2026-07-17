using System;
using iOne.AdminConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.AdminConfigs;

public class AdminConfigConfiguration : IEntityTypeConfiguration<AdminConfig>
{
    public void Configure(EntityTypeBuilder<AdminConfig> builder)
    {
        builder.ToTable("admin_config", t =>
        {
            t.HasComment("Bảng lưu trữ các cấu hình chung của hệ thống");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.SubCode)
            .HasColumnName("sub_code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)");

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Giá trị cấu hình");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(250);

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<AdminConfigStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive (case-insensitive)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // ⚠️ QUAN TRỌNG: Composite unique index trên (code, sub_code)
        builder.HasIndex(e => new { e.Code, e.SubCode }, "ix_admin_config_code_sub_code")
            .IsUnique();
    }
}

