using iOne.ClaimFolderItemPlans;
using iOne.ClaimFolderItems;
using iOne.ResClaimPlans;
using iOne.ResPartners;
using iOne.HrEmployees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderItemPlans;

public class ClaimFolderItemPlanConfiguration : IEntityTypeConfiguration<ClaimFolderItemPlan>
{
    public void Configure(EntityTypeBuilder<ClaimFolderItemPlan> builder)
    {
        builder.ToTable("claim_folder_item_plan", t =>
        {
            t.HasComment("Phương án bồi thường chi tiết cho hạng mục tổn thất");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimFolderItemId).HasColumnName("claimfolderitemid").IsRequired();
        builder.Property(x => x.ClaimPlanId).HasColumnName("claimplanid").IsRequired();
        builder.Property(x => x.PlanType).HasColumnName("plan_type").HasMaxLength(32);
        builder.Property(x => x.PartnerId).HasColumnName("partnerid");
        builder.Property(x => x.AdjustorId).HasColumnName("adjustorid");
        builder.Property(x => x.Amount).HasColumnName("amount");
        builder.Property(x => x.PartnerAmount).HasColumnName("partner_amount");
        builder.Property(x => x.AdjusterAmount).HasColumnName("adjuster_amount");
        builder.Property(x => x.ApprovedAmount).HasColumnName("approved_amount");
        builder.Property(x => x.DiscountPercent).HasColumnName("discount_percent");
        builder.Property(x => x.DiscountAmount).HasColumnName("discount_amount");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount");
        builder.Property(x => x.DepreciationPercent).HasColumnName("depreciation_percent");
        builder.Property(x => x.DepreciationAmount).HasColumnName("depreciation_amount");
        builder.Property(x => x.PredictAmountMin).HasColumnName("predictamountmin");
        builder.Property(x => x.PredictAmountMax).HasColumnName("predictamountmax");
        builder.Property(x => x.PredictAmountMedium).HasColumnName("predictamountmedium");
        builder.Property(x => x.IsWarning).HasColumnName("iswarning").HasMaxLength(1);

        builder.Property(x => x.CreationTime).HasColumnName("creationdate");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.ClaimFolderItem)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimPlan)
            .WithMany()
            .HasForeignKey(x => x.ClaimPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Adjustor)
            .WithMany()
            .HasForeignKey(x => x.AdjustorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ClaimFolderItemId, x.ClaimPlanId, x.PlanType })
            .HasDatabaseName("ix_claim_folder_item_plan_item_plan_type");
    }
}
