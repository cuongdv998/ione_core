using iOne.ClaimDocuments;
using iOne.Claims;
using iOne.ClaimFolders;
using iOne.ClaimAdjustAtLocations;
using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimDocuments;

public class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
{
    public void Configure(EntityTypeBuilder<ClaimDocument> builder)
    {
        builder.ToTable("claim_document", t =>
        {
            t.HasComment("Bảng lưu tài liệu liên quan đến vụ tổn thất");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ClaimId).HasColumnName("claimid");
        builder.Property(x => x.DocumentId).HasColumnName("documentid");
        builder.Property(x => x.DocumentTypeId).HasColumnName("doc_type_id");
        builder.Property(x => x.ClaimFolderId).HasColumnName("claimfolderid");
        builder.Property(x => x.ClaimFolderObjectId).HasColumnName("claimfolderobjectid");
        builder.Property(x => x.AdjustAtLocationId).HasColumnName("adjustatlocationid");
        builder.Property(x => x.ClaimFolderItemId).HasColumnName("claimfolderitemid");
        builder.Property(x => x.QuotationId).HasColumnName("quotationid");

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(250);

        builder.Property(x => x.Complete)
            .HasColumnName("complete")
            .HasMaxLength(1);

        builder.Property(x => x.IsCopy)
            .HasColumnName("iscopy")
            .HasMaxLength(1);

        builder.Property(x => x.IssueDate)
            .HasColumnName("issue_date");

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.Claim)
            .WithMany()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AdjustAtLocation)
            .WithMany()
            .HasForeignKey(x => x.AdjustAtLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
