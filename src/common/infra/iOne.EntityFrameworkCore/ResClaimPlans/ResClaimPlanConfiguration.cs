using System;
using iOne.ResClaimPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResClaimPlans;

public class ResClaimPlanConfiguration : IEntityTypeConfiguration<ResClaimPlan>
{
    public void Configure(EntityTypeBuilder<ResClaimPlan> builder)
    {
        builder.ToTable("res_claim_plan", t =>
        {
            t.HasComment("Phương án bồi thường");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.PaymentType)
            .HasColumnName("paymenttype")
            .HasMaxLength(15)
            .HasComment("in: thu tiền về, out: thanh toán ra");

        builder.Property(x => x.IsExpenses)
            .HasColumnName("isexpenses")
            .HasMaxLength(1)
            .HasComment("Y: có chi phí, N: không");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResClaimPlanStatus>(v, true))
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");
    }
}

