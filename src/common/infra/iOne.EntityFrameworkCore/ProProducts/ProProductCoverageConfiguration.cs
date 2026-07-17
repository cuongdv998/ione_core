using System;
using iOne.ProProducts;
using iOne.ProCoverages;
using iOne.ResUoms;
using iOne.ResTaxes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProducts;

public class ProProductCoverageConfiguration : IEntityTypeConfiguration<ProProductCoverage>
{
    public void Configure(EntityTypeBuilder<ProProductCoverage> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_coverage", t =>
        {
            t.HasComment("Bảng định nghĩa các phạm vi bảo hiểm của sản phẩm");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.CoverageId)
            .HasColumnName("coverage_id")
            .IsRequired();

        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id");

        builder.Property(x => x.InsurerCoverageCode)
            .HasColumnName("insurer_coverage_code")
            .HasMaxLength(50);

        builder.Property(x => x.UomId)
            .HasColumnName("uom_id");

        // ✅ AvailabilityType: Enum to string conversion (lowercase)
        builder.Property(x => x.AvailabilityType)
            .HasColumnName("availability_type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Required" → "required", "Standard" → "standard", etc.
                v => Enum.Parse<ProProductCoverageAvailabilityType>(v, true) // Parse "required" → Required, "standard" → Standard, etc.
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại khả dụng: required (bắt buộc), standard (chuẩn), optional (tùy chọn), selectable (có thể chọn)");

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired();

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date");

        builder.Property(x => x.SeqNumber)
            .HasColumnName("seq_number")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.TaxId)
            .HasColumnName("tax_id")
            .IsRequired();

        builder.Property(x => x.EnableQuantity)
            .HasColumnName("enable_quantity")
            .HasMaxLength(1);

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
        // Many-to-one: ProProductCoverage -> ProProduct
        builder.HasOne(e => e.Product)
            .WithMany(e => e.ProductCoverages)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProductCoverage -> ProCoverage
        builder.HasOne(e => e.Coverage)
            .WithMany(e => e.ProductCoverages)
            .HasForeignKey(e => e.CoverageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-joining: ProProductCoverage -> ProProductCoverage (Parent)
        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Many-to-one: ProProductCoverage -> ResUom
        builder.HasOne(e => e.Uom)
            .WithMany()
            .HasForeignKey(e => e.UomId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Many-to-one: ProProductCoverage -> ResTax
        builder.HasOne(e => e.Tax)
            .WithMany()
            .HasForeignKey(e => e.TaxId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
