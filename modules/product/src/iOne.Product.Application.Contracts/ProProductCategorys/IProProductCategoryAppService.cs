using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProProductCategorys;

public interface IProProductCategoryAppService : ICrudAppService<
    ProProductCategoryDto,
    Guid,
    GetProProductCategorysInput,
    CreateProProductCategoryDto,
    UpdateProProductCategoryDto>
{
}
