using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResChannels;

public interface IResChannelRepository : IRepository<ResChannel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResChannel?> FindByCodeAsync(string code);
}

