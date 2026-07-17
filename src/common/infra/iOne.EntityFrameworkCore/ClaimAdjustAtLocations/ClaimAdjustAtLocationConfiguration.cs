using System;
using iOne.ClaimAdjustAtLocations;
using iOne.Claims;
using iOne.HrEmployees;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimAdjustAtLocations;

public class ClaimAdjustAtLocationConfiguration : IEntityTypeConfiguration<ClaimAdjustAtLocation>
{
    public void Configure(EntityTypeBuilder<ClaimAdjustAtLocation> builder)
    {
        builder.ToTable("claim_adjust_at_location", t =>
        {
            t.HasComment("Bảng lưu thông tin giám định hiện trường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ClaimId)
            .HasColumnName("claimid")
            .IsRequired();

        builder.Property(x => x.AdjustorId)
            .HasColumnName("adjustorid")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("startdate");

        builder.Property(x => x.EndDate)
            .HasColumnName("enddate");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.HasValue ? v.Value.ToString().ToLowerInvariant() : null,
                v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<ClaimAdjustAtLocationStatus>(v, true))
            .HasMaxLength(15)
            .HasComment("Trạng thái giám định hiện trường");

        builder.Property(x => x.LossPosition)
            .HasColumnName("lossposition")
            .HasMaxLength(50);

        builder.Property(x => x.HasLossThirdParty)
            .HasColumnName("haslossthirdparty")
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(x => x.WitnessTestimony)
            .HasColumnName("witnesstestimony")
            .HasMaxLength(500);

        builder.Property(x => x.CauseDescription)
            .HasColumnName("causedescription")
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.LocationDescription)
            .HasColumnName("locationdescription")
            .HasMaxLength(500);

        builder.Property(x => x.DamageDescription)
            .HasColumnName("damagedescription")
            .HasMaxLength(500);

        builder.Property(x => x.PartiesInvolvedDescription)
            .HasColumnName("partiesinvolveddescription")
            .HasMaxLength(500);

        builder.Property(x => x.AddressPlan)
            .HasColumnName("addressplan")
            .HasMaxLength(500);

        builder.Property(x => x.CustomerRecommendation)
            .HasColumnName("customerrecommendation")
            .HasMaxLength(500);

        builder.Property(x => x.OtherDescription)
            .HasColumnName("otherdescription")
            .HasMaxLength(500);

        builder.Property(x => x.IsInScope)
            .HasColumnName("isinscope")
            .HasMaxLength(1);

        builder.Property(x => x.GarageId)
            .HasColumnName("garageid");

        builder.Property(x => x.IssueDate)
            .HasColumnName("issuedate");

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.Claim)
            .WithMany()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Adjustor)
            .WithMany()
            .HasForeignKey(x => x.AdjustorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Garage)
            .WithMany()
            .HasForeignKey(x => x.GarageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

