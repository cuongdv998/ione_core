using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResObjectTypes;

public interface IResObjectTypeAppService : ICrudAppService<
    ResObjectTypeDto,
    Guid,
    GetResObjectTypesInput,
    CreateResObjectTypeDto,
    UpdateResObjectTypeDto>
{
}

