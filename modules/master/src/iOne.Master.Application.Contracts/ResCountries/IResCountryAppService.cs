using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCountries;

public interface IResCountryAppService : ICrudAppService<
    ResCountryDto,
    Guid,
    GetResCountriesInput,
    CreateResCountryDto,
    UpdateResCountryDto>
{
}

