using System;
using iOne.ResAppChannels;
using iOne.ResEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResEvents;

public class ResEventNotifyTemplateConfiguration : IEntityTypeConfiguration<ResEventNotifyTemplate>
{
    public void Configure(EntityTypeBuilder<ResEventNotifyTemplate> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_event_notify_template", t =>
        {
            t.HasComment("Template cảnh báo tương ứng với sự kiện");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AppChannelId)
            .HasColumnName("app_channel_id")
            .IsRequired();

        builder.Property(x => x.RetryNumber)
            .HasColumnName("retry_number")
            .HasColumnType("numeric(2)")
            .IsRequired()
            .HasComment("Số lần retry, mặc định là 0");

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tiêu đề bản tin, có thể có tham số truyền vào, nếu có thêm số thì để dạng ${param_name}");

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(500)
            .IsRequired()
            .HasComment("Nội dung của bản tin, có thể có tham số truyền vào, nếu có tham số truyền vào thì tham số có dạng ${param_name}");

        builder.Property(x => x.Data)
            .HasColumnName("data")
            .HasMaxLength(1000)
            .HasComment("Data có dạng json:\n{\n\"screen\": \"invoice_detail\",\n\"invoice_id\": \"INV20250911001\",\n....\n}");

        // ✅ Audit columns: snake_case
        // Note: FullAuditedEntity does not have ExtraProperties, ConcurrencyStamp, or TenantId
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");

        // ✅ Relationships
        // Many-to-one: ResEventNotifyTemplate -> ResEvent
        builder.HasOne(e => e.Event)
            .WithMany(e => e.NotifyTemplates)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_res_event_notify_template_event_id");

        // Many-to-one: ResEventNotifyTemplate -> ResAppChannel
        builder.HasOne(e => e.AppChannel)
            .WithMany()
            .HasForeignKey(e => e.AppChannelId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_event_notify_template_app_channel_id");

        // ✅ Unique constraint: EventId + AppChannelId
        builder.HasIndex(e => new { e.EventId, e.AppChannelId }, "uq_res_event_notify_template_event_channel")
            .IsUnique();

        // ✅ Index: EventId for faster queries
        builder.HasIndex(e => e.EventId, "ix_res_event_notify_template_event_id");
    }
}

