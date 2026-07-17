using iOne.ClaimFolderItems;
using iOne.ClaimFolders;
using iOne.ClaimFolderIncidentObjects;
using iOne.ClaimFolderExposures;
using iOne.ResDamageLevels;
using iOne.ResObjectTypeItems;
using iOne.ResRisks;
using iOne.ResUoms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderItems;

public class ClaimFolderItemConfiguration : IEntityTypeConfiguration<ClaimFolderItem>
{
    public void Configure(EntityTypeBuilder<ClaimFolderItem> builder)
    {
        builder.ToTable("claim_folder_item", t =>
        {
            t.HasComment("Các hạng mục tổn thất (khi giám định chi tiết)");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimFolderId).HasColumnName("claimfolderid").IsRequired();
        builder.Property(x => x.ClaimFolderIncidentObjectId).HasColumnName("claimfolderincidentobjectid").IsRequired();
        builder.Property(x => x.ClaimFolderExposureId).HasColumnName("claimfolderexposureid").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("itemid");
        builder.Property(x => x.RiskId).HasColumnName("riskid");
        builder.Property(x => x.ClaimPlanId).HasColumnName("claimplanid");
        builder.Property(x => x.DemageLevelId).HasColumnName("demagelevelid");
        builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(x => x.UomId).HasColumnName("uomid");
        builder.Property(x => x.IssueDate).HasColumnName("issuedate");
        builder.Property(x => x.LossValue).HasColumnName("lossvalue");
        builder.Property(x => x.IsRecovery).HasColumnName("isrecovery").HasMaxLength(1).IsRequired();
        builder.Property(x => x.IsCover).HasColumnName("iscover").HasMaxLength(1);
        builder.Property(x => x.DepreciationPercent).HasColumnName("depreciationpercent");
        builder.Property(x => x.IsGenuien).HasColumnName("isgenuien").HasMaxLength(1);
        builder.Property(x => x.Position).HasColumnName("position").HasMaxLength(15);
        builder.Property(x => x.CoveragePercent).HasColumnName("coveragepercent");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(250);

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimFolderIncidentObject)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderIncidentObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimFolderExposure)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderExposureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Risk)
            .WithMany()
            .HasForeignKey(x => x.RiskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DemageLevel)
            .WithMany()
            .HasForeignKey(x => x.DemageLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Uom)
            .WithMany()
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
