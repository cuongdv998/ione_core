using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResDocuments;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_document")]
public class PolicyDocument : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    public virtual Guid? PolicyId { get; private set; }

    public virtual Guid? DocumentId { get; private set; } // ResDocument.Id

    // Navigation Properties
    public virtual Policy? Policy { get; set; }
    public virtual ResDocument? Document { get; set; }

    protected PolicyDocument()
    {
        // For ORM
    }

    public PolicyDocument(
        Guid id,
        Guid? policyId = null,
        Guid? documentId = null)
        : base(id)
    {
        SetPolicyId(policyId);
        SetDocumentId(documentId);
    }

    private void SetPolicyId(Guid? policyId)
    {
        if (policyId.HasValue && policyId.Value == Guid.Empty)
        {
            throw new ArgumentException("PolicyId cannot be empty.", nameof(policyId));
        }

        PolicyId = policyId;
    }

    private void SetDocumentId(Guid? documentId)
    {
        if (documentId.HasValue && documentId.Value == Guid.Empty)
        {
            throw new ArgumentException("DocumentId cannot be empty.", nameof(documentId));
        }

        DocumentId = documentId;
    }
}

