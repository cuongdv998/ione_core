using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResUserDevices;
using iOne.ResUserDevices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResUserDevices;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResUserDevicePermissions.Default)]
public class ResUserDeviceAppService : CrudAppService<
    ResUserDevice,
    ResUserDeviceDto,
    Guid,
    GetResUserDevicesInput,
    CreateResUserDeviceDto,
    UpdateResUserDeviceDto>, IResUserDeviceAppService
{
    protected ResUserDeviceManager Manager { get; }
    protected IResUserDeviceRepository UserDeviceRepository { get; }

    public ResUserDeviceAppService(
        IResUserDeviceRepository repository,
        ResUserDeviceManager manager)
        : base(repository)
    {
        Manager = manager;
        UserDeviceRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResUserDevicePermissions.View;
        GetListPolicyName = ResUserDevicePermissions.View;
        CreatePolicyName = ResUserDevicePermissions.Create;
        UpdatePolicyName = ResUserDevicePermissions.Edit;
        DeletePolicyName = ResUserDevicePermissions.Delete;
    }

    public override async Task<ResUserDeviceDto> CreateAsync(CreateResUserDeviceDto input)
    {
        // Validate unique constraint: (UserName, DeviceUid, AppChannelCode)
        if (await UserDeviceRepository.IsUniqueExistsAsync(
            input.UserName,
            input.DeviceUid,
            input.AppChannelCode))
        {
            throw new BusinessException("Master:ResUserDevice:UniqueExists")
                .WithData("UserName", input.UserName)
                .WithData("DeviceUid", input.DeviceUid)
                .WithData("AppChannelCode", input.AppChannelCode);
        }

        var entity = new ResUserDevice(
            GuidGenerator.Create(),
            input.UserName,
            input.DeviceUid,
            input.DeviceToken,
            input.AppChannelCode,
            input.EffectDate,
            input.ExpirDate,
            input.Status,
            input.Os,
            input.DeviceName
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResUserDevice, ResUserDeviceDto>(entity);
    }

    public override async Task<ResUserDeviceDto> UpdateAsync(Guid id, UpdateResUserDeviceDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update DeviceToken, ExpirDate, Status, Os, DeviceName
        // KHÔNG update: UserName, DeviceUid, AppChannelCode, EffectDate
        await Manager.UpdateAsync(
            entity,
            input.DeviceToken,
            input.ExpirDate,
            input.Status,
            input.Os,
            input.DeviceName);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResUserDevice, ResUserDeviceDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive
        entity.UpdateStatus(ResUserDeviceStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResUserDevice>> CreateFilteredQueryAsync(GetResUserDevicesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by UserName (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.UserName))
        {
            query = query.Where(x => EF.Functions.ILike(x.UserName, $"%{input.UserName}%"));
        }

        // Filter by DeviceUid (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.DeviceUid))
        {
            query = query.Where(x => EF.Functions.ILike(x.DeviceUid, $"%{input.DeviceUid}%"));
        }

        // Filter by AppChannelCode (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.AppChannelCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.AppChannelCode, $"%{input.AppChannelCode}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}
