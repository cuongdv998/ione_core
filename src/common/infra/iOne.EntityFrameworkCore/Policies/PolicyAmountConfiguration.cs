using iOne.Policies;
using iOne.ResFeeItems;
using iOne.ResPaymentMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyAmountConfiguration : IEntityTypeConfiguration<PolicyAmount>
{
    public void Configure(EntityTypeBuilder<PolicyAmount> builder)
    {
        builder.ToTable("policy_amount", t => { t.HasComment("Số tiền thanh toán của đơn bảo hiểm"); });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id")
            .IsRequired()
            .HasComment("Tham chiếu đến đơn bảo hiểm");

        builder.Property(x => x.PolicyVersionId)
            .HasColumnName("policy_version_id")
            .IsRequired()
            .HasComment("Tham chiếu đến phiên bản đơn bảo hiểm");

        builder.Property(x => x.FeeItemId)
            .HasColumnName("fee_item_id")
            .IsRequired()
            .HasComment("Tham chiếu đến khoản phí");

        builder.Property(x => x.IssueDate)
            .HasColumnName("issue_date")
            .HasColumnType("DATE")
            .IsRequired()
            .HasComment("Ngày phát hành");

        builder.Property(x => x.AmountTotal)
            .HasColumnName("amount_total")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tổng số tiền");

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Số tiền");

        builder.Property(x => x.Vat)
            .HasColumnName("vat")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("VAT");

        builder.Property(x => x.PaymentStatus)
            .HasColumnName("payment_status")
            .HasMaxLength(15)
            .IsRequired()
            .HasDefaultValue("new")
            .HasComment("Trạng thái thanh toán: new, paid, partial, cancelled");

        builder.Property(x => x.PaymentMethodId)
            .HasColumnName("payment_method_id")
            .HasComment("Tham chiếu đến hình thức thanh toán");

        builder.Property(x => x.PaymentDate)
            .HasColumnName("payment_date")
            .HasColumnType("DATE")
            .HasComment("Ngày thanh toán");

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
        builder.HasIndex(e => e.PolicyId, "ix_policy_amount_policy_id");
        builder.HasIndex(e => e.PolicyVersionId, "ix_policy_amount_policy_version_id");
        builder.HasIndex(e => e.FeeItemId, "ix_policy_amount_fee_item_id");
        builder.HasIndex(e => e.PaymentMethodId, "ix_policy_amount_payment_method_id");

        // Foreign Key Relationships
        builder.HasOne(x => x.Policy)
            .WithMany()
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_amount_policy_id");

        builder.HasOne<PolicyVersion>()
            .WithMany()
            .HasForeignKey(x => x.PolicyVersionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_amount_policy_version_id");

        builder.HasOne<ResFeeItem>()
            .WithMany()
            .HasForeignKey(x => x.FeeItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_amount_res_fee_item_id");

        builder.HasOne<ResPaymentMethod>()
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_amount_res_payment_method_id");
    }
}