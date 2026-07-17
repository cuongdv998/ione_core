using iOne.ClaimFolderExposures;
using iOne.ClaimFolders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderExposures;

public class ClaimFolderExposureConfiguration : IEntityTypeConfiguration<ClaimFolderExposure>
{
    public void Configure(EntityTypeBuilder<ClaimFolderExposure> builder)
    {
        builder.ToTable("claim_folder_exposure", t =>
        {
            t.HasComment("Bảng lưu các phạm vi bồi thường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimFolderId).HasColumnName("claimfolderid").IsRequired();
        builder.Property(x => x.ClaimFolderIncidentObjectId).HasColumnName("claimfolderincidentobjectid").IsRequired();
        builder.Property(x => x.CoverageCode).HasColumnName("coveragecode").HasMaxLength(50).IsRequired();
        builder.Property(x => x.CoverageParentCode).HasColumnName("coverageparentcode").HasMaxLength(50);
        builder.Property(x => x.InsurerCoverageCode).HasColumnName("insurercoveragecode").HasMaxLength(50);
        builder.Property(x => x.EstimateAmount).HasColumnName("estimateamount");
        builder.Property(x => x.RepairDiscount).HasColumnName("repairdiscount");
        builder.Property(x => x.RepairDiscountPercent).HasColumnName("repairdiscountpercent");
        builder.Property(x => x.MechanismIndemnifyReasonId).HasColumnName("mechanismindemnifyreasonid");
        builder.Property(x => x.MechanismIndemnifyAmount).HasColumnName("mechanismindemnifyamount");
        builder.Property(x => x.MechanismIndemnifyPercent).HasColumnName("mechanismindemnifypercent");
        builder.Property(x => x.ClaimAmount).HasColumnName("claimamount");
        builder.Property(x => x.DeductibleAmount).HasColumnName("deductibleamount");
        builder.Property(x => x.DeductibleTaxId).HasColumnName("deductibletaxid");
        builder.Property(x => x.LiabilityAmount).HasColumnName("liabilityamount");
        builder.Property(x => x.LimitLiabilityAmount).HasColumnName("limitliabilityamount");
        builder.Property(x => x.ExpenseAmount).HasColumnName("expenseamount");
        builder.Property(x => x.DepreciationAmount).HasColumnName("depreciationamount");
        builder.Property(x => x.LossLimitAmount).HasColumnName("losslimitamount");

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

