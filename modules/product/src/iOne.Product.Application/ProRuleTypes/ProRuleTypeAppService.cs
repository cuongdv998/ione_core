using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProRuleTypes;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProRuleTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProRuleTypes;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProRuleTypePermissions.Default)]
public class ProRuleTypeAppService : CrudAppService<
    ProRuleType,
    ProRuleTypeDto,
    Guid,
    GetProRuleTypesInput,
    CreateProRuleTypeDto,
    UpdateProRuleTypeDto>, IProRuleTypeAppService
{
    protected ProRuleTypeManager Manager { get; }
    protected IProRuleTypeRepository RuleTypeRepository { get; }

    public ProRuleTypeAppService(
        IProRuleTypeRepository repository,
        ProRuleTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        RuleTypeRepository = repository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProRuleTypePermissions.View;
        GetListPolicyName = ProRuleTypePermissions.View;
        CreatePolicyName = ProRuleTypePermissions.Create;
        UpdatePolicyName = ProRuleTypePermissions.Edit;
        DeletePolicyName = ProRuleTypePermissions.Delete;
    }

    public override async Task<ProRuleTypeDto> CreateAsync(CreateProRuleTypeDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await RuleTypeRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new BusinessException("Product:ProRuleType:CodeExists")
                    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ProRuleType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProRuleType, ProRuleTypeDto>(entity);
    }

    public override async Task<ProRuleTypeDto> UpdateAsync(Guid id, UpdateProRuleTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProRuleType, ProRuleTypeDto>(entity!);
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
        entity!.UpdateStatus(ProRuleTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProRuleType>> CreateFilteredQueryAsync(GetProRuleTypesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

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
