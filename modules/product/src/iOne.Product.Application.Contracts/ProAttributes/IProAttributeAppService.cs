using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProAttributes;

public interface IProAttributeAppService : ICrudAppService<
    ProAttributeDto,
    Guid,
    GetProAttributesInput,
    CreateProAttributeDto,
    UpdateProAttributeDto>
{
}
