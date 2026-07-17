using iOne.ClaimFolderEvaluateDetails;
using iOne.ClaimFolderEvaluates;
using iOne.ResClaimEvaluateItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iOne.EntityFrameworkCore.ClaimFolderEvaluateDetails;

public class ClaimFolderEvaluateDetailConfiguration : IEntityTypeConfiguration<ClaimFolderEvaluateDetail>
{
    public void Configure(EntityTypeBuilder<ClaimFolderEvaluateDetail> builder)
    {
        builder.ToTable("claim_folde_revaluate_detail");

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimFolderEvaluateId).HasColumnName("claimfolderevaluateid").IsRequired();
        builder.Property(x => x.EvaluateItemId).HasColumnName("evaluateitemid").IsRequired();
        builder.Property(x => x.Result).HasColumnName("result").HasMaxLength(1).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(250);

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne<ClaimFolderEvaluate>()
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderEvaluateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ResClaimEvaluateItem>()
            .WithMany()
            .HasForeignKey(x => x.EvaluateItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
