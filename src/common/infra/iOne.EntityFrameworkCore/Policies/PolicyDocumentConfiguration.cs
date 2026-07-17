using System;
using iOne.Policies;
using iOne.ResDocuments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyDocumentConfiguration : IEntityTypeConfiguration<PolicyDocument>
{
    public void Configure(EntityTypeBuilder<PolicyDocument> builder)
    {
        builder.ToTable("policy_document", t =>
        {
            t.HasComment("Tài liệu đính kèm đơn bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id")
            .HasComment("Tham chiếu đến đơn bảo hiểm gốc");

        builder.Property(x => x.DocumentId)
            .HasColumnName("document_id")
            .HasComment("Tham chiếu đến res_document");

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
        builder.HasIndex(e => e.PolicyId, "ix_policy_document_policy_id");
        builder.HasIndex(e => e.DocumentId, "ix_policy_document_document_id");

        // Foreign Key Relationships
        builder.HasOne(x => x.Policy)
            .WithMany()
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_document_policy_id");

        builder.HasOne<ResDocument>()
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_document_res_document_id");
    }
}

