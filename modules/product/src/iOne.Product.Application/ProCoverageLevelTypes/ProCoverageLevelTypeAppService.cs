using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProCoverageLevelTypes;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProCoverageLevelTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProCoverageLevelTypes;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProCoveragePermissions.Default)]
public class ProCoverageLevelTypeAppService : CrudAppService<
    ProCoverageLevelType,
    ProCoverageLevelTypeDto,
    Guid,
    GetProCoverageLevelTypesInput,
    CreateProCoverageLevelTypeDto,
    UpdateProCoverageLevelTypeDto>, IProCoverageLevelTypeAppService
{
    protected ProCoverageLevelTypeManager Manager { get; }
    protected IProCoverageLevelTypeRepository CoverageLevelTypeRepository { get; }

    public ProCoverageLevelTypeAppService(
        IProCoverageLevelTypeRepository repository,
        ProCoverageLevelTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        CoverageLevelTypeRepository = repository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProCoveragePermissions.View;
        GetListPolicyName = ProCoveragePermissions.View;
        CreatePolicyName = ProCoveragePermissions.Create;
        UpdatePolicyName = ProCoveragePermissions.Edit;
        DeletePolicyName = ProCoveragePermissions.Delete;
    }

    public override async Task<ProCoverageLevelTypeDto> CreateAsync(CreateProCoverageLevelTypeDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await CoverageLevelTypeRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new BusinessException("Product:ProCoverageLevelType:CodeExists")
                    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ProCoverageLevelType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProCoverageLevelType, ProCoverageLevelTypeDto>(entity);
    }

    public override async Task<ProCoverageLevelTypeDto> UpdateAsync(Guid id, UpdateProCoverageLevelTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProCoverageLevelType, ProCoverageLevelTypeDto>(entity!);
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
        entity!.UpdateStatus(ProCoverageLevelTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProCoverageLevelType>> CreateFilteredQueryAsync(GetProCoverageLevelTypesInput input)
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
