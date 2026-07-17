using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolders;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderQuotations;

[Table("claim_folder_quotation")]
public class ClaimFolderQuotation : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimFolderId { get; private set; }

    public virtual Guid? ClaimFolderIncidentObjectId { get; private set; }

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual Guid PartnerId { get; private set; }

    [Required]
    public virtual DateTime QuotationDate { get; private set; }

    [Required]
    public virtual decimal AmountTotal { get; private set; }

    /// <summary>Y/N - báo giá được chấp nhận.</summary>
    [Required]
    [MaxLength(1)]
    public virtual string IsAccept { get; private set; } = "N";

    // Navigation
    public virtual ClaimFolder? ClaimFolder { get; set; }
    public virtual ResPartner? Partner { get; set; }

    protected ClaimFolderQuotation()
    {
    }

    public ClaimFolderQuotation(
        Guid id,
        Guid claimFolderId,
        Guid partnerId,
        DateTime quotationDate,
        decimal amountTotal,
        string isAccept = "N",
        Guid? claimFolderIncidentObjectId = null,
        string? description = null)
        : base(id)
    {
        SetClaimFolderId(claimFolderId);
        SetPartnerId(partnerId);
        SetQuotationDate(quotationDate);
        SetAmountTotal(amountTotal);
        SetIsAccept(isAccept);
        ClaimFolderIncidentObjectId = claimFolderIncidentObjectId;
        SetDescription(description);
    }

    private void SetClaimFolderId(Guid claimFolderId)
    {
        if (claimFolderId == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderId cannot be empty.", nameof(claimFolderId));
        }
        ClaimFolderId = claimFolderId;
    }

    private void SetPartnerId(Guid partnerId)
    {
        if (partnerId == Guid.Empty)
        {
            throw new ArgumentException("PartnerId cannot be empty.", nameof(partnerId));
        }
        PartnerId = partnerId;
    }

    private void SetQuotationDate(DateTime quotationDate)
    {
        QuotationDate = quotationDate;
    }

    private void SetAmountTotal(decimal amountTotal)
    {
        if (amountTotal < 0)
        {
            throw new ArgumentException("AmountTotal cannot be negative.", nameof(amountTotal));
        }
        AmountTotal = amountTotal;
    }

    private void SetIsAccept(string isAccept)
    {
        if (string.IsNullOrWhiteSpace(isAccept) || isAccept.Length != 1 || (isAccept != "Y" && isAccept != "N"))
        {
            throw new ArgumentException("IsAccept must be 'Y' or 'N'.", nameof(isAccept));
        }
        IsAccept = isAccept;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
        }
        Description = description;
    }
}

