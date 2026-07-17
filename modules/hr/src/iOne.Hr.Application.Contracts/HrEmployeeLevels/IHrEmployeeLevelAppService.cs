using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployeeLevels;

public interface IHrEmployeeLevelAppService : ICrudAppService<
    HrEmployeeLevelDto,
    Guid,
    GetHrEmployeeLevelsInput,
    CreateHrEmployeeLevelDto,
    UpdateHrEmployeeLevelDto>
{
}

