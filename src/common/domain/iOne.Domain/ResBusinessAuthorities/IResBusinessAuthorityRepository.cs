using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResBusinessAuthorities;

public interface IResBusinessAuthorityRepository : IRepository<ResBusinessAuthority, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
