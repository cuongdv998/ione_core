using System;
using iOne.ResDocuments;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.PolicyContracts;

public class PolicyContractDocument : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    public virtual Guid? PolicyContractId { get; private set; }

    public virtual Guid? DocumentId { get; private set; } // ResDocument.Id

    // Navigation Properties
    public virtual PolicyContract? PolicyContract { get; set; }
    public virtual ResDocument? Document { get; set; }

    protected PolicyContractDocument()
    {
        // For ORM
    }

    public PolicyContractDocument(
        Guid id,
        Guid? policyContractId = null,
        Guid? documentId = null)
        : base(id)
    {
        SetPolicyContractId(policyContractId);
        SetDocumentId(documentId);
    }

    private void SetPolicyContractId(Guid? policyContractId)
    {
        if (policyContractId.HasValue && policyContractId.Value == Guid.Empty)
        {
            throw new ArgumentException("PolicyContractId cannot be empty.", nameof(policyContractId));
        }

        PolicyContractId = policyContractId;
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
