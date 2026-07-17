using System;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyRiskObjectConfiguration : IEntityTypeConfiguration<PolicyRiskObject>
{
    public void Configure(EntityTypeBuilder<PolicyRiskObject> builder)
    {
        builder.ToTable("policy_risk_object", t =>
        {
            t.HasComment("Bảng lưu đối tượng bảo hiểm của đơn bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id")
            .IsRequired()
            .HasComment("Tham chiếu đến đơn bảo hiểm gốc");

        builder.Property(x => x.PolicyVersionId)
            .HasColumnName("policy_version_id")
            .IsRequired()
            .HasComment("Tham chiếu đến phiên bản đơn bảo hiểm");

        builder.Property(x => x.ObjectTypeId)
            .HasColumnName("object_type_id")
            .IsRequired()
            .HasComment("Loại đối tượng bảo hiểm");

        builder.Property(x => x.RepName)
            .HasColumnName("rep_name")
            .HasMaxLength(250)
            .HasComment("Tên người đại diện");

        builder.Property(x => x.RepIdNo)
            .HasColumnName("rep_id_no")
            .HasMaxLength(15)
            .HasComment("Số CMND/CCCD người đại diện");

        builder.Property(x => x.RepPassport)
            .HasColumnName("rep_passport")
            .HasMaxLength(15)
            .HasComment("Hộ chiếu người đại diện");

        builder.Property(x => x.RepPhone)
            .HasColumnName("rep_phone")
            .HasMaxLength(15)
            .HasComment("Số điện thoại người đại diện");

        builder.Property(x => x.RepEmail)
            .HasColumnName("rep_email")
            .HasMaxLength(50)
            .HasComment("Email người đại diện");

        builder.Property(x => x.RepProvinceId)
            .HasColumnName("rep_province_id")
            .HasComment("Tỉnh/Thành phố người đại diện");

        builder.Property(x => x.RepWardId)
            .HasColumnName("rep_ward_id")
            .HasComment("Phường/Xã người đại diện");

        builder.Property(x => x.RepAddress)
            .HasColumnName("rep_address")
            .HasMaxLength(250)
            .HasComment("Địa chỉ người đại diện");

        builder.Property(x => x.RepFullAddress)
            .HasColumnName("rep_full_address")
            .HasMaxLength(250)
            .HasComment("Địa chỉ đầy đủ người đại diện");

        builder.Property(x => x.RiskObjectProvinceId)
            .HasColumnName("risk_object_province_id")
            .HasComment("Tỉnh/Thành phố đối tượng bảo hiểm");

        builder.Property(x => x.RiskObjectWardId)
            .HasColumnName("risk_object_ward_id")
            .HasComment("Phường/Xã đối tượng bảo hiểm");

        builder.Property(x => x.RiskObjectAddress)
            .HasColumnName("risk_object_address")
            .HasMaxLength(250)
            .HasComment("Địa chỉ đối tượng bảo hiểm");

        builder.Property(x => x.RiskObjectFullAddress)
            .HasColumnName("risk_object_full_address")
            .HasMaxLength(250)
            .HasComment("Địa chỉ đầy đủ đối tượng bảo hiểm");

        builder.Property(x => x.RiskObjectLat)
            .HasColumnName("risk_object_lat")
            .HasColumnType("FLOAT8")
            .HasComment("Vĩ độ đối tượng bảo hiểm");

        builder.Property(x => x.RiskObjectLong)
            .HasColumnName("risk_object_long")
            .HasColumnType("FLOAT8")
            .HasComment("Kinh độ đối tượng bảo hiểm");

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
        builder.HasIndex(e => e.PolicyId, "ix_policy_risk_object_policy_id");
        builder.HasIndex(e => e.PolicyVersionId, "ix_policy_risk_object_policy_version_id");
        builder.HasIndex(e => e.ObjectTypeId, "ix_policy_risk_object_object_type_id");

        // Foreign Key Relationships
        // Policy
        builder.HasOne(x => x.Policy)
            .WithMany(x => x.PolicyRiskObjects)
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_risk_object_policy_id");

        // PolicyVersion
        builder.HasOne(x => x.PolicyVersion)
            .WithMany(x => x.PolicyRiskObjects)
            .HasForeignKey(x => x.PolicyVersionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_risk_object_policy_version_id");

        // ResObjectType
        builder.HasOne(x => x.ObjectType)
            .WithMany(x => x.PolicyRiskObjects)
            .HasForeignKey(x => x.ObjectTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_risk_object_object_type_id");
    }
}
