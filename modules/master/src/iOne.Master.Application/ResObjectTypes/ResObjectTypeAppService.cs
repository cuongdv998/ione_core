using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResObjectTypes;
using iOne.ResObjectTypes; // For Entity, Manager, Repository
using iOne.ResRisks; // For IResRiskRepository
using iOne.ResDamageLevels; // For IResDamageLevelRepository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResObjectTypes;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResObjectTypePermissions.Default)]
public class ResObjectTypeAppService : CrudAppService<
    ResObjectType,
    ResObjectTypeDto,
    Guid,
    GetResObjectTypesInput,
    CreateResObjectTypeDto,
    UpdateResObjectTypeDto>, IResObjectTypeAppService
{
    protected ResObjectTypeManager Manager { get; }
    protected IResObjectTypeRepository ObjectTypeRepository { get; }
    protected IResRiskRepository RiskRepository { get; }
    protected IResDamageLevelRepository DamageLevelRepository { get; }

    public ResObjectTypeAppService(
        IResObjectTypeRepository repository,
        ResObjectTypeManager manager,
        IResRiskRepository riskRepository,
        IResDamageLevelRepository damageLevelRepository)
        : base(repository)
    {
        Manager = manager;
        ObjectTypeRepository = repository;
        RiskRepository = riskRepository;
        DamageLevelRepository = damageLevelRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResObjectTypePermissions.View;
        GetListPolicyName = ResObjectTypePermissions.View;
        CreatePolicyName = ResObjectTypePermissions.Create;
        UpdatePolicyName = ResObjectTypePermissions.Edit;
        DeletePolicyName = ResObjectTypePermissions.Delete;
    }

    public override async Task<ResObjectTypeDto> CreateAsync(CreateResObjectTypeDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await ObjectTypeRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResObjectType:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        // Convert enum to string for entity
        var objectGroupString = ResObjectGroupHelper.ToString(input.ObjectGroup);

        var entity = new ResObjectType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            objectGroupString,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResObjectType, ResObjectTypeDto>(entity);
    }

    public override async Task<ResObjectTypeDto> UpdateAsync(Guid id, UpdateResObjectTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, ObjectGroup, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        // Convert enum to string for entity
        var objectGroupString = ResObjectGroupHelper.ToString(input.ObjectGroup);
        await Manager.UpdateAsync(entity, input.Name, objectGroupString, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResObjectType, ResObjectTypeDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Check if ObjectType is in use by any Risk
        if (await RiskRepository.IsObjectTypeInUseAsync(id))
        {
            throw new UserFriendlyException(
                L["ResObjectType:InUseByRisk"].Value.Replace("{Name}", entity.Name)
            );
        }
        
        // Check if ObjectType is in use by any DamageLevel
        if (await DamageLevelRepository.IsObjectTypeInUseAsync(id))
        {
            throw new UserFriendlyException(
                L["ResObjectType:InUseByDamageLevel"].Value.Replace("{Name}", entity.Name)
            );
        }
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ResObjectTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResObjectType>> CreateFilteredQueryAsync(GetResObjectTypesInput input)
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

        // Filter by ObjectGroup (convert enum to string)
        if (input.ObjectGroup.HasValue)
        {
            var objectGroupString = ResObjectGroupHelper.ToString(input.ObjectGroup);
            if (!string.IsNullOrWhiteSpace(objectGroupString))
            {
                query = query.Where(x => x.ObjectGroup == objectGroupString);
            }
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}

