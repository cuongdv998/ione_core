using System;
using iOne.ProProducts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProducts;

public class ProProductCoverageLevelTermConfiguration : IEntityTypeConfiguration<ProProductCoverageLevelTerm>
{
    public void Configure(EntityTypeBuilder<ProProductCoverageLevelTerm> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_coverage_level_term", t =>
        {
            t.HasComment("Bảng định nghĩa các hạn mức chi tiết của phạm vi bảo hiểm");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductCoverageLevelId)
            .HasColumnName("product_coverage_level_id")
            .HasComment("Tham chiếu đến nhóm hạn mức của phạm vi");

        builder.Property(x => x.CoverageLevelTypeId)
            .HasColumnName("coverage_level_type_id")
            .IsRequired()
            .HasComment("Loại hạn mức");

        builder.Property(x => x.CoverageLevelBasisId)
            .HasColumnName("coverage_level_basis_id")
            .HasComment("Cơ sở tính hạn mức");

        builder.Property(x => x.AmountType)
            .HasColumnName("amount_type")
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại giá trị: percent (Tỷ lệ), fix (Giá trị cố định), quantity (Số lượng)");

        builder.Property(x => x.FromAmount)
            .HasColumnName("from_amount")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Mức tối thiểu");

        builder.Property(x => x.ToAmount)
            .HasColumnName("to_amount")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Mức tối đa");

        builder.Property(x => x.ConditionScript)
            .HasColumnName("condition_script")
            .HasColumnType("TEXT")
            .HasComment("Script dạng python cho phép kiểm tra các điều kiện nhất định");

        builder.Property(x => x.ComputeScript)
            .HasColumnName("compute_script")
            .HasColumnType("TEXT")
            .HasComment("Script python tính toán ra giá trị");

        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasMaxLength(15)
            .HasComment("Hiển thị mặc định: Y (có), N (không)");

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

        // Indexes
        builder.HasIndex(e => e.ProductCoverageLevelId, "ix_pro_product_coverage_level_term_product_coverage_level_id");
        builder.HasIndex(e => e.CoverageLevelTypeId, "ix_pro_product_coverage_level_term_coverage_level_type_id");
        builder.HasIndex(e => e.CoverageLevelBasisId, "ix_pro_product_coverage_level_term_coverage_level_basis_id");

        // Configure relationships
        // Many-to-one: ProProductCoverageLevelTerm -> ProProductCoverageLevel
        builder.HasOne(e => e.ProductCoverageLevel)
            .WithMany(e => e.Terms)
            .HasForeignKey(e => e.ProductCoverageLevelId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Many-to-one: ProProductCoverageLevelTerm -> ProCoverageLevelType
        builder.HasOne(e => e.CoverageLevelType)
            .WithMany()
            .HasForeignKey(e => e.CoverageLevelTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Many-to-one: ProProductCoverageLevelTerm -> ProCoverageLevelBasis
        builder.HasOne(e => e.CoverageLevelBasis)
            .WithMany()
            .HasForeignKey(e => e.CoverageLevelBasisId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
