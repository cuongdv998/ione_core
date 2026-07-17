using iOne.ApiKeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ApiKeys;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Prefix).HasColumnName("prefix").HasMaxLength(32).IsRequired();
        builder.Property(x => x.KeyHash).HasColumnName("key_hash").HasMaxLength(128).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.Prefix }, "ix_api_keys_tenant_id_prefix")
            .IsUnique();
    }
}
