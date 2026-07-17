using iOne.ResClaimEvaluateItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResClaimEvaluateItems;

public class ResClaimEvaluateItemConfiguration : IEntityTypeConfiguration<ResClaimEvaluateItem>
{
    public void Configure(EntityTypeBuilder<ResClaimEvaluateItem> builder)
    {
        builder.ToTable("res_claim_evaluate_item");
        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimTypeId).HasColumnName("claimtypeid").IsRequired();
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(25).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(250).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(10).IsRequired();

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");
    }
}
