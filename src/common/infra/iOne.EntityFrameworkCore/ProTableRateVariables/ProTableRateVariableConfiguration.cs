using System;
using iOne.ProAttributes;
using iOne.ProTableRateVariables;
using iOne.ProTableRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProTableRateVariables;

public class ProTableRateVariableConfiguration : IEntityTypeConfiguration<ProTableRateVariable>
{
    public void Configure(EntityTypeBuilder<ProTableRateVariable> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL standard)
        builder.ToTable("pro_table_rate_variable", t =>
        {
            t.HasComment("Định nghĩa biến đầu vào của một bảng Rate");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL standard)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TableRateId)
            .HasColumnName("table_rate_id")
            .IsRequired();

        builder.Property(x => x.AttributeId)
            .HasColumnName("attribute_id")
            .IsRequired();
        
        // ✅ Operator: String property (no conversion needed)
        builder.Property(x => x.Operator)
            .HasColumnName("operator")
            .HasMaxLength(15)
            .IsRequired();
        
        // Note: AuditedEntity doesn't have ExtraProperties like FullAuditedAggregateRoot
        // ExtraProperties is only available on AggregateRoot entities
        
        // ✅ Audit columns: snake_case (PostgreSQL standard) - AuditedEntity only has CreationTime, CreatorId, LastModificationTime, LastModifierId
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");

        // ✅ Primary key: snake_case
        builder.HasKey(e => e.Id).HasName("pk_pro_table_rate_variable");

        // Note: ProTableRateVariable is part of ProTableRate aggregate
        // It uses AuditedEntity (no soft delete), so no IsDeleted, DeletionTime, DeleterId, ConcurrencyStamp

        // Configure TableRateId foreign key relationship
        // Note: Cascade delete - when ProTableRate is deleted, Variables are also deleted
        builder.HasOne(e => e.TableRate)
            .WithMany(t => t.Variables)
            .HasForeignKey(e => e.TableRateId)
            .HasConstraintName("fk_pro_table_rate_variable_pro_table_rate_table_rate_id")
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete since Variables are part of the aggregate

        // Configure AttributeId foreign key relationship
        builder.HasOne(e => e.Attribute)
            .WithMany(a => a.TableRateVariables)
            .HasForeignKey(e => e.AttributeId)
            .HasConstraintName("fk_pro_table_rate_variable_pro_attribute_attribute_id")
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ Indexes: snake_case
        builder.HasIndex(e => e.TableRateId, "ix_pro_table_rate_variable_table_rate_id");
        builder.HasIndex(e => e.AttributeId, "ix_pro_table_rate_variable_attribute_id");
    }
}
