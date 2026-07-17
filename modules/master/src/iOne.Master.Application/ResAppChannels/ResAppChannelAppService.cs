using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResAppChannels;
using iOne.ResAppChannels; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResAppChannels;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResAppChannelPermissions.Default)]
public class ResAppChannelAppService : CrudAppService<
    ResAppChannel,
    ResAppChannelDto,
    Guid,
    GetResAppChannelsInput,
    CreateResAppChannelDto,
    UpdateResAppChannelDto>, IResAppChannelAppService
{
    protected ResAppChannelManager Manager { get; }
    protected IResAppChannelRepository AppChannelRepository { get; }

    public ResAppChannelAppService(
        IResAppChannelRepository repository,
        ResAppChannelManager manager)
        : base(repository)
    {
        Manager = manager;
        AppChannelRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResAppChannelPermissions.View;
        GetListPolicyName = ResAppChannelPermissions.View;
        CreatePolicyName = ResAppChannelPermissions.Create;
        UpdatePolicyName = ResAppChannelPermissions.Edit;
        DeletePolicyName = ResAppChannelPermissions.Delete;
    }

    public override async Task<ResAppChannelDto> CreateAsync(CreateResAppChannelDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await AppChannelRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResAppChannel:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResAppChannel:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResAppChannel(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status,
            input.Type
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResAppChannel, ResAppChannelDto>(entity);
    }

    public override async Task<ResAppChannelDto> UpdateAsync(Guid id, UpdateResAppChannelDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description, Status và Type, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status, input.Type);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResAppChannel, ResAppChannelDto>(entity!);
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
        entity!.UpdateStatus(ResAppChannelStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResAppChannel>> CreateFilteredQueryAsync(GetResAppChannelsInput input)
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

        // Filter by Type
        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
        }

        return query;
    }
}
