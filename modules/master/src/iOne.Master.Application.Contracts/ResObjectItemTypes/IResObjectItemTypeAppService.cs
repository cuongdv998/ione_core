using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResObjectItemTypes;

public interface IResObjectItemTypeAppService : ICrudAppService<
    ResObjectItemTypeDto,
    Guid,
    GetResObjectItemTypesInput,
    CreateResObjectItemTypeDto,
    UpdateResObjectItemTypeDto>
{
}
