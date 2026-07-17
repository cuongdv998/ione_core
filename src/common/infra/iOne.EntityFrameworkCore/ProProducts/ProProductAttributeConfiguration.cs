using System;
using iOne.ProProducts;
using iOne.ProAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProducts;

public class ProProductAttributeConfiguration : IEntityTypeConfiguration<ProProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProProductAttribute> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_attribute", t =>
        {
            t.HasComment("Định nghĩa các đầu vào cần có của Product");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.AttributeId)
            .HasColumnName("attribute_id")
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .HasColumnName("is_required")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("N");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(15);

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
        // Many-to-one: ProProductAttribute -> ProProduct
        builder.HasOne(e => e.Product)
            .WithMany(e => e.ProductAttributes)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProductAttribute -> ProAttribute
        builder.HasOne(e => e.Attribute)
            .WithMany(e => e.ProductAttributes)
            .HasForeignKey(e => e.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
