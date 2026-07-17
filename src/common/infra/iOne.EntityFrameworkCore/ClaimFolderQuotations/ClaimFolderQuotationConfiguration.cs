using iOne.ClaimFolderQuotations;
using iOne.ClaimFolders;
using iOne.ClaimFolderIncidentObjects;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderQuotations;

public class ClaimFolderQuotationConfiguration : IEntityTypeConfiguration<ClaimFolderQuotation>
{
    public void Configure(EntityTypeBuilder<ClaimFolderQuotation> builder)
    {
        builder.ToTable("claim_folder_quotation", t =>
        {
            t.HasComment("Báo giá của đối tác");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimFolderId).HasColumnName("claimfolderid").IsRequired();
        builder.Property(x => x.ClaimFolderIncidentObjectId).HasColumnName("claimfolderincidentobjectid");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(250);
        builder.Property(x => x.PartnerId).HasColumnName("partnerid").IsRequired();
        builder.Property(x => x.QuotationDate).HasColumnName("quotationdate").IsRequired();
        builder.Property(x => x.AmountTotal).HasColumnName("amounttotal").IsRequired();
        builder.Property(x => x.IsAccept).HasColumnName("isaccept").HasMaxLength(1).IsRequired();

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClaimFolderIncidentObject>()
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderIncidentObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

