using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResEvents;
using iOne.ResEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResEvents;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResEventPermissions.Default)]
public class ResEventAppService : CrudAppService<
    ResEvent,
    ResEventDto,
    Guid,
    GetResEventsInput,
    CreateResEventDto,
    UpdateResEventDto>, IResEventAppService
{
    protected ResEventManager Manager { get; }
    protected IResEventRepository EventRepository { get; }

    public ResEventAppService(
        IResEventRepository repository,
        ResEventManager manager)
        : base(repository)
    {
        Manager = manager;
        EventRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResEventPermissions.View;
        GetListPolicyName = ResEventPermissions.View;
        CreatePolicyName = ResEventPermissions.Create;
        UpdatePolicyName = ResEventPermissions.Edit;
        DeletePolicyName = ResEventPermissions.Delete;
    }

    public override async Task<ResEventDto> CreateAsync(CreateResEventDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await EventRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new BusinessException("ResEvent:CodeExists")
                    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResEvent(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResEvent, ResEventDto>(entity);
    }

    public override async Task<ResEventDto> UpdateAsync(Guid id, UpdateResEventDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResEvent, ResEventDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive
        entity.UpdateStatus(ResEventStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResEvent>> CreateFilteredQueryAsync(GetResEventsInput input)
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

