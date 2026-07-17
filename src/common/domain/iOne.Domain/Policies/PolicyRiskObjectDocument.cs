using System;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResDocuments;
using Volo.Abp.Domain.Entities;

namespace iOne.Policies;

/// <summary>
/// Link table between PolicyRiskObject and ResDocument.
/// Uses a composite key: (PolicyRiskObjectId, DocumentId).
/// </summary>
[Table("policy_risk_object_document")]
public class PolicyRiskObjectDocument : Entity
{
    public virtual Guid PolicyRiskObjectId { get; private set; }

    public virtual Guid DocumentId { get; private set; }

    // Navigation
    public virtual PolicyRiskObject? PolicyRiskObject { get; set; }
    public virtual ResDocument? Document { get; set; }

    protected PolicyRiskObjectDocument()
    {
        // For ORM
    }

    public PolicyRiskObjectDocument(Guid policyRiskObjectId, Guid documentId)
    {
        PolicyRiskObjectId = policyRiskObjectId;
        DocumentId = documentId;
    }

    public override object[] GetKeys()
    {
        return new object[] { PolicyRiskObjectId, DocumentId };
    }
}

