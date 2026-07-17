using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyCoverages;
using iOne.Policies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyCoverages;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyCoveragePermissions.Default)]
public class PolicyCoverageAppService : CrudAppService<
    PolicyCoverage,
    PolicyCoverageDto,
    Guid,
    GetPolicyCoveragesInput,
    CreatePolicyCoverageDto,
    UpdatePolicyCoverageDto>, IPolicyCoverageAppService
{
    protected PolicyCoverageManager Manager { get; }
    protected IPolicyCoverageRepository PolicyCoverageRepository { get; }

    public PolicyCoverageAppService(
        IRepository<PolicyCoverage, Guid> repository,
        PolicyCoverageManager manager,
        IPolicyCoverageRepository policyCoverageRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyCoverageRepository = policyCoverageRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyCoveragePermissions.View;
        GetListPolicyName = PolicyCoveragePermissions.View;
        CreatePolicyName = PolicyCoveragePermissions.Create;
        UpdatePolicyName = PolicyCoveragePermissions.Edit;
        DeletePolicyName = PolicyCoveragePermissions.Delete;
    }

    public override async Task<PolicyCoverageDto> CreateAsync(CreatePolicyCoverageDto input)
    {
        var entity = new PolicyCoverage(
            GuidGenerator.Create(),
            input.PolicyProductId,
            input.CoverageId,
            input.UomId,
            input.Quantity,
            input.TaxId,
            input.PremiumRate,
            input.PremiumTotal,
            input.Premium,
            input.Vat,
            input.CoverageParentId,
            input.InsurerCoverageCode,
            input.TableRateLineId,
            input.AmountLiability,
            input.NetRate,
            input.BaseRate,
            input.FlatRate,
            input.Loading,
            input.Discount,
            input.DiscountRate
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyCoverage, PolicyCoverageDto>(entity);
    }

    public override async Task<PolicyCoverageDto> UpdateAsync(Guid id, UpdatePolicyCoverageDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Update entity properties
        entity.UpdateCoverageParentId(input.CoverageParentId);
        entity.UpdateInsurerCoverageCode(input.InsurerCoverageCode);
        entity.UpdateTableRateLineId(input.TableRateLineId);
        entity.UpdateAmountLiability(input.AmountLiability);
        entity.UpdateQuantity(input.Quantity);
        entity.UpdateNetRate(input.NetRate);
        entity.UpdateBaseRate(input.BaseRate);
        entity.UpdateFlatRate(input.FlatRate);
        entity.UpdateLoading(input.Loading);
        entity.UpdatePremiumRate(input.PremiumRate);
        entity.UpdatePremiumTotal(input.PremiumTotal);
        entity.UpdatePremium(input.Premium);
        entity.UpdateVat(input.Vat);
        entity.UpdateDiscount(input.Discount);
        entity.UpdateDiscountRate(input.DiscountRate);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyCoverage, PolicyCoverageDto>(entity);
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

    protected override async Task<IQueryable<PolicyCoverage>> CreateFilteredQueryAsync(GetPolicyCoveragesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PolicyProductId
        if (input.PolicyProductId.HasValue)
        {
            query = query.Where(x => x.PolicyProductId == input.PolicyProductId.Value);
        }

        // Filter by CoverageId
        if (input.CoverageId.HasValue)
        {
            query = query.Where(x => x.CoverageId == input.CoverageId.Value);
        }

        // Filter by CoverageParentId
        if (input.CoverageParentId.HasValue)
        {
            query = query.Where(x => x.CoverageParentId == input.CoverageParentId.Value);
        }

        return query;
    }
}
