using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProRules;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProRules; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProRules;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProProductPermissions.Default)]
public class ProRuleAppService : CrudAppService<
    ProRule,
    ProRuleDto,
    Guid,
    GetProRulesInput,
    CreateProRuleDto,
    UpdateProRuleDto>, IProRuleAppService
{
    protected ProRuleManager Manager { get; }
    protected IProRuleRepository RuleRepository { get; }

    public ProRuleAppService(
        IProRuleRepository repository,
        ProRuleManager manager)
        : base(repository)
    {
        Manager = manager;
        RuleRepository = repository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProProductPermissions.View;
        GetListPolicyName = ProProductPermissions.View;
        CreatePolicyName = ProProductPermissions.Create;
        UpdatePolicyName = ProProductPermissions.Edit;
        DeletePolicyName = ProProductPermissions.Delete;
    }

    public override async Task<ProRuleDto> CreateAsync(CreateProRuleDto input)
    {
        // Validate ApplyToId is provided for standalone rule creation
        if (!input.ApplyToId.HasValue || input.ApplyToId.Value == Guid.Empty)
        {
            throw new BusinessException("Product:ProRule:ApplyToIdRequired")
                .WithData("ApplyToId", input.ApplyToId);
        }

        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await RuleRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ProRule:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        var entity = new ProRule(
            GuidGenerator.Create(),
            input.ApplyTo,
            input.ApplyToId.Value,
            input.RuleTypeId,
            input.Code,
            input.Name,
            input.RuleScript,
            input.Status,
            input.EffectDate,
            input.Description,
            input.Priority,
            input.ExpireDate
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProRule, ProRuleDto>(entity);
    }

    public override async Task<ProRuleDto> UpdateAsync(Guid id, UpdateProRuleDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update các fields được phép, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(
            entity,
            input.ApplyTo,
            input.ApplyToId,
            input.RuleTypeId,
            input.Name,
            input.Description,
            input.RuleScript,
            input.Priority,
            input.Status,
            input.EffectDate,
            input.ExpireDate
        );

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProRule, ProRuleDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ProRuleStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProRule>> CreateFilteredQueryAsync(GetProRulesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by ApplyTo
        if (!string.IsNullOrWhiteSpace(input.ApplyTo))
        {
            query = query.Where(x => x.ApplyTo == input.ApplyTo);
        }

        // Filter by ApplyToId
        if (input.ApplyToId.HasValue)
        {
            query = query.Where(x => x.ApplyToId == input.ApplyToId.Value);
        }

        // Filter by RuleTypeId
        if (input.RuleTypeId.HasValue)
        {
            query = query.Where(x => x.RuleTypeId == input.RuleTypeId.Value);
        }

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}
