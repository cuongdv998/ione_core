using System;
using iOne.ResUoms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResUoms;

public class ResUomConfiguration : IEntityTypeConfiguration<ResUom>
{
    public void Configure(EntityTypeBuilder<ResUom> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_uom", t =>
        {
            t.HasComment("Bảng đơn vị tính");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        // Note: EF Core với Npgsql tự động map Guid sang UUID, không cần specify HasColumnType
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResUomStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái: active (hiệu lực), deactive (hết hiệu lực)");

        builder.Property(x => x.Rounding)
            .HasColumnName("rounding")
            .HasColumnType("decimal(2,4)")
            .IsRequired()
            .HasDefaultValue(0.001m)
            .HasComment("Độ chính xác làm tròn");

        builder.Property(x => x.Factor)
            .HasColumnName("factor")
            .HasColumnType("decimal(6,6)")
            .HasDefaultValue(1m)
            .HasComment("Hệ số chuyển đổi");

        // ✅ Type: Enum to string conversion (lowercase)
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Ref" → "ref", "None" → "none"
                v => Enum.Parse<ResUomType>(v, true) // Parse "ref" → Ref, "none" → None
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue(ResUomType.None)
            .HasComment("Loại: ref (đơn vị cơ sở), none (đơn vị chuẩn)");

        // ✅ Foreign key relationship
        builder.HasOne(x => x.UomClass)
            .WithMany(x => x.Uoms)
            .HasForeignKey(x => x.ClassId)
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

        // ✅ Index: Foreign key index
        builder.HasIndex(e => e.ClassId, "ix_res_uom_class_id");
    }
}
