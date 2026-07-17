using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimAdjustAtLocations;
using iOne.Claims;
using iOne.HrEmployees;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimAdjustAtLocations;

[Table("claim_adjust_at_location")]
public class ClaimAdjustAtLocation : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimId { get; private set; }

    [Required]
    public virtual Guid AdjustorId { get; private set; }

    public virtual DateTime? StartDate { get; private set; }

    public virtual DateTime? EndDate { get; private set; }

    public virtual ClaimAdjustAtLocationStatus? Status { get; private set; }

    [MaxLength(50)]
    public virtual string? LossPosition { get; private set; }

    /// <summary>Y/N - có tổn thất bên thứ ba.</summary>
    [Required]
    [MaxLength(1)]
    public virtual string HasLossThirdParty { get; private set; } = "N";

    [MaxLength(500)]
    public virtual string? WitnessTestimony { get; private set; }

    [MaxLength(500)]
    public virtual string? CauseDescription { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [MaxLength(500)]
    public virtual string? LocationDescription { get; private set; }

    [MaxLength(500)]
    public virtual string? DamageDescription { get; private set; }

    [MaxLength(500)]
    public virtual string? PartiesInvolvedDescription { get; private set; }

    [MaxLength(500)]
    public virtual string? AddressPlan { get; private set; }

    [MaxLength(500)]
    public virtual string? CustomerRecommendation { get; private set; }

    [MaxLength(500)]
    public virtual string? OtherDescription { get; private set; }

    /// <summary>Y/N - có trong phạm vi bồi thường hay không.</summary>
    [MaxLength(1)]
    public virtual string? IsInScope { get; private set; } = "Y";

    public virtual Guid? GarageId { get; private set; }

    public virtual DateTime? IssueDate { get; private set; }

    // Navigation properties
    public virtual Claim? Claim { get; set; }
    public virtual HrEmployee? Adjustor { get; set; }
    public virtual ResPartner? Garage { get; set; }

    protected ClaimAdjustAtLocation()
    {
        // For ORM
    }

    public ClaimAdjustAtLocation(
        Guid id,
        Guid claimId,
        Guid adjustorId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        ClaimAdjustAtLocationStatus? status = null,
        string? lossPosition = null,
        string hasLossThirdParty = "N",
        string? witnessTestimony = null,
        string? causeDescription = null,
        string? description = null,
        string? locationDescription = null,
        string? damageDescription = null,
        string? partiesInvolvedDescription = null,
        string? addressPlan = null,
        string? customerRecommendation = null,
        string? otherDescription = null,
        string? isInScope = "Y",
        Guid? garageId = null,
        DateTime? issueDate = null)
        : base(id)
    {
        SetClaimId(claimId);
        SetAdjustorId(adjustorId);
        StartDate = startDate;
        EndDate = endDate;
        SetStatus(status);
        SetLossPosition(lossPosition);
        SetHasLossThirdParty(hasLossThirdParty);
        SetWitnessTestimony(witnessTestimony);
        SetCauseDescription(causeDescription);
        SetDescription(description);
        SetLocationDescription(locationDescription);
        SetDamageDescription(damageDescription);
        SetPartiesInvolvedDescription(partiesInvolvedDescription);
        SetAddressPlan(addressPlan);
        SetCustomerRecommendation(customerRecommendation);
        SetOtherDescription(otherDescription);
        SetIsInScope(isInScope);
        GarageId = garageId;
        IssueDate = issueDate;
    }

    private void SetClaimId(Guid claimId)
    {
        if (claimId == Guid.Empty)
        {
            throw new ArgumentException("ClaimId cannot be empty.", nameof(claimId));
        }
        ClaimId = claimId;
    }

    private void SetAdjustorId(Guid adjustorId)
    {
        if (adjustorId == Guid.Empty)
        {
        throw new ArgumentException("AdjustorId cannot be empty.", nameof(adjustorId));
        }
        AdjustorId = adjustorId;
    }

    private void SetStatus(ClaimAdjustAtLocationStatus? status)
    {
        Status = status;
    }

    private void SetLossPosition(string? lossPosition)
    {
        if (!string.IsNullOrWhiteSpace(lossPosition) && lossPosition.Length > 50)
        {
            throw new ArgumentException("LossPosition cannot exceed 50 characters.", nameof(lossPosition));
        }
        LossPosition = lossPosition;
    }

    private void SetHasLossThirdParty(string hasLossThirdParty)
    {
        if (string.IsNullOrWhiteSpace(hasLossThirdParty) || hasLossThirdParty.Length != 1 ||
            (hasLossThirdParty != "Y" && hasLossThirdParty != "N"))
        {
            throw new ArgumentException("HasLossThirdParty must be 'Y' or 'N'.", nameof(hasLossThirdParty));
        }
        HasLossThirdParty = hasLossThirdParty;
    }

    private void SetWitnessTestimony(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("WitnessTestimony cannot exceed 500 characters.", nameof(value));
        }
        WitnessTestimony = value;
    }

    private void SetCauseDescription(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("CauseDescription cannot exceed 500 characters.", nameof(value));
        }
        CauseDescription = value;
    }

    private void SetDescription(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(value));
        }
        Description = value;
    }

    private void SetLocationDescription(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("LocationDescription cannot exceed 500 characters.", nameof(value));
        }
        LocationDescription = value;
    }

    private void SetDamageDescription(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("DamageDescription cannot exceed 500 characters.", nameof(value));
        }
        DamageDescription = value;
    }

    private void SetPartiesInvolvedDescription(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("PartiesInvolvedDescription cannot exceed 500 characters.", nameof(value));
        }
        PartiesInvolvedDescription = value;
    }

    private void SetAddressPlan(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("AddressPlan cannot exceed 500 characters.", nameof(value));
        }
        AddressPlan = value;
    }

    private void SetCustomerRecommendation(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("CustomerRecommendation cannot exceed 500 characters.", nameof(value));
        }
        CustomerRecommendation = value;
    }

    private void SetOtherDescription(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 500)
        {
            throw new ArgumentException("OtherDescription cannot exceed 500 characters.", nameof(value));
        }
        OtherDescription = value;
    }

    private void SetIsInScope(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsInScope = "Y";
            return;
        }
        if (value.Length != 1 || (value != "Y" && value != "N"))
        {
            throw new ArgumentException("IsInScope must be 'Y' or 'N'.", nameof(value));
        }
        IsInScope = value;
    }

    public virtual void UpdateStatus(ClaimAdjustAtLocationStatus? status) => SetStatus(status);

    public virtual void UpdateAssignment(Guid adjustorId, DateTime? startDate, DateTime? endDate, ClaimAdjustAtLocationStatus? status = null)
    {
        SetAdjustorId(adjustorId);
        StartDate = startDate;
        EndDate = endDate;
        if (status.HasValue)
        {
            SetStatus(status);
        }
    }

    public virtual void UpdateAssessmentData(
        string? lossPosition,
        string hasLossThirdParty,
        string? witnessTestimony,
        string? causeDescription,
        string? description,
        string? locationDescription,
        string? damageDescription,
        string? partiesInvolvedDescription,
        string? addressPlan,
        string? customerRecommendation,
        string? otherDescription,
        Guid? garageId,
        DateTime? issueDate)
    {
        SetLossPosition(lossPosition);
        SetHasLossThirdParty(hasLossThirdParty);
        SetWitnessTestimony(witnessTestimony);
        SetCauseDescription(causeDescription);
        SetDescription(description);
        SetLocationDescription(locationDescription);
        SetDamageDescription(damageDescription);
        SetPartiesInvolvedDescription(partiesInvolvedDescription);
        SetAddressPlan(addressPlan);
        SetCustomerRecommendation(customerRecommendation);
        SetOtherDescription(otherDescription);
        GarageId = garageId;
        IssueDate = issueDate;
    }

    public virtual void MarkDone(DateTime completedAt)
    {
        if (!StartDate.HasValue)
        {
            StartDate = completedAt;
        }

        EndDate = completedAt;
        SetStatus(ClaimAdjustAtLocationStatus.Done);
    }
}
