using System;
using iOne.ResIncidentCauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResIncidentCauses;

public class ResIncidentCauseConfiguration : IEntityTypeConfiguration<ResIncidentCause>
{
    public void Configure(EntityTypeBuilder<ResIncidentCause> builder)
    {
        builder.ToTable("res_incident_cause", t =>
        {
            t.HasComment("Định nghĩa các nguyên nhân tổn thất");
        });

        builder.ConfigureByConvention();

        // Column names: UPPERCASE (matching SQL schema)
        // Note: For PostgreSQL, Guid maps to UUID. If using other DB, may need VARCHAR(36)
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired()
            .HasComment("Mã nguyên nhân tổn thất (unique, uppercase)");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tên nguyên nhân tổn thất");

        // Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResIncidentCauseStatus>(v, true)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        // Audit columns: UPPERCASE (matching SQL schema)
        // Note: SQL schema shows DATE for timestamps and VARCHAR(50) for CreatorId/LastModifierId
        // For PostgreSQL, we'll use timestamp and UUID. For other DBs, may need conversion
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
