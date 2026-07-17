using iOne.PolicyContracts;
using iOne.ResDocuments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.PolicyContracts;

public class PolicyContractDocumentConfiguration : IEntityTypeConfiguration<PolicyContractDocument>
{
    public void Configure(EntityTypeBuilder<PolicyContractDocument> builder)
    {
        builder.ToTable("policy_contract_document", t =>
        {
            t.HasComment("Tài liệu liên quan đến hợp đồng");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyContractId)
            .HasColumnName("policy_contract_id");

        builder.Property(x => x.DocumentId)
            .HasColumnName("document_id");

        // Foreign Key Relationships
        builder.HasOne(x => x.PolicyContract)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.PolicyContractId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policyco_reference_policyco");

        builder.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policyco_reference_resdocum");

        // Indexes
        builder.HasIndex(e => e.PolicyContractId, "ix_policy_contract_document_contract_id");
        builder.HasIndex(e => e.DocumentId, "ix_policy_contract_document_document_id");

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
    }
}
