using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResAppChannels;

public interface IResAppChannelRepository : IRepository<ResAppChannel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
