using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProLineOfBusinesses;

public interface IProLineOfBusinessAppService : ICrudAppService<
    ProLineOfBusinessDto,
    Guid,
    GetProLineOfBusinessesInput,
    CreateProLineOfBusinessDto,
    UpdateProLineOfBusinessDto>
{
    Task<List<ProLineOfBusinessSelectDto>> GetSelectListAsync();
}




