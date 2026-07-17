using System;
using iOne.ResBusinessAuthorities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResBusinessAuthorities;

public class ResBusinessAuthorityConfiguration : IEntityTypeConfiguration<ResBusinessAuthority>
{
    public void Configure(EntityTypeBuilder<ResBusinessAuthority> builder)
    {
        builder.ToTable("res_business_authority", t =>
        {
            t.HasComment("Định nghĩa các thẩm quyền thực hiện các step của flow");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.BusinessCode)
            .HasColumnName("business_code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Định nghĩa trong bảng AdminConfig với code = BUSINESS_CODE");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResBusinessAuthorityStatus>(v, true))
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        // TenantId: only map if entity implements IMultiTenant (ConfigureByConvention handles it)

        builder.HasIndex(e => e.Code, "ix_res_business_authority_code")
            .IsUnique();
    }
}
