using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProTableRates;

public interface IProTableRateAppService : ICrudAppService<
    ProTableRateDto,
    Guid,
    GetProTableRatesInput,
    CreateProTableRateDto,
    UpdateProTableRateDto>
{
}
