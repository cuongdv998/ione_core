using System;
using iOne.AccountPaymentRequests;
using iOne.Claims;
using iOne.HrEmployees;
using iOne.ResCurrencies;
using iOne.ResCustomers;
using iOne.ResPartners;
using iOne.ResPaymentMethods;
using iOne.ResPaymentTypes;
using iOne.ResReasons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.AccountPaymentRequests;

public class AccountPaymentRequestConfiguration : IEntityTypeConfiguration<AccountPaymentRequest>
{
    public void Configure(EntityTypeBuilder<AccountPaymentRequest> builder)
    {
        builder.ToTable("account_payment_request", t =>
        {
            t.HasComment("Bảng đề nghị thanh toán");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .HasComment("Khách hàng nhận thanh toán");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id")
            .HasComment("Đối tác nhận thanh toán");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(x => x.IssueDate)
            .HasColumnName("issue_date")
            .IsRequired()
            .HasComment("Ngày thanh toán");

        builder.Property(x => x.PaymentMethodId)
            .HasColumnName("payment_method_id")
            .IsRequired()
            .HasComment("Phương thức thanh toán");

        builder.Property(x => x.PaymentTypeId)
            .HasColumnName("payment_type_id")
            .IsRequired()
            .HasComment("Loại thanh toán: Tạm ứng, Thanh toán công nợ");

        builder.Property(x => x.CurrencyId)
            .HasColumnName("currency_id")
            .IsRequired()
            .HasComment("Tiền tệ");

        builder.Property(x => x.ClaimFolderId)
            .HasColumnName("claim_folder_id")
            .HasComment("ID của claim folder liên quan");

        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id")
            .HasComment("ID đơn bảo hiểm liên quan (thanh toán công nợ)");

        builder.Property(x => x.DueDate)
            .HasColumnName("due_date")
            .IsRequired()
            .HasComment("Hạn thanh toán");

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(15, 3)
            .IsRequired()
            .HasComment("Tổng giá trị thanh toán");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<AccountPaymentRequestStatus>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: draft, pending_approval, approved, rejected, cancelled");

        builder.Property(x => x.SubmittedDate)
            .HasColumnName("submitted_date")
            .HasComment("Ngày trình");

        builder.Property(x => x.SubmitterId)
            .HasColumnName("submitter_id")
            .HasComment("Người trình");

        builder.Property(x => x.ApproveEmpId)
            .HasColumnName("approve_emp_id")
            .HasComment("Người duyệt");

        builder.Property(x => x.ApprovedDate)
            .HasColumnName("approved_date")
            .HasComment("Ngày duyệt");

        builder.Property(x => x.ReasonId)
            .HasColumnName("reason_id")
            .HasComment("Lý do, liên kết với bảng ResReason (sử dụng khi từ chối duyệt)");

        builder.Property(x => x.ReasonDescription)
            .HasColumnName("reason_description")
            .HasMaxLength(250)
            .HasComment("Mô tả khi chọn lý do");

        builder.Property(x => x.TransRef)
            .HasColumnName("trans_ref")
            .HasMaxLength(255)
            .HasComment("Mã tham chiếu giao dịch (payment gateway / đối tác)");

        builder.Property(x => x.PaymentProvider)
            .HasColumnName("payment_provider")
            .HasMaxLength(255)
            .HasComment("Nhà cung cấp thanh toán (VNPAY, MOMO, ...)");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");

        // Relationships
        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentMethod)
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentType)
            .WithMany()
            .HasForeignKey(x => x.PaymentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Submitter)
            .WithMany()
            .HasForeignKey(x => x.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApproveEmp)
            .WithMany()
            .HasForeignKey(x => x.ApproveEmpId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reason)
            .WithMany()
            .HasForeignKey(x => x.ReasonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
