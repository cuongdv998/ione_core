using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResPartners;

public class ResPartnerAgreementConfiguration : IEntityTypeConfiguration<ResPartnerAgreement>
{
    public void Configure(EntityTypeBuilder<ResPartnerAgreement> builder)
    {
        builder.ToTable("res_partner_agreement", t =>
        {
            t.HasComment("Định nghĩa một số thỏa thuận với Cty bảo hiểm gốc");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id")
            .IsRequired();

        builder.Property(x => x.AgreementTermId)
            .HasColumnName("agreement_term_id")
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date");

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date")
            .IsRequired();

        // ✅ Audit columns: snake_case (AuditedEntity chỉ có CreationTime, CreatorId, LastModificationTime, LastModifierId)
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");

        // ✅ Foreign Keys với OnDelete(DeleteBehavior.Restrict)
        builder.HasOne(x => x.Partner)
            .WithMany(x => x.Agreements)
            .HasForeignKey(x => x.PartnerId)
            .HasConstraintName("fk_res_partner_agreement_res_partner_partner_id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.AgreementTerm)
            .WithMany()
            .HasForeignKey(x => x.AgreementTermId)
            .HasConstraintName("fk_res_partner_agreement_res_agreement_term_agreement_term_id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ✅ Indexes
        builder.HasIndex(e => e.PartnerId, "ix_res_partner_agreement_partner_id");
        builder.HasIndex(e => e.AgreementTermId, "ix_res_partner_agreement_agreement_term_id");
    }
}

