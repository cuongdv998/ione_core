using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployeePositions;

public interface IHrEmployeePositionAppService : ICrudAppService<
    HrEmployeePositionDto,
    Guid,
    GetHrEmployeePositionsInput,
    CreateHrEmployeePositionDto,
    UpdateHrEmployeePositionDto>
{
}

