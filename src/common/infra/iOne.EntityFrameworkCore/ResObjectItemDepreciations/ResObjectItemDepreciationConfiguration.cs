using System;
using iOne.ResObjectItemDepreciations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResObjectItemDepreciations;

public class ResObjectItemDepreciationConfiguration : IEntityTypeConfiguration<ResObjectItemDepreciation>
{
    public void Configure(EntityTypeBuilder<ResObjectItemDepreciation> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_object_item_depreciation", t =>
        {
            t.HasComment("Định nghĩa khấu hao tài sản");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        // Note: EF Core với Npgsql tự động map Guid sang UUID, không cần specify HasColumnType
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ObjectTypeItemId)
            .HasColumnName("object_type_item_id")
            .IsRequired();

        builder.Property(x => x.CarGroupId)
            .HasColumnName("car_group_id")
            .IsRequired();

        builder.Property(x => x.UsedTimeFrom)
            .HasColumnName("used_time_from")
            .IsRequired()
            .HasComment("Thời gian sử dụng (năm) từ");

        builder.Property(x => x.UsedTimeTo)
            .HasColumnName("used_time_to")
            .IsRequired()
            .HasComment("Thời gian sử dụng (năm) đến");

        builder.Property(x => x.DepreciationPercent)
            .HasColumnName("depreciation_percent")
            .IsRequired()
            .HasComment("Tỷ lệ khấu hao (%)");

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired()
            .HasComment("Ngày hiệu lực");

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date")
            .HasComment("Ngày hết hạn");

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResObjectItemDepreciationStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");

        // ✅ Foreign key relationships
        builder.HasOne(x => x.ObjectTypeItemNavigation)
            .WithMany(x => x.ObjectItemDepreciations)
            .HasForeignKey(x => x.ObjectTypeItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CarGroupNavigation)
            .WithMany(x => x.ObjectItemDepreciations)
            .HasForeignKey(x => x.CarGroupId)
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
        builder.HasIndex(e => e.ObjectTypeItemId, "ix_res_object_item_depreciation_object_type_item_id");
        builder.HasIndex(e => e.CarGroupId, "ix_res_object_item_depreciation_car_group_id");
        
        // ✅ Index: Composite index for efficient querying by effective dates
        builder.HasIndex(e => new { e.EffectDate, e.ExpireDate }, "ix_res_object_item_depreciation_dates");
    }
}
