using System;
using iOne.ProProductPlanDefinitions;
using iOne.ProProducts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProductPlanDefinitions;

public class ProProductPlanDefinitionConfiguration : IEntityTypeConfiguration<ProProductPlanDefinition>
{
    public void Configure(EntityTypeBuilder<ProProductPlanDefinition> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_plan_definition", t =>
        {
            t.HasComment("Bảng định nghĩa các gói của sản phẩm gốc");
        });

        builder.ConfigureByConvention();

        // ✅ Primary key: snake_case
        builder.HasKey(e => e.Id).HasName("pk_pro_product_plan_definition");

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.PlanCode)
            .HasColumnName("plan_code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Nếu có khai mã PLAN_OPTIONAL thì sản phẩm này cho phép ngoài gói cố định, có thể tùy chỉnh khi cấp đơn");

        builder.Property(x => x.PlanName)
            .HasColumnName("plan_name")
            .HasMaxLength(250)
            .IsRequired();

        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ProProductPlanDefinitionStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");

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

        // ✅ Index on ProductId
        builder.HasIndex(e => e.ProductId, "ix_pro_product_plan_definition_product_id");

        // Configure relationships
        // Many-to-one: ProProductPlanDefinition -> ProProduct
        builder.HasOne(e => e.Product)
            .WithMany(e => e.ProductPlanDefinitions)
            .HasForeignKey(e => e.ProductId)
            .HasConstraintName("fk_pro_product_plan_definition_pro_product_product_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
