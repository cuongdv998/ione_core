using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResReasons;
using iOne.ResReasons; // For Entity, Manager, Repository
using iOne.ResReasonGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Master.ResReasons;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResReasonPermissions.Default)]
public class ResReasonAppService : CrudAppService<
    ResReason,
    ResReasonDto,
    Guid,
    GetResReasonsInput,
    CreateResReasonDto,
    UpdateResReasonDto>, IResReasonAppService
{
    protected ResReasonManager Manager { get; }
    protected IResReasonRepository ReasonRepository { get; }
    protected IResReasonGroupRepository ReasonGroupRepository { get; }

    public ResReasonAppService(
        IResReasonRepository repository,
        ResReasonManager manager,
        IResReasonGroupRepository reasonGroupRepository)
        : base(repository)
    {
        Manager = manager;
        ReasonRepository = repository;
        ReasonGroupRepository = reasonGroupRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResReasonPermissions.View;
        GetListPolicyName = ResReasonPermissions.View;
        CreatePolicyName = ResReasonPermissions.Create;
        UpdatePolicyName = ResReasonPermissions.Edit;
        DeletePolicyName = ResReasonPermissions.Delete;
    }

    public override async Task<ResReasonDto> CreateAsync(CreateResReasonDto input)
    {
        // Validate reason group exists
        if (!await ReasonGroupRepository.AnyAsync(x => x.Id == input.GroupId))
        {
            throw new UserFriendlyException(
                    L["ResReason:GroupNotFound"].Value
                );
        }

        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await ReasonRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResReason:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        var entity = new ResReason(
            GuidGenerator.Create(),
            input.GroupId,
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResReason, ResReasonDto>(entity);
    }

    public override async Task<ResReasonDto> UpdateAsync(Guid id, UpdateResReasonDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code và GroupId
        // Code and GroupId are immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResReason, ResReasonDto>(entity!);
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
        entity!.UpdateStatus(ResReasonGroupStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResReason>> CreateFilteredQueryAsync(GetResReasonsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by GroupId
        if (input.GroupId.HasValue)
        {
            query = query.Where(x => x.GroupId == input.GroupId.Value);
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

    /// <summary>
    /// Lấy danh sách lý do cho dropdown theo mã nhóm (ví dụ INCIDENT_REASON). Chỉ cần đăng nhập.
    /// </summary>
    [Authorize]
    public virtual async Task<List<ResReasonSelectDto>> GetSelectListByGroupCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<ResReasonSelectDto>();
        }
        var group = await ReasonGroupRepository.FirstOrDefaultAsync(x => x.Code == code && x.Status == ResReasonGroupStatus.Active);
        if (group == null)
        {
            return new List<ResReasonSelectDto>();
        }
        var query = await ReadOnlyRepository.GetQueryableAsync();
        query = query.Where(x => x.GroupId == group.Id && x.Status == ResReasonGroupStatus.Active);
        query = query.OrderBy(x => x.Name);
        var entities = await AsyncExecuter.ToListAsync(query);
        return entities.Select(x => new ResReasonSelectDto
        {
            Id = x.Id,
            Code = x.Code ?? string.Empty,
            Name = x.Name ?? string.Empty
        }).ToList();
    }
}
