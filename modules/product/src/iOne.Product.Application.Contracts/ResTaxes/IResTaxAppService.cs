using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ResTaxes;

public interface IResTaxAppService : ICrudAppService<
    ResTaxDto,
    Guid,
    GetResTaxesInput,
    CreateResTaxDto,
    UpdateResTaxDto>
{
}

