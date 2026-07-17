using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResPartners;

public class PartnerConsentConfiguration : IEntityTypeConfiguration<PartnerConsent>
{
    public void Configure(EntityTypeBuilder<PartnerConsent> builder)
    {
        builder.ToTable("partner_consent", t =>
        {
            t.HasComment("Bảng lưu lịch sử đồng ý điều khoản của đối tác");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(x => x.PolicyId).HasColumnName("policy_id");
        builder.Property(x => x.ConsentType)
            .HasColumnName("consent_type")
            .HasMaxLength(25)
            .IsRequired()
            .HasComment("Loại điều khoản:\nDATA_SHARING: chia sẻ thông tin cá nhân\nCONTRACT_TERM: điều khoản hợp đồng");
        builder.Property(x => x.ConsentContent).HasColumnName("consent_content").HasColumnType("text");
        builder.Property(x => x.TermVersion).HasColumnName("term_version").HasMaxLength(50);
        builder.Property(x => x.IsAccepted).HasColumnName("is_accepted").IsRequired();
        builder.Property(x => x.AcceptedAt).HasColumnName("accepted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
        builder.Property(x => x.PartnerContractId).HasColumnName("partner_contract_id");

        // ABP audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
    }
}
