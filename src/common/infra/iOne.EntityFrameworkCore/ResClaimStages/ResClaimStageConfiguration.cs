using System;
using iOne.ResClaimStages;
using iOne.ResClaimTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResClaimStages;

public class ResClaimStageConfiguration : IEntityTypeConfiguration<ResClaimStage>
{
    public void Configure(EntityTypeBuilder<ResClaimStage> builder)
    {
        builder.ToTable("res_claim_stage", t =>
        {
            t.HasComment("Bảng định nghĩa các giai đoạn xử lý bồi thường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimTypeId).HasColumnName("claimtypeid").IsRequired();
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(25).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(250).IsRequired();
        builder.Property(x => x.SurveyPlanId).HasColumnName("surveyplanid");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResClaimStageStatus>(v, true))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.ClaimType)
            .WithMany()
            .HasForeignKey(x => x.ClaimTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

