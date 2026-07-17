using iOne.ResPartnerMessageLoggings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResPartnerMessageLoggings;

public class ResPartnerMessageLoggingConfiguration : IEntityTypeConfiguration<ResPartnerMessageLogging>
{
    public void Configure(EntityTypeBuilder<ResPartnerMessageLogging> builder)
    {
        builder.ToTable("res_partner_message_logging", t =>
        {
            t.HasComment("Lưu log request/response khi gọi API đối tác bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PartnerCode)
            .HasColumnName("partner_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id");

        builder.Property(x => x.ApiUrl)
            .HasColumnName("api_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.HttpMethod)
            .HasColumnName("http_method")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.RequestBody)
            .HasColumnName("request_body")
            .HasColumnType("text");

        builder.Property(x => x.ResponseBody)
            .HasColumnName("response_body")
            .HasColumnType("text");

        builder.Property(x => x.HttpStatusCode)
            .HasColumnName("http_status_code");

        builder.Property(x => x.IsSuccess)
            .HasColumnName("is_success")
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(x => x.DurationMs)
            .HasColumnName("duration_ms");

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(e => e.PartnerCode, "ix_res_partner_message_logging_partner_code");
        builder.HasIndex(e => e.PolicyId, "ix_res_partner_message_logging_policy_id");
    }
}
