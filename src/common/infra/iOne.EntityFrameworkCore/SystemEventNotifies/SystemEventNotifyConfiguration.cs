using System;
using iOne.SystemEventNotifies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.SystemEventNotifies;

public class SystemEventNotifyConfiguration : IEntityTypeConfiguration<SystemEventNotify>
{
    public void Configure(EntityTypeBuilder<SystemEventNotify> builder)
    {
        builder.ToTable("system_event_notify", t =>
        {
            t.HasComment("Lưu các bản tin cần gửi của hệ thống (chưa gửi, hoặc gửi fail)");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.EventCode)
            .HasColumnName("event_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AppChannelId)
            .HasColumnName("app_channel_id")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tiêu đề bản tin, đã được thay thế tham số");

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(500)
            .IsRequired()
            .HasComment("Nội dung bản tin, đã được thay thế tham số");

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasMaxLength(500)
            .HasComment("Thông tin mô tả hành xử cho các hệ thống nhận tin, đã được thay thế các tham số");

        builder.Property(x => x.RecipientType)
            .HasColumnName("recipient_type")
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Loại đối tượng nhận tin: cus (khách hàng), emp (nhân viên)");

        builder.Property(x => x.RecipientId)
            .HasColumnName("recipient_id")
            .IsRequired()
            .HasComment("Id tương ứng với nhân viên hoặc khách hàng");

        builder.Property(x => x.Recipient)
            .HasColumnName("recipient")
            .HasMaxLength(500)
            .HasComment("Địa chỉ nhận tin");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => string.IsNullOrEmpty(v) ? SystemEventNotifyStatus.Pending : Enum.Parse<SystemEventNotifyStatus>(v, true))
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái: pending (chờ gửi), sent (đã gửi), fail (lỗi), read (đã đọc), deactive (đã xóa)");

        builder.Property(x => x.ScheduleAt)
            .HasColumnName("schedule_at")
            .IsRequired()
            .HasComment("Thời gian dự kiến gửi");

        builder.Property(x => x.SentAt)
            .HasColumnName("sent_at")
            .HasComment("Thời điểm gửi");

        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at")
            .HasComment("Thời điểm đọc");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000)
            .HasComment("Bản tin lỗi nếu có lỗi xảy ra");

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(e => e.EventCode, "ix_system_event_notify_event_code");
        builder.HasIndex(e => e.Status, "ix_system_event_notify_status");
        builder.HasIndex(e => e.ScheduleAt, "ix_system_event_notify_schedule_at");
    }
}
