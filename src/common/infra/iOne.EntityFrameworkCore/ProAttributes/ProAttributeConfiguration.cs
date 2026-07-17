using System;
using iOne.ProAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProAttributes;

public class ProAttributeConfiguration : IEntityTypeConfiguration<ProAttribute>
{
    public void Configure(EntityTypeBuilder<ProAttribute> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_attribute", t =>
        {
            t.HasComment("Bảng cấu hình các thuộc tính hệ thống");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(250);
        builder.Property(x => x.DataPath)
            .HasColumnName("data_path")
            .HasMaxLength(50);
        builder.Property(x => x.ComputeScript)
            .HasColumnName("compute_script")
            .HasColumnType("TEXT");
        builder.Property(x => x.ClearDataScript)
            .HasColumnName("clear_data_script")
            .HasMaxLength(1000);
        
        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ProAttributeStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");
        
        // ✅ Spec: Enum to string conversion
        builder.Property(x => x.Spec)
            .HasColumnName("spec")
            .HasConversion<string>(
                v => v.ToString(), // Convert enum to string
                v => Enum.Parse<ProAttributeSpec>(v, true) // Parse string to enum
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Phạm vi tham số: RiskObject, Customer, Policy, Coverage");
        
        // ✅ DataType: Enum to string conversion
        builder.Property(x => x.DataType)
            .HasColumnName("data_type")
            .HasConversion<string>(
                v => v.ToString(), // Convert enum to string
                v => Enum.Parse<ProAttributeDataType>(v, true) // Parse string to enum
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Loại dữ liệu: String, Int, Float, Date, Boolean");
        
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

    }
}
