using System;
using iOne.ProProducts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProducts;

public class ProProductCoverageLevelConfiguration : IEntityTypeConfiguration<ProProductCoverageLevel>
{
    public void Configure(EntityTypeBuilder<ProProductCoverageLevel> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_coverage_level", t =>
        {
            t.HasComment("Định nghĩa các nhóm hạn mức của phạm vi");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductCoverageId)
            .HasColumnName("product_coverage_id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired()
            .HasComment("Ngày hiệu lực");

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date")
            .HasComment("Ngày hết hạn");

        builder.Property(x => x.ConditionalScript)
            .HasColumnName("conditional_script")
            .HasColumnType("TEXT")
            .HasComment("Script điều kiện");

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

        // Configure relationships
        // Many-to-one: ProProductCoverageLevel -> ProProductCoverage
        builder.HasOne(e => e.ProductCoverage)
            .WithMany(e => e.CoverageLevels)
            .HasForeignKey(e => e.ProductCoverageId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
