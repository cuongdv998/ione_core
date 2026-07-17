using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProProductTypes;

public interface IProProductTypeAppService : ICrudAppService<
    ProProductTypeDto,
    Guid,
    GetProProductTypesInput,
    CreateProProductTypeDto,
    UpdateProProductTypeDto>
{
}
