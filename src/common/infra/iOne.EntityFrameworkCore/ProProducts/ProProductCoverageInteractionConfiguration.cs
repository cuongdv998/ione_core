using System;
using iOne.ProProducts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProducts;

public class ProProductCoverageInteractionConfiguration : IEntityTypeConfiguration<ProProductCoverageInteraction>
{
    public void Configure(EntityTypeBuilder<ProProductCoverageInteraction> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_coverage_interaction", t =>
        {
            t.HasComment("Bảng định nghĩa ràng buộc giữa các phạm vi bảo hiểm");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductCoverageId)
            .HasColumnName("product_coverage_id")
            .IsRequired();

        // ✅ InteractionType: Enum to string conversion (lowercase)
        builder.Property(x => x.InteractionType)
            .HasColumnName("interaction_type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Dependency" → "dependency", "Incompatible" → "incompatible", etc.
                v => Enum.Parse<ProProductCoverageInteractionType>(v, true) // Parse "dependency" → Dependency, "incompatible" → Incompatible, etc.
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại ràng buộc: dependency (phụ thuộc), incompatible (không tương thích), exclusive (loại trừ nhau)");

        builder.Property(x => x.InteractionCoverageId)
            .HasColumnName("interaction_coverage_id")
            .IsRequired()
            .HasComment("Phạm vi có ràng buộc");

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired();

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date");

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
        // Many-to-one: ProProductCoverageInteraction -> ProProductCoverage (ProductCoverage)
        builder.HasOne(e => e.ProductCoverage)
            .WithMany(e => e.Interactions)
            .HasForeignKey(e => e.ProductCoverageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProductCoverageInteraction -> ProProductCoverage (InteractionCoverage)
        builder.HasOne(e => e.InteractionCoverage)
            .WithMany()
            .HasForeignKey(e => e.InteractionCoverageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
