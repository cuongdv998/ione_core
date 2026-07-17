using System;
using iOne.ResCarCategories;
using iOne.ResCarBrands;
using iOne.ResCarModels;
using iOne.ResCarLines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResCarCategories;

public class ResCarCategoryConfiguration : IEntityTypeConfiguration<ResCarCategory>
{
    public void Configure(EntityTypeBuilder<ResCarCategory> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_car_category", t =>
        {
            t.HasComment("Định nghĩa Phiên bản xe");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        // Note: EF Core với Npgsql tự động map Guid sang UUID, không cần specify HasColumnType
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.CarBrandId)
            .HasColumnName("car_brand_id")
            .IsRequired();

        builder.Property(x => x.CarModelId)
            .HasColumnName("car_model_id");

        builder.Property(x => x.MotorClassId)
            .HasColumnName("motor_class_id")
            .IsRequired();

        builder.Property(x => x.CarLineId)
            .HasColumnName("car_line_id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.SeatNumber)
            .HasColumnName("seat_number")
            .HasColumnType("numeric(2,0)")
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
                v => Enum.Parse<ResCarCategoryStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");
        
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

        // ✅ Foreign Key relationships
        builder.HasOne(x => x.CarBrand)
            .WithMany(x => x.CarCategories)
            .HasForeignKey(x => x.CarBrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CarModel)
            .WithMany(x => x.CarCategories)
            .HasForeignKey(x => x.CarModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

