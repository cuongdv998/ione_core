using System;
using iOne.ResTaxes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResTaxes;

public class ResTaxConfiguration : IEntityTypeConfiguration<ResTax>
{
    public void Configure(EntityTypeBuilder<ResTax> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_tax", t =>
        {
            t.HasComment("Bảng định nghĩa bậc thuế");
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
            .HasMaxLength(500);
        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasColumnType("numeric(6,3)")
            .IsRequired()
            .HasComment("Giá trị thuế (theo phần trăm hoặc hệ số, tùy nghiệp vụ)");
        
        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResTaxStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");
        
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

        // ✅ One-to-many relationship with ResFeeItem
        // Note: The foreign key is configured in ResFeeItemConfiguration
    }
}

