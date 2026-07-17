using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyVersions;
using iOne.Policies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyVersions;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyVersionPermissions.Default)]
public class PolicyVersionAppService : CrudAppService<
    PolicyVersion,
    PolicyVersionDto,
    Guid,
    GetPolicyVersionsInput,
    CreatePolicyVersionDto,
    UpdatePolicyVersionDto>, IPolicyVersionAppService
{
    protected PolicyVersionManager Manager { get; }
    protected IPolicyVersionRepository PolicyVersionRepository { get; }

    public PolicyVersionAppService(
        IRepository<PolicyVersion, Guid> repository,
        PolicyVersionManager manager,
        IPolicyVersionRepository policyVersionRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyVersionRepository = policyVersionRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyVersionPermissions.View;
        GetListPolicyName = PolicyVersionPermissions.View;
        CreatePolicyName = PolicyVersionPermissions.Create;
        UpdatePolicyName = PolicyVersionPermissions.Edit;
        DeletePolicyName = PolicyVersionPermissions.Delete;
    }

    public override async Task<PolicyVersionDto> CreateAsync(CreatePolicyVersionDto input)
    {
        var entity = new PolicyVersion(
            GuidGenerator.Create(),
            input.Version,
            input.PolicyId,
            input.Type,
            input.Status,
            input.EffectDate,
            input.ExpireDate,
            input.OrgEffectDate,
            input.OrgExpireDate,
            input.PremiumTotal,
            input.Premium,
            input.Vat,
            input.InternalNote,
            input.CustomerNote,
            input.Discount,
            input.DiscountRate,
            input.Markup,
            input.ApprovalDate,
            input.ApproverId,
            input.ApprovalStatus,
            input.InsurerIntegrationStatus,
            input.InsurerIntegrationDescription,
            input.EndorsementType,
            input.EndorsementReasonId,
            input.EndorsementDescription
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyVersion, PolicyVersionDto>(entity);
    }

    public override async Task<PolicyVersionDto> UpdateAsync(Guid id, UpdatePolicyVersionDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Update entity properties
        entity.UpdateVersion(input.Version);
        entity.UpdateStatus(input.Status);
        entity.UpdateEffectDate(input.EffectDate);
        entity.UpdateExpireDate(input.ExpireDate);
        entity.UpdateOrgEffectDate(input.OrgEffectDate);
        entity.UpdateOrgExpireDate(input.OrgExpireDate);
        entity.UpdateInternalNote(input.InternalNote);
        entity.UpdateCustomerNote(input.CustomerNote);
        entity.UpdatePremiumTotal(input.PremiumTotal);
        entity.UpdatePremium(input.Premium);
        entity.UpdateVat(input.Vat);
        entity.UpdateDiscount(input.Discount);
        entity.UpdateDiscountRate(input.DiscountRate);
        entity.UpdateMarkup(input.Markup);
        entity.UpdateApprovalDate(input.ApprovalDate);
        entity.UpdateApproverId(input.ApproverId);
        entity.UpdateApprovalStatus(input.ApprovalStatus);
        entity.UpdateInsurerIntegrationStatus(input.InsurerIntegrationStatus);
        entity.UpdateInsurerIntegrationDescription(input.InsurerIntegrationDescription);
        entity.UpdateEndorsementType(input.EndorsementType);
        entity.UpdateEndorsementReasonId(input.EndorsementReasonId);
        entity.UpdateEndorsementDescription(input.EndorsementDescription);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyVersion, PolicyVersionDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<PolicyVersion>> CreateFilteredQueryAsync(GetPolicyVersionsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PolicyId
        if (input.PolicyId.HasValue)
        {
            query = query.Where(x => x.PolicyId == input.PolicyId.Value);
        }

        // Filter by Type (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Type))
        {
            query = query.Where(x => EF.Functions.ILike(x.Type, $"%{input.Type}%"));
        }

        // Filter by Status (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(x => EF.Functions.ILike(x.Status, $"%{input.Status}%"));
        }

        return query;
    }
}
