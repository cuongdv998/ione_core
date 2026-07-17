using System;
using iOne.ResObjectTypeItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResObjectTypeItems;

public class ResObjectTypeItemConfiguration : IEntityTypeConfiguration<ResObjectTypeItem>
{
    public void Configure(EntityTypeBuilder<ResObjectTypeItem> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_object_type_item", t =>
        {
            t.HasComment("Định nghĩa hạng mục đối tượng bảo hiểm (dùng cho giải quyết bồi thường)");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        // Note: EF Core với Npgsql tự động map Guid sang UUID, không cần specify HasColumnType
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ObjectTypeId)
            .HasColumnName("object_type_id")
            .IsRequired();

        builder.Property(x => x.ObjectItemType)
            .HasColumnName("object_item_type");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.UomId)
            .HasColumnName("uom_id")
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
                v => Enum.Parse<ResObjectTypeItemStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");

        // ✅ Foreign key relationships
        builder.HasOne(x => x.ObjectTypeNavigation)
            .WithMany(x => x.ObjectTypeItems)
            .HasForeignKey(x => x.ObjectTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ObjectItemTypeNavigation)
            .WithMany(x => x.ObjectTypeItems)
            .HasForeignKey(x => x.ObjectItemType)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Uom)
            .WithMany(x => x.ObjectTypeItems)
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ ExtraProperties: snake_case
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        // ✅ Audit columns: snake_case
        // Note: EF Core với Npgsql tự động map Guid sang UUID cho các audit ID columns
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // ✅ Index: Foreign key indexes
        builder.HasIndex(e => e.ObjectTypeId, "ix_res_object_type_item_object_type_id");
        builder.HasIndex(e => e.ObjectItemType, "ix_res_object_type_item_object_item_type");
        builder.HasIndex(e => e.UomId, "ix_res_object_type_item_uom_id");
    }
}
