using System;
using iOne.Claims;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.ProLineOfBusinesses;
using iOne.ResClaimTypes;
using iOne.ResPartners;
using iOne.ResReasons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Claims;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("claim", t =>
        {
            t.HasComment("Bảng lưu yêu cầu bồi thường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        // Foreign Keys
        builder.Property(x => x.LobId)
            .HasColumnName("lob_id")
            .IsRequired()
            .HasComment("Id link đến nghiệp vụ bảo hiểm");

        builder.Property(x => x.ClaimTypeId)
            .HasColumnName("claim_type_id")
            .HasComment("Loại claim, dựa vào loại này có thể có các process claim khác nhau (ví dụ: vehicle, property, health sẽ có process khác nhau)");

        builder.Property(x => x.InsurerId)
            .HasColumnName("insurer_id")
            .HasComment("Đối tác bảo hiểm gốc liên quan");

        builder.Property(x => x.CancelReasonId)
            .HasColumnName("cancel_reason_id")
            .HasComment("Lý do khi hủy yêu cầu");

        builder.Property(x => x.OpenEmployeeId)
            .HasColumnName("open_employee_id")
            .IsRequired()
            .HasComment("Nhân viên mở yêu cầu");

        builder.Property(x => x.CloseEmployeeId)
            .HasColumnName("close_employee_id")
            .HasComment("Nhân viên đóng yêu cầu");

        builder.Property(x => x.CancelEmployeeId)
            .HasColumnName("cancel_employee_id")
            .HasComment("Nhân viên hủy yêu cầu");

        builder.Property(x => x.ProcessDeptId)
            .HasColumnName("process_dept_id")
            .HasComment("Đơn vị giám định (khi assign)");

        builder.Property(x => x.ProcessEmpId)
            .HasColumnName("process_emp_id")
            .HasComment("Người giám định (khi assign)");

        builder.Property(x => x.CertificateNo)
            .HasColumnName("certificate_no")
            .HasMaxLength(50)
            .HasComment("Số GCN (Giấy chứng nhận bảo hiểm)");

        // Basic Info
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Mã tiếp nhận");

        builder.Property(x => x.ProcessClaimType)
            .HasColumnName("process_claim_type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ProcessClaimType>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Phân loại xử lý: own (broker xử lý), insurer (bảo hiểm gốc xử lý - cần theo dõi tiến độ)");

        builder.Property(x => x.InsurerIncidentCode)
            .HasColumnName("insurer_incident_code")
            .HasMaxLength(50)
            .HasComment("Mã tiếp nhận của đối tác");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ClaimStatus>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái vụ tổn thất: draft (Nháp), pending_receive (chờ xử lý), inprogress (đang xử lý), closed (đã đóng), called (đã hủy)");

        // Dates
        builder.Property(x => x.OpenDate)
            .HasColumnName("open_date")
            .IsRequired()
            .HasComment("Ngày mở");

        builder.Property(x => x.CloseDate)
            .HasColumnName("close_date")
            .HasComment("Ngày đóng");

        builder.Property(x => x.CancelDate)
            .HasColumnName("cancel_date")
            .HasComment("Ngày hủy");

        builder.Property(x => x.NotifyDate)
            .HasColumnName("notify_date")
            .IsRequired()
            .HasComment("Ngày thông báo");

        // Notifier Info
        builder.Property(x => x.NotifierName)
            .HasColumnName("notifier_name")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tên người thông báo");

        builder.Property(x => x.NotifierPhone)
            .HasColumnName("notifier_phone")
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Số điện thoại người thông báo");

        builder.Property(x => x.NotifierEmail)
            .HasColumnName("notifier_email")
            .HasMaxLength(50)
            .HasComment("Email người thông báo");

        builder.Property(x => x.NotifierInRelationship)
            .HasColumnName("notifier_in_relationship")
            .HasMaxLength(50)
            .HasComment("Moi quan he cua nguoi thong bao voi doi tuong dc bao hiem hoac nguoi dai dien. Lay theo cau hinh trong bang AdminConfig (code = CLAIM_RELATIONSHIP)");

        // Contact Info
        builder.Property(x => x.ContactName)
            .HasColumnName("contact_name")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tên người liên hệ");

        builder.Property(x => x.ContactPhone)
            .HasColumnName("contact_phone")
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Số điện thoại người liên hệ");

        builder.Property(x => x.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(50)
            .HasComment("Email người liên hệ");

        builder.Property(x => x.ContactInRelationship)
            .HasColumnName("contact_in_relationship")
            .HasMaxLength(50)
            .HasComment("Moi quan he cua nguoi liên hệ voi doi tuong dc bao hiem hoac nguoi dai dien. Lay theo cau hinh trong bang app_domain (code = CLAIM_RELATIONSHIP, group = M)");

        // Other Info
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ClaimPriority>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Mức độ ưu tiên: high (cao), medium (trung bình), low (thấp)");

        builder.Property(x => x.SnapshotLink)
            .HasColumnName("snapshot_link")
            .HasMaxLength(250)
            .HasComment("Link webview để người báo tổn thất bổ sung thông tin hiện trường và theo dõi tiến độ xử lý bồi thường");

        builder.Property(x => x.CancelNote)
            .HasColumnName("cancel_note")
            .HasMaxLength(500)
            .HasComment("Ghi chú khi hủy yêu cầu");

        // Audit columns: UPPERCASE (matching SQL schema)
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(e => e.LobId, "ix_claim_lob_id");
        builder.HasIndex(e => e.ClaimTypeId, "ix_claim_claim_type_id");
        builder.HasIndex(e => e.InsurerId, "ix_claim_insurer_id");
        builder.HasIndex(e => e.OpenEmployeeId, "ix_claim_open_employee_id");
        builder.HasIndex(e => e.Status, "ix_claim_status");

        // Foreign Key Relationships
        // LOB (ProLineOfBusiness)
        builder.HasOne(x => x.Lob)
            .WithMany()
            .HasForeignKey(x => x.LobId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CLAIM_REFERENCE_PROLINEO");

        // ClaimType (ResClaimType)
        builder.HasOne(x => x.ClaimType)
            .WithMany()
            .HasForeignKey(x => x.ClaimTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CLAIM_REFERENCE_RESCLAIM");

        // Insurer (ResPartner)
        builder.HasOne(x => x.Insurer)
            .WithMany()
            .HasForeignKey(x => x.InsurerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CLAIM_REFERENCE_RESPARTN");

        // CancelReason (ResReason)
        builder.HasOne(x => x.CancelReason)
            .WithMany()
            .HasForeignKey(x => x.CancelReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        // OpenEmployee (HrEmployee)
        builder.HasOne(x => x.OpenEmployee)
            .WithMany()
            .HasForeignKey(x => x.OpenEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // CloseEmployee (HrEmployee)
        builder.HasOne(x => x.CloseEmployee)
            .WithMany()
            .HasForeignKey(x => x.CloseEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // CancelEmployee (HrEmployee)
        builder.HasOne(x => x.CancelEmployee)
            .WithMany()
            .HasForeignKey(x => x.CancelEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProcessDepartment (HrDepartment) - Đơn vị giám định
        builder.HasOne(x => x.ProcessDepartment)
            .WithMany()
            .HasForeignKey(x => x.ProcessDeptId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProcessEmployee (HrEmployee) - Người giám định
        builder.HasOne(x => x.ProcessEmployee)
            .WithMany()
            .HasForeignKey(x => x.ProcessEmpId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
