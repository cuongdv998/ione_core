using iOne.ClaimFolderQuotationDetails;
using iOne.ClaimFolderQuotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderQuotationDetails;

public class ClaimFolderQuotationDetailConfiguration : IEntityTypeConfiguration<ClaimFolderQuotationDetail>
{
    public void Configure(EntityTypeBuilder<ClaimFolderQuotationDetail> builder)
    {
        builder.ToTable("claim_folder_quotation_detail", t =>
        {
            t.HasComment("Chi tiết các hạng mục báo giá");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QuotationId).HasColumnName("quotationid");
        builder.Property(x => x.ItemName).HasColumnName("itemname").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(x => x.Plan).HasColumnName("plan").HasMaxLength(50);
        builder.Property(x => x.Price).HasColumnName("price").IsRequired();
        builder.Property(x => x.AmountTotal).HasColumnName("amounttotal").IsRequired();

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne<ClaimFolderQuotation>()
            .WithMany()
            .HasForeignKey(x => x.QuotationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

