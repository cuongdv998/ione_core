using System;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyVersionConfiguration : IEntityTypeConfiguration<PolicyVersion>
{
    public void Configure(EntityTypeBuilder<PolicyVersion> builder)
    {
        builder.ToTable("policy_version", t =>
        {
            t.HasComment("Phiên bản đơn bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasColumnType("NUMERIC(2)")
            .IsRequired()
            .HasComment("Phiên bản");

        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id")
            .IsRequired()
            .HasComment("Tham chiếu đến đơn bảo hiểm gốc");

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại Policy: O (policy gốc), A (policy sửa đổi bổ sung)");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái đơn: quotation (báo giá), draft (nháp), active (đang hiệu lực), expired (hết hạn), terminated (chấm dứt), cancelled (hủy)");

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired()
            .HasComment("Ngày hiệu lực");

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date")
            .IsRequired()
            .HasComment("Ngày hết hạn");

        builder.Property(x => x.OrgEffectDate)
            .HasColumnName("org_effect_date")
            .IsRequired()
            .HasComment("Ngày hiệu lực gốc");

        builder.Property(x => x.OrgExpireDate)
            .HasColumnName("org_expire_date")
            .IsRequired()
            .HasComment("Ngày hết hạn gốc");

        builder.Property(x => x.InternalNote)
            .HasColumnName("internal_note")
            .HasMaxLength(500)
            .HasComment("Ghi chú nội bộ");

        builder.Property(x => x.CustomerNote)
            .HasColumnName("customer_note")
            .HasMaxLength(500)
            .HasComment("Ghi chú khách hàng");

        builder.Property(x => x.PremiumTotal)
            .HasColumnName("premium_total")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tổng phí bảo hiểm");

        builder.Property(x => x.Premium)
            .HasColumnName("premium")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Phí bảo hiểm");

        builder.Property(x => x.Vat)
            .HasColumnName("vat")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tiền thuế");

        builder.Property(x => x.Discount)
            .HasColumnName("discount")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Số tiền giảm phí");

        builder.Property(x => x.DiscountRate)
            .HasColumnName("discount_rate")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Tỷ lệ giảm phí");

        builder.Property(x => x.Markup)
            .HasColumnName("markup")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Markup");

        builder.Property(x => x.ApprovalDate)
            .HasColumnName("approval_date")
            .HasComment("Ngày duyệt");

        builder.Property(x => x.ApproverId)
            .HasColumnName("approver_id")
            .HasComment("Người duyệt");

        builder.Property(x => x.ApprovalStatus)
            .HasColumnName("approval_status")
            .HasMaxLength(15)
            .HasComment("Trạng thái duyệt: pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối duyệt)");

        builder.Property(x => x.TerminationStatus)
            .HasColumnName("termination_status")
            .HasComment("Trạng thái chấm dứt: 0-Pending (chờ duyệt chấm dứt), 1-Approved (đã duyệt chấm dứt), 2-Rejected (từ chối chấm dứt)");

        builder.Property(x => x.RefundAmount)
            .HasColumnName("refund_amount")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Tổng phí cần hoàn");

        builder.Property(x => x.InsurerIntegrationStatus)
            .HasColumnName("insurer_integration_status")
            .HasMaxLength(15)
            .HasComment("Trạng thái tích hợp với BH gốc: fail (lỗi tích hợp), succ (thành công)");

        builder.Property(x => x.InsurerIntegrationDescription)
            .HasColumnName("insurer_integration_description")
            .HasColumnType("TEXT")
            .HasComment("Mô tả tích hợp với BH gốc");

        builder.Property(x => x.EndorsementType)
            .HasColumnName("endorsement_type")
            .HasComment("Loại sửa đổi bổ sung");

        builder.Property(x => x.EndorsementReasonId)
            .HasColumnName("endorsement_reason_id")
            .HasComment("Lý do sửa đổi bổ sung");

        builder.Property(x => x.EndorsementDescription)
            .HasColumnName("endorsement_description")
            .HasMaxLength(300)
            .HasComment("Mô tả sửa đổi bổ sung");

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
        builder.HasIndex(e => e.PolicyId, "ix_policy_version_policy_id");

        // Foreign Key Relationships
        // Policy
        builder.HasOne(x => x.Policy)
            .WithMany(x => x.PolicyVersions)
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_version_policy_id");
    }
}
