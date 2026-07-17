using System;
using iOne.ProProductDistributions;
using iOne.ProProducts;
using iOne.ResChannels;
using iOne.ResAppChannels;
using iOne.HrEmployeeRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProductDistributions;

public class ProProductDistributionConfiguration : IEntityTypeConfiguration<ProProductDistribution>
{
    public void Configure(EntityTypeBuilder<ProProductDistribution> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product_distribution", t =>
        {
            t.HasComment("Thiết lập quyền phân phối sản phẩm");
        });

        builder.ConfigureByConvention();

        // ✅ Primary key: snake_case
        builder.HasKey(e => e.Id).HasName("pk_pro_product_distribution");

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.ChannelId)
            .HasColumnName("channel_id")
            .HasComment("Kênh phân phối");

        builder.Property(x => x.AppChannelId)
            .HasColumnName("app_channel_id")
            .HasComment("Kênh ứng dụng (app mobile, web ....)");

        builder.Property(x => x.EmployeeRoleId)
            .HasColumnName("employee_role_id")
            .HasComment("Nhân viên bán hàng");

        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ProProductDistributionStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động");

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

        // ✅ Indexes: snake_case with ix_ prefix
        builder.HasIndex(e => e.ProductId, "ix_pro_product_distribution_product_id");

        // Configure relationships
        // Many-to-one: ProProductDistribution -> ProProduct
        builder.HasOne(e => e.Product)
            .WithMany(e => e.ProductDistributions)
            .HasForeignKey(e => e.ProductId)
            .HasConstraintName("fk_pro_product_distribution_pro_product_product_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProductDistribution -> ResChannel
        builder.HasOne(e => e.Channel)
            .WithMany()
            .HasForeignKey(e => e.ChannelId)
            .HasConstraintName("fk_pro_product_distribution_res_channel_channel_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProductDistribution -> ResAppChannel
        builder.HasOne(e => e.AppChannel)
            .WithMany()
            .HasForeignKey(e => e.AppChannelId)
            .HasConstraintName("fk_pro_product_distribution_res_app_channel_app_channel_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProductDistribution -> HrEmployeeRole
        builder.HasOne(e => e.EmployeeRole)
            .WithMany()
            .HasForeignKey(e => e.EmployeeRoleId)
            .HasConstraintName("fk_pro_product_distribution_hr_employee_role_employee_role_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
