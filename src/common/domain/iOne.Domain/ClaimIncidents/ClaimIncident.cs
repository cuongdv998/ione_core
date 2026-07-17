using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.Claims;
using iOne.ResIncidentCauses;
using iOne.ResIncidentLevels;
using iOne.ResObjectTypes;
using iOne.ResPartners;
using iOne.ResProvinces;
using iOne.ResWards;

namespace iOne.ClaimIncidents;

[Table("claim_incident")]
public class ClaimIncident : FullAuditedAggregateRoot<Guid>
{
    // Foreign Keys
    [Required]
    public virtual Guid IncidentId { get; private set; }

    [Required]
    public virtual Guid ObjectTypeId { get; private set; }

    public virtual Guid? AssessmentPartnerId { get; private set; }

    public virtual Guid? IncidentLevelId { get; private set; }

    public virtual Guid? IncidentProvinceId { get; private set; }

    public virtual Guid? IncidentWardId { get; private set; }

    [Required]
    public virtual Guid IncidentCauseId { get; private set; }

    // Basic Info
    [Required]
    [MaxLength(1)]
    public virtual string OnLocation { get; private set; } = null!; // Y or N

    public virtual DateTime? AssessmentDate { get; private set; }

    public virtual DateTime? IncidentDate { get; private set; }

    [MaxLength(50)]
    public virtual string? IncidentAddress { get; private set; }

    [MaxLength(250)]
    public virtual string? IncidentFullAddress { get; private set; }

    public virtual double? IncidentLat { get; private set; }

    public virtual double? IncidentLong { get; private set; }

    [MaxLength(500)]
    public virtual string? IncidentDescription { get; private set; }

    [MaxLength(500)]
    public virtual string? IncidentResult { get; private set; }

    [MaxLength(500)]
    public virtual string? Note { get; private set; }

    // Navigation Properties
    //public virtual Claim Incident { get; set; } = null!;
    //public virtual ResObjectType ObjectType { get; set; } = null!;
    //public virtual ResPartner? AssessmentPartner { get; set; }
    //public virtual ResIncidentLevel? IncidentLevel { get; set; }
    //public virtual ResProvince? IncidentProvince { get; set; }
    //public virtual ResWard? IncidentWard { get; set; }
    //public virtual ResIncidentCause IncidentCause { get; set; } = null!;

    protected ClaimIncident()
    {
        // For ORM
    }

    public ClaimIncident(
        Guid id,
        Guid incidentId,
        Guid objectTypeId,
        string onLocation,
        Guid incidentCauseId,
        Guid? assessmentPartnerId = null,
        DateTime? assessmentDate = null,
        DateTime? incidentDate = null,
        Guid? incidentLevelId = null,
        Guid? incidentProvinceId = null,
        Guid? incidentWardId = null,
        string? incidentAddress = null,
        string? incidentFullAddress = null,
        double? incidentLat = null,
        double? incidentLong = null,
        string? incidentDescription = null,
        string? incidentResult = null,
        string? note = null)
        : base(id)
    {
        SetIncidentId(incidentId);
        SetObjectTypeId(objectTypeId);
        SetOnLocation(onLocation);
        SetIncidentCauseId(incidentCauseId);
        SetAssessmentPartnerId(assessmentPartnerId);
        SetAssessmentDate(assessmentDate);
        SetIncidentDate(incidentDate);
        SetIncidentLevelId(incidentLevelId);
        SetIncidentProvinceId(incidentProvinceId);
        SetIncidentWardId(incidentWardId);
        SetIncidentAddress(incidentAddress);
        SetIncidentFullAddress(incidentFullAddress);
        SetIncidentLat(incidentLat);
        SetIncidentLong(incidentLong);
        SetIncidentDescription(incidentDescription);
        SetIncidentResult(incidentResult);
        SetNote(note);
    }

    private void SetIncidentId(Guid incidentId)
    {
        if (incidentId == Guid.Empty)
        {
            throw new ArgumentException("IncidentId cannot be empty.", nameof(incidentId));
        }
        IncidentId = incidentId;
    }

    private void SetObjectTypeId(Guid objectTypeId)
    {
        if (objectTypeId == Guid.Empty)
        {
            throw new ArgumentException("ObjectTypeId cannot be empty.", nameof(objectTypeId));
        }
        ObjectTypeId = objectTypeId;
    }

    private void SetAssessmentPartnerId(Guid? assessmentPartnerId)
    {
        AssessmentPartnerId = assessmentPartnerId;
    }

    private void SetIncidentLevelId(Guid? incidentLevelId)
    {
        IncidentLevelId = incidentLevelId;
    }

    private void SetIncidentProvinceId(Guid? incidentProvinceId)
    {
        IncidentProvinceId = incidentProvinceId;
    }

    private void SetIncidentWardId(Guid? incidentWardId)
    {
        IncidentWardId = incidentWardId;
    }

    private void SetIncidentCauseId(Guid incidentCauseId)
    {
        if (incidentCauseId == Guid.Empty)
        {
            throw new ArgumentException("IncidentCauseId cannot be empty.", nameof(incidentCauseId));
        }
        IncidentCauseId = incidentCauseId;
    }

    private void SetOnLocation(string onLocation)
    {
        if (string.IsNullOrWhiteSpace(onLocation))
        {
            throw new ArgumentException("OnLocation cannot be null or empty.", nameof(onLocation));
        }

        var upperLocation = onLocation.ToUpperInvariant();
        if (upperLocation != "Y" && upperLocation != "N")
        {
            throw new ArgumentException("OnLocation must be 'Y' or 'N'.", nameof(onLocation));
        }

        if (upperLocation.Length > 1)
        {
            throw new ArgumentException("OnLocation cannot exceed 1 character.", nameof(onLocation));
        }

        OnLocation = upperLocation;
    }

    private void SetAssessmentDate(DateTime? assessmentDate)
    {
        AssessmentDate = assessmentDate;
    }

    private void SetIncidentDate(DateTime? incidentDate)
    {
        IncidentDate = incidentDate;
    }

    private void SetIncidentAddress(string? incidentAddress)
    {
        if (!string.IsNullOrWhiteSpace(incidentAddress) && incidentAddress.Length > 50)
        {
            throw new ArgumentException("IncidentAddress cannot exceed 50 characters.", nameof(incidentAddress));
        }
        IncidentAddress = incidentAddress;
    }

    private void SetIncidentFullAddress(string? incidentFullAddress)
    {
        if (!string.IsNullOrWhiteSpace(incidentFullAddress) && incidentFullAddress.Length > 250)
        {
            throw new ArgumentException("IncidentFullAddress cannot exceed 250 characters.", nameof(incidentFullAddress));
        }
        IncidentFullAddress = incidentFullAddress;
    }

    private void SetIncidentLat(double? incidentLat)
    {
        IncidentLat = incidentLat;
    }

    private void SetIncidentLong(double? incidentLong)
    {
        IncidentLong = incidentLong;
    }

    private void SetIncidentDescription(string? incidentDescription)
    {
        if (!string.IsNullOrWhiteSpace(incidentDescription) && incidentDescription.Length > 500)
        {
            throw new ArgumentException("IncidentDescription cannot exceed 500 characters.", nameof(incidentDescription));
        }
        IncidentDescription = incidentDescription;
    }

    private void SetIncidentResult(string? incidentResult)
    {
        if (!string.IsNullOrWhiteSpace(incidentResult) && incidentResult.Length > 500)
        {
            throw new ArgumentException("IncidentResult cannot exceed 500 characters.", nameof(incidentResult));
        }
        IncidentResult = incidentResult;
    }

    private void SetNote(string? note)
    {
        if (!string.IsNullOrWhiteSpace(note) && note.Length > 500)
        {
            throw new ArgumentException("Note cannot exceed 500 characters.", nameof(note));
        }
        Note = note;
    }

    // Update methods
    public virtual void UpdateIncidentId(Guid incidentId)
    {
        SetIncidentId(incidentId);
    }

    public virtual void UpdateObjectTypeId(Guid objectTypeId)
    {
        SetObjectTypeId(objectTypeId);
    }

    public virtual void UpdateAssessmentPartnerId(Guid? assessmentPartnerId)
    {
        SetAssessmentPartnerId(assessmentPartnerId);
    }

    public virtual void UpdateIncidentLevelId(Guid? incidentLevelId)
    {
        SetIncidentLevelId(incidentLevelId);
    }

    public virtual void UpdateIncidentProvinceId(Guid? incidentProvinceId)
    {
        SetIncidentProvinceId(incidentProvinceId);
    }

    public virtual void UpdateIncidentWardId(Guid? incidentWardId)
    {
        SetIncidentWardId(incidentWardId);
    }

    public virtual void UpdateIncidentCauseId(Guid incidentCauseId)
    {
        SetIncidentCauseId(incidentCauseId);
    }

    public virtual void UpdateOnLocation(string onLocation)
    {
        SetOnLocation(onLocation);
    }

    public virtual void UpdateAssessmentDate(DateTime? assessmentDate)
    {
        SetAssessmentDate(assessmentDate);
    }

    public virtual void UpdateIncidentDate(DateTime? incidentDate)
    {
        SetIncidentDate(incidentDate);
    }

    public virtual void UpdateIncidentAddress(string? incidentAddress)
    {
        SetIncidentAddress(incidentAddress);
    }

    public virtual void UpdateIncidentFullAddress(string? incidentFullAddress)
    {
        SetIncidentFullAddress(incidentFullAddress);
    }

    public virtual void UpdateIncidentLat(double? incidentLat)
    {
        SetIncidentLat(incidentLat);
    }

    public virtual void UpdateIncidentLong(double? incidentLong)
    {
        SetIncidentLong(incidentLong);
    }

    public virtual void UpdateIncidentDescription(string? incidentDescription)
    {
        SetIncidentDescription(incidentDescription);
    }

    public virtual void UpdateIncidentResult(string? incidentResult)
    {
        SetIncidentResult(incidentResult);
    }

    public virtual void UpdateNote(string? note)
    {
        SetNote(note);
    }
}
