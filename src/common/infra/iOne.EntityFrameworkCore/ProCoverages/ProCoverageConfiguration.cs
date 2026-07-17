using System;
using iOne.ProCoverages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProCoverages;

public class ProCoverageConfiguration : IEntityTypeConfiguration<ProCoverage>
{
    public void Configure(EntityTypeBuilder<ProCoverage> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_coverage", t =>
        {
            t.HasComment("Bảng định nghĩa phạm vi bảo hiểm");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.LobId).HasColumnName("lob_id");
        builder.Property(x => x.ObjectTypeId).HasColumnName("object_type_id");
        builder.Property(x => x.CoverageGroupId).HasColumnName("coverage_group_id");
        builder.Property(x => x.CoverageTypeId).HasColumnName("coverage_type_id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.ShortName).HasColumnName("short_name");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Description).HasColumnName("description");
        
        // ✅ Type: Enum to string conversion
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>(
                v => v.ToString(), // Convert enum to string
                v => Enum.Parse<ProCoverageTermType>(v, true) // Parse string to enum
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại điều khoản: Main (Phạm vi chính), Addon (Phạm vi bổ sung), Exclusion (Phạm vi loại trừ), Benefit (Quyền lợi đi kèm)");
        
        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ProCoverageStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");
        
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

        // Foreign keys
        builder.HasOne(x => x.Lob)
            .WithMany()
            .HasForeignKey(x => x.LobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ObjectType)
            .WithMany()
            .HasForeignKey(x => x.ObjectTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(x => x.CoverageGroup)
            .WithMany()
            .HasForeignKey(x => x.CoverageGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CoverageType)
            .WithMany()
            .HasForeignKey(x => x.CoverageTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
