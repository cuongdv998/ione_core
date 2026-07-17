using System;
using iOne.ResUserDevices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResUserDevices;

public class ResUserDeviceConfiguration : IEntityTypeConfiguration<ResUserDevice>
{
    public void Configure(EntityTypeBuilder<ResUserDevice> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_user_device", t =>
        {
            t.HasComment("Bảng lưu trữ định danh thiết bị mobile của user để gửi notify đến app");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserName)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DeviceUid)
            .HasColumnName("device_uid")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.DeviceToken)
            .HasColumnName("device_token")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.AppChannelCode)
            .HasColumnName("app_channel_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired();

        builder.Property(x => x.ExpirDate)
            .HasColumnName("expir_date");

        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => string.IsNullOrEmpty(v) ? ResUserDeviceStatus.Active : Enum.Parse<ResUserDeviceStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        builder.Property(x => x.Os)
            .HasColumnName("os")
            .HasMaxLength(50);

        builder.Property(x => x.DeviceName)
            .HasColumnName("device_name")
            .HasMaxLength(250);

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

        // ✅ Unique index: (username, device_uid, app_channel_code)
        builder.HasIndex(e => new { e.UserName, e.DeviceUid, e.AppChannelCode }, "uq_res_user_device_username_device_uid_app_channel_code")
            .IsUnique();
    }
}
