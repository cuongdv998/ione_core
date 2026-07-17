using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyCoverageLevels;
using iOne.Policies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyCoverageLevels;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyCoverageLevelPermissions.Default)]
public class PolicyCoverageLevelAppService : CrudAppService<
    PolicyCoverageLevel,
    PolicyCoverageLevelDto,
    Guid,
    GetPolicyCoverageLevelsInput,
    CreatePolicyCoverageLevelDto,
    UpdatePolicyCoverageLevelDto>, IPolicyCoverageLevelAppService
{
    protected PolicyCoverageLevelManager Manager { get; }
    protected IPolicyCoverageLevelRepository PolicyCoverageLevelRepository { get; }

    public PolicyCoverageLevelAppService(
        IRepository<PolicyCoverageLevel, Guid> repository,
        PolicyCoverageLevelManager manager,
        IPolicyCoverageLevelRepository policyCoverageLevelRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyCoverageLevelRepository = policyCoverageLevelRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyCoverageLevelPermissions.View;
        GetListPolicyName = PolicyCoverageLevelPermissions.View;
        CreatePolicyName = PolicyCoverageLevelPermissions.Create;
        UpdatePolicyName = PolicyCoverageLevelPermissions.Edit;
        DeletePolicyName = PolicyCoverageLevelPermissions.Delete;
    }

    public override async Task<PolicyCoverageLevelDto> CreateAsync(CreatePolicyCoverageLevelDto input)
    {
        var entity = new PolicyCoverageLevel(
            GuidGenerator.Create(),
            input.PolicyCoverageId,
            input.CoverageLevelTypeId,
            input.CoverageLevelBasisId,
            input.AmountType,
            input.FromAmount,
            input.ToAmount,
            input.ConditionScript,
            input.ComputeScript
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyCoverageLevel, PolicyCoverageLevelDto>(entity);
    }

    public override async Task<PolicyCoverageLevelDto> UpdateAsync(Guid id, UpdatePolicyCoverageLevelDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Update entity properties
        entity.UpdateCoverageLevelTypeId(input.CoverageLevelTypeId);
        entity.UpdateCoverageLevelBasisId(input.CoverageLevelBasisId);
        entity.UpdateConditionScript(input.ConditionScript);
        entity.UpdateComputeScript(input.ComputeScript);
        entity.UpdateAmountType(input.AmountType);
        entity.UpdateFromAmount(input.FromAmount);
        entity.UpdateToAmount(input.ToAmount);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyCoverageLevel, PolicyCoverageLevelDto>(entity);
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

    protected override async Task<IQueryable<PolicyCoverageLevel>> CreateFilteredQueryAsync(GetPolicyCoverageLevelsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PolicyCoverageId
        if (input.PolicyCoverageId.HasValue)
        {
            query = query.Where(x => x.PolicyCoverageId == input.PolicyCoverageId.Value);
        }

        // Filter by CoverageLevelTypeId
        if (input.CoverageLevelTypeId.HasValue)
        {
            query = query.Where(x => x.CoverageLevelTypeId == input.CoverageLevelTypeId.Value);
        }

        // Filter by CoverageLevelBasisId
        if (input.CoverageLevelBasisId.HasValue)
        {
            query = query.Where(x => x.CoverageLevelBasisId == input.CoverageLevelBasisId.Value);
        }

        // Filter by AmountType (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.AmountType))
        {
            query = query.Where(x => EF.Functions.ILike(x.AmountType, $"%{input.AmountType}%"));
        }

        return query;
    }
}
