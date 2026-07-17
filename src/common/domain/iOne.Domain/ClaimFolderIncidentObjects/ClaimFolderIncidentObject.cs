using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolders;
using iOne.ResObjectTypes;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderIncidentObjects;

[Table("claim_folder_incident_object")]
public class ClaimFolderIncidentObject : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimFolderId { get; private set; }

    [Required]
    public virtual Guid ObjectTypeId { get; private set; }

    [MaxLength(50)]
    public virtual string? CarPlate { get; private set; }

    [MaxLength(50)]
    public virtual string? CarEngineNumber { get; private set; }

    [MaxLength(50)]
    public virtual string? CarVin { get; private set; }

    [MaxLength(250)]
    public virtual string? Name { get; private set; }

    [MaxLength(15)]
    public virtual string? IdNo { get; private set; }

    [MaxLength(25)]
    public virtual string? PassportNo { get; private set; }

    public virtual DateTime? ExitDate { get; private set; }

    [MaxLength(50)]
    public virtual string? ProfileNo { get; private set; }

    // Navigation
    public virtual ClaimFolder? ClaimFolder { get; set; }
    public virtual ResObjectType? ObjectType { get; set; }

    protected ClaimFolderIncidentObject()
    {
    }

    public ClaimFolderIncidentObject(
        Guid id,
        Guid claimFolderId,
        Guid objectTypeId,
        string? carPlate = null,
        string? carEngineNumber = null,
        string? carVin = null,
        string? name = null,
        string? idNo = null,
        string? passportNo = null,
        DateTime? exitDate = null,
        string? profileNo = null)
        : base(id)
    {
        SetClaimFolderId(claimFolderId);
        SetObjectTypeId(objectTypeId);
        SetCarPlate(carPlate);
        SetCarEngineNumber(carEngineNumber);
        SetCarVin(carVin);
        SetName(name);
        SetIdNo(idNo);
        SetPassportNo(passportNo);
        ExitDate = exitDate;
        SetProfileNo(profileNo);
    }

    private void SetClaimFolderId(Guid claimFolderId)
    {
        if (claimFolderId == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderId cannot be empty.", nameof(claimFolderId));
        }
        ClaimFolderId = claimFolderId;
    }

    private void SetObjectTypeId(Guid objectTypeId)
    {
        if (objectTypeId == Guid.Empty)
        {
            throw new ArgumentException("ObjectTypeId cannot be empty.", nameof(objectTypeId));
        }
        ObjectTypeId = objectTypeId;
    }

    private void SetCarPlate(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
        {
            throw new ArgumentException("CarPlate cannot exceed 50 characters.", nameof(value));
        }
        CarPlate = value;
    }

    private void SetCarEngineNumber(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
        {
            throw new ArgumentException("CarEngineNumber cannot exceed 50 characters.", nameof(value));
        }
        CarEngineNumber = value;
    }

    private void SetCarVin(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
        {
            throw new ArgumentException("CarVin cannot exceed 50 characters.", nameof(value));
        }
        CarVin = value;
    }

    private void SetName(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(value));
        }
        Name = value;
    }

    private void SetIdNo(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 15)
        {
            throw new ArgumentException("IdNo cannot exceed 15 characters.", nameof(value));
        }
        IdNo = value;
    }

    private void SetPassportNo(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 25)
        {
            throw new ArgumentException("PassportNo cannot exceed 25 characters.", nameof(value));
        }
        PassportNo = value;
    }

    private void SetProfileNo(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
        {
            throw new ArgumentException("ProfileNo cannot exceed 50 characters.", nameof(value));
        }
        ProfileNo = value;
    }

    private void SetExitDate(DateTime? value)
    {
        ExitDate = value;
    }

    public virtual void UpdateExitDate(DateTime? value) => SetExitDate(value);

    public virtual void UpdateDetailedAssessmentInfo(
        Guid objectTypeId,
        string? carPlate = null,
        string? carEngineNumber = null,
        string? carVin = null,
        string? name = null,
        string? idNo = null,
        DateTime? exitDate = null)
    {
        SetObjectTypeId(objectTypeId);
        SetCarPlate(carPlate);
        SetCarEngineNumber(carEngineNumber);
        SetCarVin(carVin);
        SetName(name);
        SetIdNo(idNo);
        SetExitDate(exitDate);
    }
}
