using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using iOne.Partner.ResChannels;
using iOne.ResChannels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResChannels;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResChannelPermissions.Default)]
public class ResChannelAppService : CrudAppService<
    ResChannel,
    ResChannelDto,
    Guid,
    GetResChannelsInput,
    CreateResChannelDto,
    UpdateResChannelDto>,
    IResChannelAppService
{
    protected ResChannelManager Manager { get; }

    public ResChannelAppService(
        IResChannelRepository repository,
        ResChannelManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResChannelPermissions.View;
        GetListPolicyName = ResChannelPermissions.View;
        CreatePolicyName = ResChannelPermissions.Create;
        UpdatePolicyName = ResChannelPermissions.Edit;
        DeletePolicyName = ResChannelPermissions.Delete;
    }

    public override async Task<ResChannelDto> CreateAsync(CreateResChannelDto input)
    {
        var entity = new ResChannel(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResChannel, ResChannelDto>(entity);
    }

    public override async Task<ResChannelDto> UpdateAsync(Guid id, UpdateResChannelDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Status, Description - không update Code
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Status,
            input.Description
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResChannel, ResChannelDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResChannelStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResChannel>> CreateFilteredQueryAsync(GetResChannelsInput input)
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

