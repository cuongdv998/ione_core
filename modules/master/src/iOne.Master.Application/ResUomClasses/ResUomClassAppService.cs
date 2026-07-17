using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResUomClasses;
using iOne.ResUomClasses; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResUomClasses;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResUomClassPermissions.Default)]
public class ResUomClassAppService : CrudAppService<
    ResUomClass,
    ResUomClassDto,
    Guid,
    GetResUomClassesInput,
    CreateResUomClassDto,
    UpdateResUomClassDto>, IResUomClassAppService
{
    protected ResUomClassManager Manager { get; }
    protected IResUomClassRepository UomClassRepository { get; }

    public ResUomClassAppService(
        IResUomClassRepository repository,
        ResUomClassManager manager)
        : base(repository)
    {
        Manager = manager;
        UomClassRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResUomClassPermissions.View;
        GetListPolicyName = ResUomClassPermissions.View;
        CreatePolicyName = ResUomClassPermissions.Create;
        UpdatePolicyName = ResUomClassPermissions.Edit;
        DeletePolicyName = ResUomClassPermissions.Delete;
    }

    public override async Task<ResUomClassDto> CreateAsync(CreateResUomClassDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await UomClassRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResUomClass:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        var entity = new ResUomClass(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResUomClass, ResUomClassDto>(entity);
    }

    public override async Task<ResUomClassDto> UpdateAsync(Guid id, UpdateResUomClassDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResUomClass, ResUomClassDto>(entity!);
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
        entity!.UpdateStatus(ResUomClassStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResUomClass>> CreateFilteredQueryAsync(GetResUomClassesInput input)
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

    [Authorize(ResUomClassPermissions.View)]
    public virtual async Task<List<ResUomClassSelectDto>> GetSelectListAsync()
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        query = query
            .Where(x => x.Status == ResUomClassStatus.Active && !x.IsDeleted)
            .OrderBy(x => x.Name);

        var entities = await AsyncExecuter.ToListAsync(query);

        return entities.Select(x => new ResUomClassSelectDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name
        }).ToList();
    }
}

