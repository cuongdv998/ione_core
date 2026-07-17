using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarLines;

public interface IResCarLineAppService : ICrudAppService<
    ResCarLineDto,
    Guid,
    GetResCarLinesInput,
    CreateResCarLineDto,
    UpdateResCarLineDto>
{
}


