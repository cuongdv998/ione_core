using System;
using iOne.ProProducts;
using iOne.ProTableRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProducts;

public class ProProductTableRateConfiguration : IEntityTypeConfiguration<ProProductTableRate>
{
    public void Configure(EntityTypeBuilder<ProProductTableRate> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_table_rate", t =>
        {
            t.HasComment("Bảng quan hệ nhiều-nhiều giữa ProProduct và ProTableRate");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.TableRateId)
            .HasColumnName("table_rate_id")
            .IsRequired();

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
        // Many-to-one: ProProductTableRate -> ProProduct
        builder.HasOne(e => e.Product)
            .WithMany(e => e.ProductTableRates)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProductTableRate -> ProTableRate
        builder.HasOne(e => e.TableRate)
            .WithMany()
            .HasForeignKey(e => e.TableRateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
