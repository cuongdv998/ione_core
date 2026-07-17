using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResUoms;

public interface IResUomAppService : ICrudAppService<
    ResUomDto,
    Guid,
    GetResUomsInput,
    CreateResUomDto,
    UpdateResUomDto>
{
}
