using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResFeeItems;

public interface IResFeeItemAppService : ICrudAppService<
    ResFeeItemDto,
    Guid,
    GetResFeeItemsInput,
    CreateResFeeItemDto,
    UpdateResFeeItemDto>
{
}
