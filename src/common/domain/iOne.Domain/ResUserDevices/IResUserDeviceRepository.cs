using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResUserDevices;

public interface IResUserDeviceRepository : IRepository<ResUserDevice, Guid>
{
    Task<bool> IsUniqueExistsAsync(string userName, string deviceUid, string appChannelCode, Guid? excludeId = null);
}
