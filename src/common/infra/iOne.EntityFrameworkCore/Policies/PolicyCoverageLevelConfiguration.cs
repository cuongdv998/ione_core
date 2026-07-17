using System;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyCoverageLevelConfiguration : IEntityTypeConfiguration<PolicyCoverageLevel>
{
    public void Configure(EntityTypeBuilder<PolicyCoverageLevel> builder)
    {
        builder.ToTable("policy_coverage_level", t =>
        {
            t.HasComment("Bảng lưu các hạn mức của phạm vi bảo hiểm theo đơn");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyCoverageId)
            .HasColumnName("policy_coverage_id")
            .IsRequired()
            .HasComment("Tham chiếu đến phạm vi bảo hiểm của đơn");

        builder.Property(x => x.CoverageLevelTypeId)
            .HasColumnName("coverage_level_type_id")
            .IsRequired()
            .HasComment("Loại hạn mức");

        builder.Property(x => x.CoverageLevelBasisId)
            .HasColumnName("coverage_level_basis_id")
            .IsRequired()
            .HasComment("Cơ sở tính hạn mức");

        builder.Property(x => x.ConditionScript)
            .HasColumnName("condition_script")
            .HasColumnType("TEXT")
            .HasComment("Script điều kiện áp dụng hạn mức");

        builder.Property(x => x.ComputeScript)
            .HasColumnName("compute_script")
            .HasColumnType("TEXT")
            .HasComment("Script tính toán giá trị hạn mức");

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
        builder.HasIndex(e => e.PolicyCoverageId, "ix_policy_coverage_level_policy_coverage_id");

        // Foreign Key Relationships
        // PolicyCoverage
        builder.HasOne(x => x.PolicyCoverage)
            .WithMany(x => x.PolicyCoverageLevels)
            .HasForeignKey(x => x.PolicyCoverageId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_coverage_level_policy_coverage_id");
    }
}
