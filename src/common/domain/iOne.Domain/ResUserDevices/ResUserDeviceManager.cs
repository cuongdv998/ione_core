using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResUserDevices;

public class ResUserDeviceManager : DomainService
{
    protected IResUserDeviceRepository Repository { get; }

    public ResUserDeviceManager(IResUserDeviceRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResUserDevice resUserDevice)
    {
        // Check unique constraint: (UserName, DeviceUid, AppChannelCode)
        if (await Repository.IsUniqueExistsAsync(
            resUserDevice.UserName,
            resUserDevice.DeviceUid,
            resUserDevice.AppChannelCode))
        {
            throw new BusinessException("Master:ResUserDevice:UniqueExists")
                .WithData("UserName", resUserDevice.UserName)
                .WithData("DeviceUid", resUserDevice.DeviceUid)
                .WithData("AppChannelCode", resUserDevice.AppChannelCode);
        }

        await Repository.InsertAsync(resUserDevice);
    }

    public virtual async Task UpdateAsync(
        ResUserDevice resUserDevice,
        string deviceToken,
        DateTime? expirDate,
        ResUserDeviceStatus status,
        string? os,
        string? deviceName)
    {
        // ⚠️ QUAN TRỌNG: Chỉ update các trường được phép sửa
        // KHÔNG update: UserName, DeviceUid, AppChannelCode, EffectDate
        resUserDevice.UpdateDeviceToken(deviceToken);
        resUserDevice.UpdateExpirDate(expirDate);
        resUserDevice.UpdateStatus(status);
        resUserDevice.UpdateOs(os);
        resUserDevice.UpdateDeviceName(deviceName);
        await Repository.UpdateAsync(resUserDevice);
    }
}
