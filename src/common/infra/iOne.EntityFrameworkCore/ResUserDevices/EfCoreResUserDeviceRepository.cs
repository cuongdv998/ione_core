using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResUserDevices;

namespace iOne.EntityFrameworkCore.ResUserDevices;

public class EfCoreResUserDeviceRepository : EfCoreRepository<iOneDbContext, ResUserDevice, Guid>, IResUserDeviceRepository
{
    public EfCoreResUserDeviceRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsUniqueExistsAsync(string userName, string deviceUid, string appChannelCode, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => 
            x.UserName == userName && 
            x.DeviceUid == deviceUid && 
            x.AppChannelCode == appChannelCode);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
