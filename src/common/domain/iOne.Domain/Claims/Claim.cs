using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.ProLineOfBusinesses;
using iOne.ResClaimTypes;
using iOne.ResPartners;
using iOne.ResReasons;

namespace iOne.Claims;

[Table("claim")]
public class Claim : FullAuditedAggregateRoot<Guid>
{
    // Foreign Keys
    [Required]
    public virtual Guid LobId { get; private set; }

    public virtual Guid? ClaimTypeId { get; private set; }

    public virtual Guid? InsurerId { get; private set; }

    public virtual Guid? CancelReasonId { get; private set; }

    [Required]
    public virtual Guid OpenEmployeeId { get; private set; }

    public virtual Guid? CloseEmployeeId { get; private set; }

    public virtual Guid? CancelEmployeeId { get; private set; }

    /// <summary>Đơn vị giám định (khi assign).</summary>
    public virtual Guid? ProcessDeptId { get; private set; }

    /// <summary>Người giám định (khi assign).</summary>
    public virtual Guid? ProcessEmpId { get; private set; }

    /// <summary>Số GCN (Giấy chứng nhận bảo hiểm).</summary>
    [MaxLength(50)]
    public virtual string? CertificateNo { get; private set; }

    // Basic Info
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    public virtual ProcessClaimType ProcessClaimType { get; private set; }

    [MaxLength(50)]
    public virtual string? InsurerIncidentCode { get; private set; }

    [Required]
    public virtual ClaimStatus Status { get; private set; }

    // Dates
    [Required]
    public virtual DateTime OpenDate { get; private set; }

    public virtual DateTime? CloseDate { get; private set; }

    public virtual DateTime? CancelDate { get; private set; }

    [Required]
    public virtual DateTime NotifyDate { get; private set; }

    // Notifier Info
    [Required]
    [MaxLength(250)]
    public virtual string NotifierName { get; private set; } = null!;

    [Required]
    [MaxLength(15)]
    public virtual string NotifierPhone { get; private set; } = null!;

    [MaxLength(50)]
    public virtual string? NotifierEmail { get; private set; }

    [MaxLength(50)]
    public virtual string? NotifierInRelationship { get; private set; }

    // Contact Info
    [Required]
    [MaxLength(250)]
    public virtual string ContactName { get; private set; } = null!;

    [Required]
    [MaxLength(15)]
    public virtual string ContactPhone { get; private set; } = null!;

    [MaxLength(50)]
    public virtual string? ContactEmail { get; private set; }

    [MaxLength(50)]
    public virtual string? ContactInRelationship { get; private set; }

    // Other Info
    [Required]
    public virtual ClaimPriority Priority { get; private set; }

    [MaxLength(250)]
    public virtual string? SnapshotLink { get; private set; }

    [MaxLength(500)]
    public virtual string? CancelNote { get; private set; }

    // Navigation Properties
    public virtual ProLineOfBusiness Lob { get; set; } = null!;
    public virtual ResClaimType? ClaimType { get; set; }
    public virtual ResPartner? Insurer { get; set; }
    public virtual ResReason? CancelReason { get; set; }
    public virtual HrEmployee OpenEmployee { get; set; } = null!;
    public virtual HrEmployee? CloseEmployee { get; set; }
    public virtual HrEmployee? CancelEmployee { get; set; }
    public virtual HrDepartment? ProcessDepartment { get; set; }
    public virtual HrEmployee? ProcessEmployee { get; set; }

    protected Claim()
    {
        // For ORM
    }

    public Claim(
        Guid id,
        Guid lobId,
        string code,
        ProcessClaimType processClaimType,
        ClaimStatus status,
        DateTime openDate,
        DateTime notifyDate,
        string notifierName,
        string notifierPhone,
        string contactName,
        string contactPhone,
        ClaimPriority priority,
        Guid openEmployeeId,
        Guid? claimTypeId = null,
        Guid? insurerId = null,
        string? insurerIncidentCode = null,
        DateTime? closeDate = null,
        DateTime? cancelDate = null,
        Guid? closeEmployeeId = null,
        Guid? cancelEmployeeId = null,
        Guid? cancelReasonId = null,
        string? cancelNote = null,
        string? notifierEmail = null,
        string? notifierInRelationship = null,
        string? contactEmail = null,
        string? contactInRelationship = null,
        string? snapshotLink = null,
        Guid? processDeptId = null,
        Guid? processEmpId = null,
        string? certificateNo = null)
        : base(id)
    {
        SetLobId(lobId);
        SetCode(code);
        SetProcessClaimType(processClaimType);
        SetStatus(status);
        SetOpenDate(openDate);
        SetNotifyDate(notifyDate);
        SetNotifierName(notifierName);
        SetNotifierPhone(notifierPhone);
        SetContactName(contactName);
        SetContactPhone(contactPhone);
        SetPriority(priority);
        SetOpenEmployeeId(openEmployeeId);
        SetClaimTypeId(claimTypeId);
        SetInsurerId(insurerId);
        SetInsurerIncidentCode(insurerIncidentCode);
        SetCloseDate(closeDate);
        SetCancelDate(cancelDate);
        SetCloseEmployeeId(closeEmployeeId);
        SetCancelEmployeeId(cancelEmployeeId);
        SetCancelReasonId(cancelReasonId);
        SetCancelNote(cancelNote);
        SetNotifierEmail(notifierEmail);
        SetNotifierInRelationship(notifierInRelationship);
        SetContactEmail(contactEmail);
        SetContactInRelationship(contactInRelationship);
        SetSnapshotLink(snapshotLink);
        SetProcessDeptId(processDeptId);
        SetProcessEmpId(processEmpId);
        SetCertificateNo(certificateNo);
    }

    private void SetLobId(Guid lobId)
    {
        if (lobId == Guid.Empty)
        {
            throw new ArgumentException("LobId cannot be empty.", nameof(lobId));
        }
        LobId = lobId;
    }

    private void SetClaimTypeId(Guid? claimTypeId)
    {
        ClaimTypeId = claimTypeId;
    }

    private void SetInsurerId(Guid? insurerId)
    {
        InsurerId = insurerId;
    }

    private void SetCancelReasonId(Guid? cancelReasonId)
    {
        CancelReasonId = cancelReasonId;
    }

    private void SetOpenEmployeeId(Guid openEmployeeId)
    {
        if (openEmployeeId == Guid.Empty)
        {
            throw new ArgumentException("OpenEmployeeId cannot be empty.", nameof(openEmployeeId));
        }
        OpenEmployeeId = openEmployeeId;
    }

    private void SetCloseEmployeeId(Guid? closeEmployeeId)
    {
        CloseEmployeeId = closeEmployeeId;
    }

    private void SetCancelEmployeeId(Guid? cancelEmployeeId)
    {
        CancelEmployeeId = cancelEmployeeId;
    }

    private void SetProcessDeptId(Guid? processDeptId)
    {
        ProcessDeptId = processDeptId;
    }

    private void SetProcessEmpId(Guid? processEmpId)
    {
        ProcessEmpId = processEmpId;
    }

    private void SetCertificateNo(string? certificateNo)
    {
        if (!string.IsNullOrWhiteSpace(certificateNo) && certificateNo.Length > 50)
        {
            throw new ArgumentException("CertificateNo cannot exceed 50 characters.", nameof(certificateNo));
        }
        CertificateNo = certificateNo;
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Code cannot exceed 50 characters.", nameof(code));
        }

        Code = code;
    }

    private void SetProcessClaimType(ProcessClaimType processClaimType)
    {
        ProcessClaimType = processClaimType;
    }

    private void SetInsurerIncidentCode(string? insurerIncidentCode)
    {
        if (!string.IsNullOrWhiteSpace(insurerIncidentCode) && insurerIncidentCode.Length > 50)
        {
            throw new ArgumentException("InsurerIncidentCode cannot exceed 50 characters.", nameof(insurerIncidentCode));
        }
        InsurerIncidentCode = insurerIncidentCode;
    }

    private void SetStatus(ClaimStatus status)
    {
        Status = status;
    }

    private void SetOpenDate(DateTime openDate)
    {
        OpenDate = openDate;
    }

    private void SetCloseDate(DateTime? closeDate)
    {
        CloseDate = closeDate;
    }

    private void SetCancelDate(DateTime? cancelDate)
    {
        CancelDate = cancelDate;
    }

    private void SetNotifyDate(DateTime notifyDate)
    {
        NotifyDate = notifyDate;
    }

    private void SetNotifierName(string notifierName)
    {
        if (string.IsNullOrWhiteSpace(notifierName))
        {
            throw new ArgumentException("NotifierName cannot be null or empty.", nameof(notifierName));
        }

        if (notifierName.Length > 250)
        {
            throw new ArgumentException("NotifierName cannot exceed 250 characters.", nameof(notifierName));
        }

        NotifierName = notifierName;
    }

    private void SetNotifierPhone(string notifierPhone)
    {
        if (string.IsNullOrWhiteSpace(notifierPhone))
        {
            throw new ArgumentException("NotifierPhone cannot be null or empty.", nameof(notifierPhone));
        }

        if (notifierPhone.Length > 15)
        {
            throw new ArgumentException("NotifierPhone cannot exceed 15 characters.", nameof(notifierPhone));
        }

        NotifierPhone = notifierPhone;
    }

    private void SetNotifierEmail(string? notifierEmail)
    {
        if (!string.IsNullOrWhiteSpace(notifierEmail) && notifierEmail.Length > 50)
        {
            throw new ArgumentException("NotifierEmail cannot exceed 50 characters.", nameof(notifierEmail));
        }
        NotifierEmail = notifierEmail;
    }

    private void SetNotifierInRelationship(string? notifierInRelationship)
    {
        if (!string.IsNullOrWhiteSpace(notifierInRelationship) && notifierInRelationship.Length > 50)
        {
            throw new ArgumentException("NotifierInRelationship cannot exceed 50 characters.", nameof(notifierInRelationship));
        }
        NotifierInRelationship = notifierInRelationship;
    }

    private void SetContactName(string contactName)
    {
        if (string.IsNullOrWhiteSpace(contactName))
        {
            throw new ArgumentException("ContactName cannot be null or empty.", nameof(contactName));
        }

        if (contactName.Length > 250)
        {
            throw new ArgumentException("ContactName cannot exceed 250 characters.", nameof(contactName));
        }

        ContactName = contactName;
    }

    private void SetContactPhone(string contactPhone)
    {
        if (string.IsNullOrWhiteSpace(contactPhone))
        {
            throw new ArgumentException("ContactPhone cannot be null or empty.", nameof(contactPhone));
        }

        if (contactPhone.Length > 15)
        {
            throw new ArgumentException("ContactPhone cannot exceed 15 characters.", nameof(contactPhone));
        }

        ContactPhone = contactPhone;
    }

    private void SetContactEmail(string? contactEmail)
    {
        if (!string.IsNullOrWhiteSpace(contactEmail) && contactEmail.Length > 50)
        {
            throw new ArgumentException("ContactEmail cannot exceed 50 characters.", nameof(contactEmail));
        }
        ContactEmail = contactEmail;
    }

    private void SetContactInRelationship(string? contactInRelationship)
    {
        if (!string.IsNullOrWhiteSpace(contactInRelationship) && contactInRelationship.Length > 50)
        {
            throw new ArgumentException("ContactInRelationship cannot exceed 50 characters.", nameof(contactInRelationship));
        }
        ContactInRelationship = contactInRelationship;
    }

    private void SetPriority(ClaimPriority priority)
    {
        Priority = priority;
    }

    private void SetSnapshotLink(string? snapshotLink)
    {
        if (!string.IsNullOrWhiteSpace(snapshotLink) && snapshotLink.Length > 250)
        {
            throw new ArgumentException("SnapshotLink cannot exceed 250 characters.", nameof(snapshotLink));
        }
        SnapshotLink = snapshotLink;
    }

    private void SetCancelNote(string? cancelNote)
    {
        if (!string.IsNullOrWhiteSpace(cancelNote) && cancelNote.Length > 500)
        {
            throw new ArgumentException("CancelNote cannot exceed 500 characters.", nameof(cancelNote));
        }
        CancelNote = cancelNote;
    }

    // Update methods
    public virtual void UpdateLobId(Guid lobId)
    {
        SetLobId(lobId);
    }

    public virtual void UpdateClaimTypeId(Guid? claimTypeId)
    {
        SetClaimTypeId(claimTypeId);
    }

    public virtual void UpdateInsurerId(Guid? insurerId)
    {
        SetInsurerId(insurerId);
    }

    public virtual void UpdateCancelReasonId(Guid? cancelReasonId)
    {
        SetCancelReasonId(cancelReasonId);
    }

    public virtual void UpdateOpenEmployeeId(Guid openEmployeeId)
    {
        SetOpenEmployeeId(openEmployeeId);
    }

    public virtual void UpdateCloseEmployeeId(Guid? closeEmployeeId)
    {
        SetCloseEmployeeId(closeEmployeeId);
    }

    public virtual void UpdateCancelEmployeeId(Guid? cancelEmployeeId)
    {
        SetCancelEmployeeId(cancelEmployeeId);
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateProcessClaimType(ProcessClaimType processClaimType)
    {
        SetProcessClaimType(processClaimType);
    }

    public virtual void UpdateInsurerIncidentCode(string? insurerIncidentCode)
    {
        SetInsurerIncidentCode(insurerIncidentCode);
    }

    public virtual void UpdateStatus(ClaimStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateOpenDate(DateTime openDate)
    {
        SetOpenDate(openDate);
    }

    public virtual void UpdateCloseDate(DateTime? closeDate)
    {
        SetCloseDate(closeDate);
    }

    public virtual void UpdateCancelDate(DateTime? cancelDate)
    {
        SetCancelDate(cancelDate);
    }

    public virtual void UpdateNotifyDate(DateTime notifyDate)
    {
        SetNotifyDate(notifyDate);
    }

    public virtual void UpdateNotifierName(string notifierName)
    {
        SetNotifierName(notifierName);
    }

    public virtual void UpdateNotifierPhone(string notifierPhone)
    {
        SetNotifierPhone(notifierPhone);
    }

    public virtual void UpdateNotifierEmail(string? notifierEmail)
    {
        SetNotifierEmail(notifierEmail);
    }

    public virtual void UpdateNotifierInRelationship(string? notifierInRelationship)
    {
        SetNotifierInRelationship(notifierInRelationship);
    }

    public virtual void UpdateContactName(string contactName)
    {
        SetContactName(contactName);
    }

    public virtual void UpdateContactPhone(string contactPhone)
    {
        SetContactPhone(contactPhone);
    }

    public virtual void UpdateContactEmail(string? contactEmail)
    {
        SetContactEmail(contactEmail);
    }

    public virtual void UpdateContactInRelationship(string? contactInRelationship)
    {
        SetContactInRelationship(contactInRelationship);
    }

    public virtual void UpdatePriority(ClaimPriority priority)
    {
        SetPriority(priority);
    }

    public virtual void UpdateSnapshotLink(string? snapshotLink)
    {
        SetSnapshotLink(snapshotLink);
    }

    public virtual void UpdateCancelNote(string? cancelNote)
    {
        SetCancelNote(cancelNote);
    }

    public virtual void UpdateProcessDeptId(Guid? processDeptId)
    {
        SetProcessDeptId(processDeptId);
    }

    public virtual void UpdateProcessEmpId(Guid? processEmpId)
    {
        SetProcessEmpId(processEmpId);
    }

    public virtual void UpdateCertificateNo(string? certificateNo)
    {
        SetCertificateNo(certificateNo);
    }
}
