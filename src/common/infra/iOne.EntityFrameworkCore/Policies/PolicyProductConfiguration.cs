using System;
using iOne.Policies;
using iOne.ProProducts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyProductConfiguration : IEntityTypeConfiguration<PolicyProduct>
{
    public void Configure(EntityTypeBuilder<PolicyProduct> builder)
    {
        builder.ToTable("policy_product", t =>
        {
            t.HasComment("Bảng lưu sản phẩm liên quan đến policy");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyVersionId)
            .HasColumnName("policy_version_id")
            .IsRequired()
            .HasComment("Tham chiếu đến phiên bản đơn bảo hiểm");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired()
            .HasComment("Tham chiếu đến sản phẩm bảo hiểm");

        builder.Property(x => x.InsurerProductCode)
            .HasColumnName("insurer_product_code")
            .HasMaxLength(50)
            .HasComment("Mã sản phẩm tương ứng của BH gốc");

        builder.Property(x => x.AmountLiability)
            .HasColumnName("amount_liability")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Mức trách nhiệm");

        builder.Property(x => x.PremiumTotal)
            .HasColumnName("premium_total")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tổng phí bảo hiểm");

        builder.Property(x => x.Premium)
            .HasColumnName("premium")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Phí bảo hiểm trước thuế");

        builder.Property(x => x.Vat)
            .HasColumnName("vat")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tiền thuế");

        builder.Property(x => x.Discount)
            .HasColumnName("discount")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Giảm phí theo số tiền");

        builder.Property(x => x.DiscountRate)
            .HasColumnName("discount_rate")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Giảm phí theo tỷ lệ");

        builder.Property(x => x.Markup)
            .HasColumnName("markup")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Markup");

        // Audit columns
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // Indexes
        builder.HasIndex(e => e.PolicyVersionId, "ix_policy_product_policy_version_id");
        builder.HasIndex(e => e.ProductId, "ix_policy_product_product_id");

        // Foreign Key Relationships
        // PolicyVersion
        builder.HasOne(x => x.PolicyVersion)
            .WithMany(x => x.PolicyProducts)
            .HasForeignKey(x => x.PolicyVersionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_product_policy_version_id");

        // ProProduct
        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
