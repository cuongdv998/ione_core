using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResReasonGroups;
using iOne.ResReasonGroups; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResReasonGroups;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResReasonGroupPermissions.Default)]
public class ResReasonGroupAppService : CrudAppService<
    ResReasonGroup,
    ResReasonGroupDto,
    Guid,
    GetResReasonGroupsInput,
    CreateResReasonGroupDto,
    UpdateResReasonGroupDto>, IResReasonGroupAppService
{
    protected ResReasonGroupManager Manager { get; }
    protected IResReasonGroupRepository ReasonGroupRepository { get; }

    public ResReasonGroupAppService(
        IResReasonGroupRepository repository,
        ResReasonGroupManager manager)
        : base(repository)
    {
        Manager = manager;
        ReasonGroupRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResReasonGroupPermissions.View;
        GetListPolicyName = ResReasonGroupPermissions.View;
        CreatePolicyName = ResReasonGroupPermissions.Create;
        UpdatePolicyName = ResReasonGroupPermissions.Edit;
        DeletePolicyName = ResReasonGroupPermissions.Delete;
    }

    public override async Task<ResReasonGroupDto> CreateAsync(CreateResReasonGroupDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await ReasonGroupRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResReasonGroup:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        var entity = new ResReasonGroup(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResReasonGroup, ResReasonGroupDto>(entity);
    }

    public override async Task<ResReasonGroupDto> UpdateAsync(Guid id, UpdateResReasonGroupDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResReasonGroup, ResReasonGroupDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Validate if the reason group is being used by any reasons
        if (await ReasonGroupRepository.HasReasonsAsync(id))
        {
            throw new UserFriendlyException(
                L["ResReasonGroup:HasReasons"].Value.Replace("{Name}", entity.Name)
            );
        }
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ResReasonGroupStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResReasonGroup>> CreateFilteredQueryAsync(GetResReasonGroupsInput input)
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
