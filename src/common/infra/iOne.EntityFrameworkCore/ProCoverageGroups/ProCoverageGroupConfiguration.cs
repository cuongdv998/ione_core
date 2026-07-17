using System;
using iOne.ProCoverageGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProCoverageGroups;

public class ProCoverageGroupConfiguration : IEntityTypeConfiguration<ProCoverageGroup>
{
    public void Configure(EntityTypeBuilder<ProCoverageGroup> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_coverage_group", t =>
        {
            t.HasComment("Bảng định nghĩa nhóm phạm vi");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.Description).HasColumnName("description");
        
        // ✅ Audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // Configure Status enum -> string conversion (lowercase)
        builder.Property(e => e.Status)
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ProCoverageGroupStatus>(v, true)
            )
            .HasMaxLength(15)
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");

        // Configure Description
        builder.Property(e => e.Description)
            .HasMaxLength(500);
    }
}

