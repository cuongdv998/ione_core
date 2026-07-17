using System;
using iOne.ProRules;
using iOne.ProRuleTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProRules;

public class ProRuleConfiguration : IEntityTypeConfiguration<ProRule>
{
    public void Configure(EntityTypeBuilder<ProRule> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_rule", t =>
        {
            t.HasComment("Bảng định nghĩa các quy tắc đối với sản phẩm");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        // Note: EF Core với Npgsql tự động map Guid sang UUID, không cần specify HasColumnType
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ApplyTo)
            .HasColumnName("apply_to")
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Phạm vi áp dụng: product (mức sản phẩm), coverage (mức phạm vi), level (mức hạn mức của phạm vi)");

        builder.Property(x => x.ApplyToId)
            .HasColumnName("apply_to_id")
            .IsRequired();

        builder.Property(x => x.RuleTypeId)
            .HasColumnName("rule_type_id")
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.RuleScript)
            .HasColumnName("rule_script")
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasDefaultValue(1)
            .IsRequired()
            .HasComment("Thứ tự ưu tiên thực thi, số nhỏ ưu tiên trước");

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ProRuleStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired();

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date");

        // ✅ Foreign key relationship with ProRuleType
        builder.HasOne(x => x.RuleType)
            .WithMany()
            .HasForeignKey(x => x.RuleTypeId)
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
    }
}
