using System;
using iOne.ResPartnerTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResPartnerTypes;

public class ResPartnerTypeConfiguration : IEntityTypeConfiguration<ResPartnerType>
{
    public void Configure(EntityTypeBuilder<ResPartnerType> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_partner_type", t =>
        {
            t.HasComment("Loại đối tác");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25);
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250);
        
        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResPartnerTypeStatus>(v, true)
            )
            .HasMaxLength(10)
            .HasComment("Trạng thái: - active: Hoạt động - deactive: Không hoạt động");
        
        // ✅ ExtraProperties: snake_case
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        
        // ✅ Audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
    }
}

