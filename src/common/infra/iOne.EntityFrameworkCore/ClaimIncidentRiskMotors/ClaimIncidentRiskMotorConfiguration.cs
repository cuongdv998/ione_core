using System;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimIncidentRiskMotors;

public class ClaimIncidentRiskMotorConfiguration : IEntityTypeConfiguration<ClaimIncidentRiskMotor>
{
    public void Configure(EntityTypeBuilder<ClaimIncidentRiskMotor> builder)
    {
        builder.ToTable("claim_incident_risk_motor", t =>
        {
            t.HasComment("Bảng lưu thông tin đối tượng bảo hiểm (Oto, Xe máy)");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        // Foreign Keys
        builder.Property(x => x.IncidentObjectId)
            .HasColumnName("incident_object_id")
            .HasComment("Id của đối tượng tổn thất");

        // Car Info
        builder.Property(x => x.CarPlate)
            .HasColumnName("car_plate")
            .HasMaxLength(15)
            .HasComment("Biển số xe");

        builder.Property(x => x.Vin)
            .HasColumnName("vin")
            .HasMaxLength(25)
            .HasComment("Số khung");

        builder.Property(x => x.EngineNumber)
            .HasColumnName("engine_number")
            .HasMaxLength(25)
            .HasComment("Số máy");

        builder.Property(x => x.PersonsOnCar)
            .HasColumnName("persons_on_car")
            .HasColumnType("NUMERIC(3,0)")
            .HasComment("Số người trên xe khi xảy ra tổn thất");

        // Driver Info
        builder.Property(x => x.DriverName)
            .HasColumnName("driver_name")
            .HasMaxLength(250)
            .HasComment("Tên lái xe");

        builder.Property(x => x.DriverPhone)
            .HasColumnName("driver_phone")
            .HasMaxLength(15)
            .HasComment("Số điện thoại lái xe");

        builder.Property(x => x.DriverIdNo)
            .HasColumnName("driver_id_no")
            .HasMaxLength(25)
            .HasComment("Số CCCD của lái xe");

        builder.Property(x => x.DriverLicenseNo)
            .HasColumnName("driver_license_no")
            .HasMaxLength(50)
            .HasComment("Số giấy phép lái xe");

        builder.Property(x => x.DriverLicenseLevel)
            .HasColumnName("driver_license_level")
            .HasMaxLength(5)
            .HasComment("Hạng giấy phép lái xe của người lái xe: cấu hình trong bảng AdminConfig (code: DERIVER_LICENSE_LEVEL)");

        builder.Property(x => x.DriverLicenseEffectDate)
            .HasColumnName("driver_license_effect_date")
            .HasComment("Ngày hiệu lực giấy phép lái xe");

        builder.Property(x => x.DriverLicenseExpireDate)
            .HasColumnName("driver_license_expire_date")
            .HasComment("Ngày hết hạn giấy phép lái xe");

        builder.Property(x => x.DriverSex)
            .HasColumnName("driver_sex")
            .HasMaxLength(15)
            .HasComment("Giới tính của lái xe: M (nam), F (nữ)");

        // Car Registry Info
        builder.Property(x => x.CarRegistryNo)
            .HasColumnName("car_registry_no")
            .HasMaxLength(50)
            .HasComment("Số giấy đăng ký xe");

        builder.Property(x => x.CarRegistryEffectDate)
            .HasColumnName("car_registry_effect_date")
            .HasComment("Ngày hiệu lực giấy đăng ký xe");

        builder.Property(x => x.CarRegistryExpireDate)
            .HasColumnName("car_registry_expire_date")
            .HasComment("Ngày hết hạn giấy đăng ký xe");

        // Audit columns: UPPERCASE (matching SQL schema)
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        // Indexes
        builder.HasIndex(e => e.IncidentObjectId, "ix_claim_incident_risk_motor_incident_object_id");

        // Foreign Key Relationships
        // ClaimIncident (IncidentObject)
        builder.HasOne(x => x.IncidentObject)
            .WithMany()
            .HasForeignKey(x => x.IncidentObjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CLAIMINC_REFERENCE_CLAIMINC");

    }
}
