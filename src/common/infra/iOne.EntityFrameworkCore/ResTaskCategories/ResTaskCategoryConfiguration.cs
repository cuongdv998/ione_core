using System;
using iOne.ResTaskCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResTaskCategories;

public class ResTaskCategoryConfiguration : IEntityTypeConfiguration<ResTaskCategory>
{
    public void Configure(EntityTypeBuilder<ResTaskCategory> builder)
    {
        builder.ToTable("res_task_category", t =>
        {
            t.HasComment("Danh sách công việc thực hiện");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.BusinessType)
            .HasColumnName("business_type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResTaskCategoryBusinessType>(v, true))
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại nghiệp vụ: policy (cấp đơn), claim (bồi thường), common (chung)");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResTaskCategoryStatus>(v, true))
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái: active - Hoạt động; deactive - Không hoạt động");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(e => e.Code, "ix_res_task_category_code")
            .IsUnique();
    }
}
