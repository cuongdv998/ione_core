using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResPartnerTypes;

public interface IResPartnerTypeRepository : IRepository<ResPartnerType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code);
}

