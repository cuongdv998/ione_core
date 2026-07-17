using System;
using iOne.ClaimSlas;
using iOne.ResClaimStages;
using iOne.ResPartners;
using iOne.ResTaskCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimSlas;

public class ClaimSlaConfiguration : IEntityTypeConfiguration<ClaimSla>
{
    public void Configure(EntityTypeBuilder<ClaimSla> builder)
    {
        builder.ToTable("claim_sla", t =>
        {
            t.HasComment("Định nghĩa SLA cho quá trình claim");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.InsurerId).HasColumnName("insurerid").IsRequired();
        builder.Property(x => x.ClaimStageId).HasColumnName("claimstageid").IsRequired();
        builder.Property(x => x.TaskId).HasColumnName("taskid");
        builder.Property(x => x.SlaTime).HasColumnName("slatime").IsRequired();
        builder.Property(x => x.EffectDate).HasColumnName("effectdate").IsRequired();
        builder.Property(x => x.ExpireDate).HasColumnName("expiredate");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ClaimSlaStatus>(v, true))
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.Insurer)
            .WithMany()
            .HasForeignKey(x => x.InsurerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimStage)
            .WithMany()
            .HasForeignKey(x => x.ClaimStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Task)
            .WithMany()
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

