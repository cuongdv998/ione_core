using System;
using iOne.ClaimFolderExposureEstimates;
using iOne.ClaimFolders;
using iOne.ResFeeItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderExposureEstimates;

public class ClaimFolderExposureEstimateConfiguration : IEntityTypeConfiguration<ClaimFolderExposureEstimate>
{
    public void Configure(EntityTypeBuilder<ClaimFolderExposureEstimate> builder)
    {
        builder.ToTable("claim_folder_exposure_estimate", t =>
        {
            t.HasComment("Bảng ước bồi thường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ClaimFolderId)
            .HasColumnName("claimfolderid");

        builder.Property(x => x.FeeItemId)
            .HasColumnName("feeitemid")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ClaimFolderExposureEstimateStatus>(v, true))
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FeeItem)
            .WithMany()
            .HasForeignKey(x => x.FeeItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

