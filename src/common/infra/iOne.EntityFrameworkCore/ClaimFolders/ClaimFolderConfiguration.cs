using System;
using iOne.ClaimFolders;
using iOne.Claims;
using iOne.HrEmployees;
using iOne.ResClaimTypes;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolders;

public class ClaimFolderConfiguration : IEntityTypeConfiguration<ClaimFolder>
{
    public void Configure(EntityTypeBuilder<ClaimFolder> builder)
    {
        builder.ToTable("claim_folder", t =>
        {
            t.HasComment("Hồ sơ bồi thường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FolderNo)
            .HasColumnName("folderno")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Số hồ sơ");

        builder.Property(x => x.FolderName)
            .HasColumnName("foldername")
            .HasMaxLength(250)
            .HasComment("Tên hồ sơ");

        builder.Property(x => x.InsurerId)
            .HasColumnName("insurerid")
            .HasComment("Đối tác bảo hiểm gốc liên quan");

        builder.Property(x => x.ClaimId)
            .HasColumnName("claimid")
            .IsRequired()
            .HasComment("Lần thông báo tổn thất");

        builder.Property(x => x.IncidentId)
            .HasColumnName("incidentid")
            .HasComment("Vụ tổn thất");

        builder.Property(x => x.ProductId)
            .HasColumnName("productid");

        builder.Property(x => x.ClaimTypeId)
            .HasColumnName("claimtypeid")
            .IsRequired()
            .HasComment("Loại claim");

        builder.Property(x => x.PolicyNo)
            .HasColumnName("policyno")
            .HasMaxLength(50)
            .HasComment("Mã đơn bảo hiểm");

        builder.Property(x => x.InsurerPolicyNo)
            .HasColumnName("insurerpolicyno")
            .HasMaxLength(50)
            .HasComment("Mã đơn bảo hiểm của Cty bảo hiểm gốc");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ClaimFolderStatus>(v, true))
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái hiện tại của hồ sơ: new, inprogress, closed, cancelled");

        builder.Property(x => x.Stage)
            .HasColumnName("stage")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Giai đoạn xử lý hiện tại");

        builder.Property(x => x.OpenDate)
            .HasColumnName("opendate")
            .IsRequired();

        builder.Property(x => x.OpenEmployeeId)
            .HasColumnName("openemployeeid");

        builder.Property(x => x.CloseDate)
            .HasColumnName("closedate");

        builder.Property(x => x.CloseEmployeeId)
            .HasColumnName("closeemployeeid");

        builder.Property(x => x.CloseNote)
            .HasColumnName("closenote")
            .HasMaxLength(500);

        builder.Property(x => x.CancelReasonId)
            .HasColumnName("cancelreasonid");

        builder.Property(x => x.CancelDate)
            .HasColumnName("canceldate");

        builder.Property(x => x.CancelEmployeeId)
            .HasColumnName("cancelemployeeid");

        builder.Property(x => x.CancelNote)
            .HasColumnName("cancelnote")
            .HasMaxLength(500);

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasConversion<string>(
                v => v.HasValue ? v.Value.ToString().ToLowerInvariant() : null,
                v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<ClaimFolderPriority>(v, true))
            .HasMaxLength(15)
            .HasComment("Mức độ ưu tiên: high, medium, low");

        builder.Property(x => x.HasAdjustLocation)
            .HasColumnName("hasadjustlocation")
            .HasMaxLength(1)
            .IsRequired()
            .HasComment("Có giám định hiện trường hay không: Y/N");

        // Audit columns
        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        // Relationships
        builder.HasOne(x => x.Claim)
            .WithMany()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimType)
            .WithMany()
            .HasForeignKey(x => x.ClaimTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OpenEmployee)
            .WithMany()
            .HasForeignKey(x => x.OpenEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CloseEmployee)
            .WithMany()
            .HasForeignKey(x => x.CloseEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Insurer)
            .WithMany()
            .HasForeignKey(x => x.InsurerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

