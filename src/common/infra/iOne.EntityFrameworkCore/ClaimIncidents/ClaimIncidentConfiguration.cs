using System;
using iOne.ClaimIncidents;
using iOne.Claims;
using iOne.ResIncidentCauses;
using iOne.ResIncidentLevels;
using iOne.ResObjectTypes;
using iOne.ResPartners;
using iOne.ResProvinces;
using iOne.ResWards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimIncidents;

public class ClaimIncidentConfiguration : IEntityTypeConfiguration<ClaimIncident>
{
    public void Configure(EntityTypeBuilder<ClaimIncident> builder)
    {
        builder.ToTable("claim_incident", t =>
        {
            t.HasComment("Đối tượng tổn thất");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        // Foreign Keys
        builder.Property(x => x.IncidentId)
            .HasColumnName("incident_id")
            .IsRequired()
            .HasComment("Id của claim");

        builder.Property(x => x.ObjectTypeId)
            .HasColumnName("object_type_id")
            .IsRequired()
            .HasComment("Loại đối tượng tổn thất");

        builder.Property(x => x.AssessmentPartnerId)
            .HasColumnName("assessment_partner_id")
            .HasComment("Bên thực hiện giám định");

        builder.Property(x => x.IncidentLevelId)
            .HasColumnName("incident_level_id")
            .HasComment("Mức độ tổn thất");

        builder.Property(x => x.IncidentProvinceId)
            .HasColumnName("incident_province_id")
            .HasComment("Tỉnh nơi xảy ra tổn thất");

        builder.Property(x => x.IncidentWardId)
            .HasColumnName("incident_ward_id")
            .HasComment("Phường/xã nơi xảy ra tổn thất");

        builder.Property(x => x.IncidentCauseId)
            .HasColumnName("incident_cause_id")
            .IsRequired()
            .HasComment("Nguyên nhân xảy ra tổn thất");

        // Basic Info
        builder.Property(x => x.OnLocation)
            .HasColumnName("on_location")
            .HasMaxLength(1)
            .IsRequired()
            .HasComment("Còn ở hiện trường hay không: Y (có), N (không)");

        builder.Property(x => x.AssessmentDate)
            .HasColumnName("assessment_date")
            .HasComment("Ngày dự kiến giám định chi tiết");

        builder.Property(x => x.IncidentDate)
            .HasColumnName("incident_date")
            .HasComment("Thời điểm tổn thất");

        builder.Property(x => x.IncidentAddress)
            .HasColumnName("incident_address")
            .HasMaxLength(50)
            .HasComment("Địa chỉ nơi xảy ra tổn thất");

        builder.Property(x => x.IncidentFullAddress)
            .HasColumnName("incident_full_address")
            .HasMaxLength(250)
            .HasComment("Địa chỉ đầy đủ nơi xảy ra tổn thất");

        builder.Property(x => x.IncidentLat)
            .HasColumnName("incident_lat")
            .HasColumnType("FLOAT8")
            .HasComment("Vĩ độ nơi xảy ra tổn thất");

        builder.Property(x => x.IncidentLong)
            .HasColumnName("incident_long")
            .HasColumnType("FLOAT8")
            .HasComment("Kinh độ nơi xảy ra tổn thất");

        builder.Property(x => x.IncidentDescription)
            .HasColumnName("incident_description")
            .HasMaxLength(500)
            .HasComment("Mô tả sự việc dẫn đến tổn thất");

        builder.Property(x => x.IncidentResult)
            .HasColumnName("incident_result")
            .HasMaxLength(500)
            .HasComment("Hậu quả của tổn thất");

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500)
            .HasComment("Ghi chú khác");

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
        builder.HasIndex(e => e.IncidentId, "ix_claim_incident_incident_id");
        builder.HasIndex(e => e.ObjectTypeId, "ix_claim_incident_object_type_id");
        builder.HasIndex(e => e.IncidentCauseId, "ix_claim_incident_incident_cause_id");

        // Foreign Key Relationships
        // Claim (Incident)
        //builder.HasOne(x => x.Incident)
        //    .WithMany()
        //    .HasForeignKey(x => x.IncidentId)
        //    .OnDelete(DeleteBehavior.Restrict)
        //    .HasConstraintName("FK_CLAIMINC_REFERENCE_CLAIM");

        //// ObjectType (ResObjectType)
        //builder.HasOne(x => x.ObjectType)
        //    .WithMany()
        //    .HasForeignKey(x => x.ObjectTypeId)
        //    .OnDelete(DeleteBehavior.Restrict);

        //// AssessmentPartner (ResPartner)
        //builder.HasOne(x => x.AssessmentPartner)
        //    .WithMany()
        //    .HasForeignKey(x => x.AssessmentPartnerId)
        //    .OnDelete(DeleteBehavior.Restrict);

        //// IncidentLevel (ResIncidentLevel)
        //builder.HasOne(x => x.IncidentLevel)
        //    .WithMany()
        //    .HasForeignKey(x => x.IncidentLevelId)
        //    .OnDelete(DeleteBehavior.Restrict)
        //    .HasConstraintName("FK_CLAIMINC_REFERENCE_RESINCID_LEVEL");

        //// IncidentProvince (ResProvince)
        //builder.HasOne(x => x.IncidentProvince)
        //    .WithMany()
        //    .HasForeignKey(x => x.IncidentProvinceId)
        //    .OnDelete(DeleteBehavior.Restrict);

        //// IncidentWard (ResWard)
        //builder.HasOne(x => x.IncidentWard)
        //    .WithMany()
        //    .HasForeignKey(x => x.IncidentWardId)
        //    .OnDelete(DeleteBehavior.Restrict);

        //// IncidentCause (ResIncidentCause)
        //builder.HasOne(x => x.IncidentCause)
        //    .WithMany()
        //    .HasForeignKey(x => x.IncidentCauseId)
        //    .OnDelete(DeleteBehavior.Restrict)
        //    .HasConstraintName("FK_CLAIMINC_REFERENCE_RESINCID_CAUSE");
    }
}
