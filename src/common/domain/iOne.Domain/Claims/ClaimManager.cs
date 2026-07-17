using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using iOne.HrEmployees;
using iOne.ProLineOfBusinesses;
using iOne.ResClaimTypes;
using iOne.ResPartners;
using iOne.ResReasons;

namespace iOne.Claims;

public class ClaimManager : DomainService
{
    protected IClaimRepository Repository { get; }

    public ClaimManager(IClaimRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(Claim claim)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(claim.Code))
        {
            throw new BusinessException("Master:Claim:CodeExists")
                .WithData("Code", claim.Code);
        }

        await Repository.InsertAsync(claim);
    }

    public virtual async Task UpdateAsync(
        Claim claim,
        Guid lobId,
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
    {
        claim.UpdateLobId(lobId);
        claim.UpdateProcessClaimType(processClaimType);
        claim.UpdateStatus(status);
        claim.UpdateOpenDate(openDate);
        claim.UpdateNotifyDate(notifyDate);
        claim.UpdateNotifierName(notifierName);
        claim.UpdateNotifierPhone(notifierPhone);
        claim.UpdateContactName(contactName);
        claim.UpdateContactPhone(contactPhone);
        claim.UpdatePriority(priority);
        claim.UpdateOpenEmployeeId(openEmployeeId);
        claim.UpdateClaimTypeId(claimTypeId);
        claim.UpdateInsurerId(insurerId);
        claim.UpdateInsurerIncidentCode(insurerIncidentCode);
        claim.UpdateCloseDate(closeDate);
        claim.UpdateCancelDate(cancelDate);
        claim.UpdateCloseEmployeeId(closeEmployeeId);
        claim.UpdateCancelEmployeeId(cancelEmployeeId);
        claim.UpdateCancelReasonId(cancelReasonId);
        claim.UpdateCancelNote(cancelNote);
        claim.UpdateNotifierEmail(notifierEmail);
        claim.UpdateNotifierInRelationship(notifierInRelationship);
        claim.UpdateContactEmail(contactEmail);
        claim.UpdateContactInRelationship(contactInRelationship);
        claim.UpdateSnapshotLink(snapshotLink);
        claim.UpdateProcessDeptId(processDeptId);
        claim.UpdateProcessEmpId(processEmpId);
        claim.UpdateCertificateNo(certificateNo);
        await Repository.UpdateAsync(claim);
    }
}
