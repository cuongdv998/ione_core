using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResUomClasses;

public interface IResUomClassAppService : ICrudAppService<
    ResUomClassDto,
    Guid,
    GetResUomClassesInput,
    CreateResUomClassDto,
    UpdateResUomClassDto>
{
    Task<List<ResUomClassSelectDto>> GetSelectListAsync();
}

