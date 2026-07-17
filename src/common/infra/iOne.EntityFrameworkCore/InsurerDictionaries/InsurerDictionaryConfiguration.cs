using System;
using iOne.InsurerDictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.InsurerDictionaries;

public class InsurerDictionaryConfiguration : IEntityTypeConfiguration<InsurerDictionary>
{
    public void Configure(EntityTypeBuilder<InsurerDictionary> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("insurer_dictionary", t =>
        {
            t.HasComment("Bảng mapping các dữ liệu master data với công ty bảo hiểm gốc");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.BusinessName)
            .HasColumnName("business_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.InsurerId)
            .HasColumnName("insurer_id")
            .IsRequired();

        builder.Property(x => x.OwnCode)
            .HasColumnName("own_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.InsurerCode)
            .HasColumnName("insurer_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ExtraData)
            .HasColumnName("extra_data")
            .HasColumnType("text");

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<InsurerDictionaryStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        // Audit columns: snake_case (PostgreSQL convention)
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(x => new { x.BusinessName, x.InsurerId, x.OwnCode })
            .HasDatabaseName("ix_insurer_dictionary_business_insurer_own");
    }
}
