using System;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyRiskMotorConfiguration : IEntityTypeConfiguration<PolicyRiskMotor>
{
    public void Configure(EntityTypeBuilder<PolicyRiskMotor> builder)
    {
        builder.ToTable("policy_risk_motor", t =>
        {
            t.HasComment("Bảng lưu thông tin rủi ro xe cơ giới của đối tượng bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyRiskObjectId)
            .HasColumnName("policy_risk_object_id")
            .HasComment("Tham chiếu đến đối tượng bảo hiểm");

        builder.Property(x => x.RiskObjectValue)
            .HasColumnName("risk_object_value")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Giá trị đối tượng bảo hiểm");

        builder.Property(x => x.MotorClassCode)
            .HasColumnName("motor_class_code")
            .HasMaxLength(50)
            .HasComment("Mã nhóm xe cơ giới");

        builder.Property(x => x.CarLineCode)
            .HasColumnName("car_line_code")
            .HasMaxLength(50)
            .HasComment("Dòng xe");

        builder.Property(x => x.CarGroupCode)
            .HasColumnName("car_group_code")
            .HasMaxLength(50)
            .HasComment("Nhóm xe");

        builder.Property(x => x.CarTypeCode)
            .HasColumnName("car_type_code")
            .HasMaxLength(50)
            .HasComment("Loại xe");

        builder.Property(x => x.CarBrandCode)
            .HasColumnName("car_brand_code")
            .HasMaxLength(50)
            .HasComment("Hãng xe");

        builder.Property(x => x.CarModelCode)
            .HasColumnName("car_model_code")
            .HasMaxLength(50)
            .HasComment("Model xe");

        builder.Property(x => x.CarCategoryCode)
            .HasColumnName("car_category_code")
            .HasMaxLength(50)
            .HasComment("Phân loại xe");

        builder.Property(x => x.CarUsage)
            .HasColumnName("car_usage")
            .HasMaxLength(15)
            .HasComment("Mục đích sử dụng xe");

        builder.Property(x => x.CarOld)
            .HasColumnName("car_old")
            .HasColumnType("NUMERIC(6,3)")
            .HasComment("Số năm sử dụng xe");

        builder.Property(x => x.CarProductionYear)
            .HasColumnName("car_production_year")
            .HasComment("Năm sản xuất xe");

        builder.Property(x => x.CarPlate)
            .HasColumnName("car_plate")
            .HasMaxLength(15)
            .HasComment("Biển số xe");

        builder.Property(x => x.CarPlateType)
            .HasColumnName("car_plate_type")
            .HasMaxLength(50)
            .HasComment("Loại biển số xe");

        builder.Property(x => x.CarPlateClear)
            .HasColumnName("car_plate_clear")
            .HasMaxLength(15)
            .HasComment("Biển số xe (chuẩn hóa, không dấu/ký tự đặc biệt)");

        builder.Property(x => x.CarSeatNumber)
            .HasColumnName("car_seat_number")
            .HasColumnType("NUMERIC(2)")
            .HasComment("Số chỗ ngồi");

        builder.Property(x => x.CarVin)
            .HasColumnName("car_vin")
            .HasMaxLength(25)
            .HasComment("Số khung xe (VIN)");

        builder.Property(x => x.CarEngineNumber)
            .HasColumnName("car_engine_number")
            .HasMaxLength(25)
            .HasComment("Số máy");

        builder.Property(x => x.CarPayloadCapacity)
            .HasColumnName("car_payload_capacity")
            .HasColumnType("NUMERIC(6,3)")
            .HasComment("Tải trọng xe");

        builder.Property(x => x.CarColor)
            .HasColumnName("car_color")
            .HasMaxLength(15)
            .HasComment("Màu xe");

        builder.Property(x => x.CarOrigin)
            .HasColumnName("car_origin")
            .HasMaxLength(50)
            .HasComment("Nguồn gốc xe");

        builder.Property(x => x.CarNew)
            .HasColumnName("car_new")
            .HasMaxLength(1)
            .HasComment("Xe mới (Y/N)");

        // Audit columns
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // Indexes
        builder.HasIndex(e => e.PolicyRiskObjectId, "ix_policy_risk_motor_policy_risk_object_id");

        // Foreign Key Relationships
        // PolicyRiskObject
        builder.HasOne(x => x.PolicyRiskObject)
            .WithMany(x => x.PolicyRiskMotors)
            .HasForeignKey(x => x.PolicyRiskObjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_risk_motor_policy_risk_object_id");
    }
}
