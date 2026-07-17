using System;
using iOne.ClaimStages;
using iOne.Claims;
using iOne.ClaimFolders;
using iOne.ResClaimStages;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimStages;

public class ClaimStageConfiguration : IEntityTypeConfiguration<ClaimStage>
{
    public void Configure(EntityTypeBuilder<ClaimStage> builder)
    {
        builder.ToTable("claim_stage", t =>
        {
            t.HasComment("Các giai đoạn xử lý bồi thường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ClaimId)
            .HasColumnName("claimid")
            .IsRequired();

        builder.Property(x => x.ClaimFolderId)
            .HasColumnName("claimfolderid");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partnerid");

        builder.Property(x => x.StageId)
            .HasColumnName("stageid")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ClaimStageStatus>(v, true))
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("startdate");

        builder.Property(x => x.DueDate)
            .HasColumnName("duedate")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("enddate");

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.Claim)
            .WithMany()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Stage)
            .WithMany()
            .HasForeignKey(x => x.StageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

