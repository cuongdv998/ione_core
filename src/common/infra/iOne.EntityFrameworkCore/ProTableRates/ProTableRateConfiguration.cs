using System;
using iOne.ProLineOfBusinesses;
using iOne.ProTableRates;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProTableRates;

public class ProTableRateConfiguration : IEntityTypeConfiguration<ProTableRate>
{
    public void Configure(EntityTypeBuilder<ProTableRate> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL standard)
        builder.ToTable("pro_table_rate", t =>
        {
            t.HasComment("Bảng định nghĩa Bảng phí");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL standard)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LobId)
            .HasColumnName("lob_id")
            .IsRequired();

        builder.Property(x => x.InsurerId)
            .HasColumnName("insurer_id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);
        
        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ProTableRateStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");
        
        // ✅ ExtraProperties: snake_case (ABP convention)
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        
        // ✅ Audit columns: snake_case (PostgreSQL standard)
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        // Note: deletion_time, deleter_id, is_deleted, concurrency_stamp are ABP audit fields
        // They are not in the original schema but are required by FullAuditedAggregateRoot
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // ✅ Primary key: snake_case
        builder.HasKey(e => e.Id).HasName("pk_pro_table_rate");

        // Configure LobId foreign key relationship
        builder.HasOne(e => e.Lob)
            .WithMany(l => l.TableRates)
            .HasForeignKey(e => e.LobId)
            .HasConstraintName("fk_pro_table_rate_pro_line_of_business_lob_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Configure InsurerId foreign key relationship
        builder.HasOne(e => e.Insurer)
            .WithMany(p => p.TableRates)
            .HasForeignKey(e => e.InsurerId)
            .HasConstraintName("fk_pro_table_rate_res_partner_insurer_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
