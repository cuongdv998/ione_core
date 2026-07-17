using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResDamageLevels;
using iOne.ResDamageLevels; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResDamageLevels;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResDamageLevelPermissions.Default)]
public class ResDamageLevelAppService : CrudAppService<
    ResDamageLevel,
    ResDamageLevelDto,
    Guid,
    GetResDamageLevelsInput,
    CreateResDamageLevelDto,
    UpdateResDamageLevelDto>, IResDamageLevelAppService
{
    protected ResDamageLevelManager Manager { get; }
    protected IResDamageLevelRepository DamageLevelRepository { get; }

    public ResDamageLevelAppService(
        IResDamageLevelRepository repository,
        ResDamageLevelManager manager)
        : base(repository)
    {
        Manager = manager;
        DamageLevelRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResDamageLevelPermissions.View;
        GetListPolicyName = ResDamageLevelPermissions.View;
        CreatePolicyName = ResDamageLevelPermissions.Create;
        UpdatePolicyName = ResDamageLevelPermissions.Edit;
        DeletePolicyName = ResDamageLevelPermissions.Delete;
    }

    public override async Task<ResDamageLevelDto> CreateAsync(CreateResDamageLevelDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await DamageLevelRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResDamageLevel:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResDamageLevel:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResDamageLevel(
            GuidGenerator.Create(),
            input.ObjectTypeId,
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResDamageLevel, ResDamageLevelDto>(entity);
    }

    public override async Task<ResDamageLevelDto> UpdateAsync(Guid id, UpdateResDamageLevelDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update ObjectTypeId, Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.ObjectTypeId, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResDamageLevel, ResDamageLevelDto>(entity!);
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
        entity!.UpdateStatus(ResDamageLevelStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResDamageLevel>> CreateFilteredQueryAsync(GetResDamageLevelsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by ObjectTypeId
        if (input.ObjectTypeId.HasValue)
        {
            query = query.Where(x => x.ObjectTypeId == input.ObjectTypeId.Value);
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

