using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResPartners;

public interface IResPartnerRepository : IRepository<ResPartner, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> AnyByProvinceIdAsync(Guid provinceId);
    Task<bool> AnyByWardIdAsync(Guid wardId);
    Task<ResPartner?> FindByCodeAsync(string code);
}

