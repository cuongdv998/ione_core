using iOne.ResClaimStageTasks;
using iOne.ResClaimStages;
using iOne.ResTaskCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iOne.EntityFrameworkCore.ResClaimStageTasks;

public class ResClaimStageTaskConfiguration : IEntityTypeConfiguration<ResClaimStageTask>
{
    public void Configure(EntityTypeBuilder<ResClaimStageTask> builder)
    {
        builder.ToTable("res_claim_stage_task");

        builder.HasKey(x => new { x.ClaimStageId, x.TaskCategoryId });

        builder.Property(x => x.ClaimStageId).HasColumnName("claimstageid");
        builder.Property(x => x.TaskCategoryId).HasColumnName("taskcategoryid");

        builder.HasOne(x => x.ClaimStage)
            .WithMany()
            .HasForeignKey(x => x.ClaimStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TaskCategory)
            .WithMany()
            .HasForeignKey(x => x.TaskCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

