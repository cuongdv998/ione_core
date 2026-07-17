using System;
using iOne.ResDocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResDocumentTypes;

public class ResDocumentTypeConfiguration : IEntityTypeConfiguration<ResDocumentType>
{
    public void Configure(EntityTypeBuilder<ResDocumentType> builder)
    {
        builder.ToTable("res_document_type", t =>
        {
            t.HasComment("Loại chứng từ");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResDocumentTypeStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive (case-insensitive)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động");

        builder.Property(x => x.Bucket)
            .HasColumnName("bucket")
            .HasMaxLength(50)
            .HasComment("Thông tin bucket sẽ lưu tài liệu");

        builder.Property(x => x.DocumentGroupCode)
            .HasColumnName("document_group_code")
            .HasMaxLength(50)
            .HasComment("Mã nhóm tài liệu (tùy chọn)");

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

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

