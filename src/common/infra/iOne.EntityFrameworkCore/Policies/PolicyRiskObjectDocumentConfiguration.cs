using iOne.Policies;
using iOne.ResDocuments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyRiskObjectDocumentConfiguration : IEntityTypeConfiguration<PolicyRiskObjectDocument>
{
    public void Configure(EntityTypeBuilder<PolicyRiskObjectDocument> builder)
    {
        builder.ToTable("policy_risk_object_document", t =>
        {
            t.HasComment("Tài liệu liên quan đến đối tượng bảo hiểm (PolicyRiskObject)");
        });

        // Composite PK
        builder.HasKey(x => new { x.PolicyRiskObjectId, x.DocumentId });

        builder.Property(x => x.PolicyRiskObjectId)
            .HasColumnName("policy_risk_object_id")
            .IsRequired();

        builder.Property(x => x.DocumentId)
            .HasColumnName("document_id")
            .IsRequired();

        builder.HasOne(x => x.PolicyRiskObject)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.PolicyRiskObjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_risk_object_document_risk_object_id");

        builder.HasOne<ResDocument>()
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_risk_object_document_document_id");

        builder.HasIndex(x => x.PolicyRiskObjectId, "ix_policy_risk_object_document_risk_object_id");
        builder.HasIndex(x => x.DocumentId, "ix_policy_risk_object_document_document_id");
    }
}

