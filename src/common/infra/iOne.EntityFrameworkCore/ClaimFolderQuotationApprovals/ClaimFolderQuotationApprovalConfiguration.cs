using iOne.ClaimFolderQuotationApprovals;
using iOne.ClaimFolders;
using iOne.Claims;
using iOne.ResPartners;
using iOne.ResReasons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderQuotationApprovals;

public class ClaimFolderQuotationApprovalConfiguration : IEntityTypeConfiguration<ClaimFolderQuotationApproval>
{
    public void Configure(EntityTypeBuilder<ClaimFolderQuotationApproval> builder)
    {
        builder.ToTable("claim_folder_quotation_approval", t =>
        {
            t.HasComment("Phê duyệt PASC/Báo giá");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ClaimId)
            .HasColumnName("claim_id")
            .IsRequired()
            .HasComment("Yêu cầu bồi thường");

        builder.Property(x => x.ClaimFolderId)
            .HasColumnName("claim_folder_id")
            .HasComment("Hồ sơ (tùy chọn)");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id")
            .HasComment("Đối tác chịu trách nhiệm");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(
                v => ClaimFolderQuotationApprovalStatusDatabase.ToColumnValue(v),
                v => ClaimFolderQuotationApprovalStatusDatabase.FromColumnValue(v))
            .HasMaxLength(50)
            .IsRequired()
            .HasComment(
                "Trạng thái: new (mới), inprogress (đang xử lý), pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối), done (hoàn thành), cancelled (hủy)");

        builder.Property(x => x.SubmittedDate)
            .HasColumnName("submitted_date")
            .HasColumnType("date")
            .IsRequired()
            .HasComment("Ngày trình duyệt");

        builder.Property(x => x.SubmitterId)
            .HasColumnName("submitter_id")
            .IsRequired()
            .HasComment("Người trình duyệt");

        builder.Property(x => x.ApprovedDate)
            .HasColumnName("approved_date")
            .HasColumnType("date")
            .HasComment("Ngày phê duyệt");

        builder.Property(x => x.ApproverId)
            .HasColumnName("approver_id")
            .HasComment("Người phê duyệt");

        builder.Property(x => x.ClaimAmount)
            .HasColumnName("claim_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Số tiền yêu cầu bồi thường");

        builder.Property(x => x.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Tổng tiền giảm giá");

        builder.Property(x => x.DepreciationAmount)
            .HasColumnName("depreciation_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Tổng khấu hao phụ tùng");

        builder.Property(x => x.ExpenseAmount)
            .HasColumnName("expense_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Số tiền sẽ chi trả");

        builder.Property(x => x.AssessmentAmount)
            .HasColumnName("assessment_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Chi phí giám định");

        builder.Property(x => x.LossPreventionAmount)
            .HasColumnName("loss_prevention_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Chi phí đề phòng hạn chế tổn thất");

        builder.Property(x => x.RescueAmount)
            .HasColumnName("rescue_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Chi phí cứu hộ");

        builder.Property(x => x.OtherAmount)
            .HasColumnName("other_amount")
            .HasColumnType("numeric(15,3)")
            .HasComment("Chi phí khác");

        builder.Property(x => x.Data)
            .HasColumnName("data")
            .HasColumnType("text")
            .HasComment("Dữ liệu snapshot của lần trình duyệt (JSON)");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(250);

        builder.Property(x => x.ReasonId)
            .HasColumnName("reason_id")
            .HasComment("Lý do (bắt buộc khi từ chối)");

        builder.Property(x => x.ReasonDescription)
            .HasColumnName("reason_description")
            .HasMaxLength(250)
            .HasComment("Mô tả lý do");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        builder.HasIndex(x => x.ClaimId, "ix_claim_folder_quotation_approval_claim_id");
        builder.HasIndex(x => x.ClaimFolderId, "ix_claim_folder_quotation_approval_claim_folder_id");
        builder.HasIndex(x => x.Status, "ix_claim_folder_quotation_approval_status");

        builder.HasOne(x => x.Claim)
            .WithMany()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_claim_folder_quotation_approval_claim");

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_claim_folder_quotation_approval_claim_folder");

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reason)
            .WithMany()
            .HasForeignKey(x => x.ReasonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
