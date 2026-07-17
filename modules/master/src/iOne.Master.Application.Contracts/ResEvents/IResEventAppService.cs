using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResEvents;

public interface IResEventAppService : ICrudAppService<
    ResEventDto,
    Guid,
    GetResEventsInput,
    CreateResEventDto,
    UpdateResEventDto>
{
}

