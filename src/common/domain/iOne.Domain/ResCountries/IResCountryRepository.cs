using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCountries;

public interface IResCountryRepository : IRepository<ResCountry, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

