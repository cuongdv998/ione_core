using System;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyCoverageConfiguration : IEntityTypeConfiguration<PolicyCoverage>
{
    public void Configure(EntityTypeBuilder<PolicyCoverage> builder)
    {
        builder.ToTable("policy_coverage", t =>
        {
            t.HasComment("Bảng lưu các phạm vi bảo hiểm của đơn bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyProductId)
            .HasColumnName("policy_product_id")
            .IsRequired()
            .HasComment("Tham chiếu đến sản phẩm bảo hiểm của đơn");

        builder.Property(x => x.CoverageId)
            .HasColumnName("coverage_id")
            .IsRequired()
            .HasComment("Tham chiếu đến phạm vi bảo hiểm");

        builder.Property(x => x.CoverageParentId)
            .HasColumnName("coverage_parent_id")
            .HasComment("Tham chiếu đến phạm vi bảo hiểm cha (nếu có)");

        builder.Property(x => x.InsurerCoverageCode)
            .HasColumnName("insurer_coverage_code")
            .HasMaxLength(50)
            .HasComment("Mã phạm vi bảo hiểm tương ứng của BH gốc");

        builder.Property(x => x.UomId)
            .HasColumnName("uom_id")
            .IsRequired()
            .HasComment("Đơn vị đo lường");

        builder.Property(x => x.TableRateLineId)
            .HasColumnName("table_rate_line_id")
            .HasComment("Tham chiếu đến bảng tỷ lệ");

        builder.Property(x => x.AmountLiability)
            .HasColumnName("amount_liability")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Mức trách nhiệm");

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Số lượng tham gia");

        builder.Property(x => x.TaxId)
            .HasColumnName("tax_id")
            .IsRequired()
            .HasComment("Thuế suất");

        builder.Property(x => x.NetRate)
            .HasColumnName("net_rate")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Phí thuần (theo đăng ký với bộ tài chính)");

        builder.Property(x => x.BaseRate)
            .HasColumnName("base_rate")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Phí cơ bản (>= phí thuần)");

        builder.Property(x => x.FlatRate)
            .HasColumnName("flat_rate")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Phí theo số tiền cố định");

        builder.Property(x => x.Loading)
            .HasColumnName("loading")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Phí bổ sung thêm");

        builder.Property(x => x.PremiumRate)
            .HasColumnName("premium_rate")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tỷ lệ phí bảo hiểm (= phí cơ sở + phí bổ sung)");

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
        builder.HasIndex(e => e.PolicyProductId, "ix_policy_coverage_policy_product_id");
        builder.HasIndex(e => e.CoverageId, "ix_policy_coverage_coverage_id");

        // Foreign Key Relationships
        // PolicyProduct
        builder.HasOne(x => x.PolicyProduct)
            .WithMany(x => x.PolicyCoverages)
            .HasForeignKey(x => x.PolicyProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_coverage_policy_product_id");

        // ProCoverage
        builder.HasOne(x => x.Coverage)
            .WithMany(x => x.PolicyCoverages)
            .HasForeignKey(x => x.CoverageId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_coverage_coverage_id");
    }
}
